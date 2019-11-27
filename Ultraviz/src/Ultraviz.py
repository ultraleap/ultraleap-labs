#!/usr/bin/env python3
# Ultraviz - An Ultrahaptics Contorl Point Visualizer built with PyQt which plots control points in 3D space
# Dependencies:
# -Python 3.7.x (http://python.org)
# -The following Python modules are required: pyqt5 pyqtgraph numpy PyOpenGL atom SimpleWebSocketServer qtmodern
#   The recommended way to install these via pip (for Python 3)
#   To install with pip3 run this command:
#     $ pip3 install pyqt5 pyqtgraph numpy PyOpenGL atom SimpleWebSocketServer qtmodern
# On Windows, pywin32 is also required.
import sys
import os
import re
import threading
import argparse
import platform
from subprocess import Popen
import json

# To apply dark style and modern window appearance
import qtmodern
from qtmodern.styles import dark as darkMode
import qtmodern.windows

from pybuild import setupPyInstallerBuild
setupPyInstallerBuild()

IS_WINDOWS = platform.system().lower() == "windows"
IS_UNIX = platform.system().lower() in ("darwin", "linux", "mac")

from log_handler import SDKLogPipeHandler
from bookmarks import BookmarksManager
from ui import UHSDKLogViewer
from websocket import createWebSocketServer, get_clients, socketIsOpen

try:
    from PyQt5.QtWidgets import *
    from PyQt5.QtGui import QIcon
    from PyQt5.QtCore import Qt
except Exception as e:
    print("Exception on thirdparty import: " + str(e))
    if IS_WINDOWS:
        print("*** WARNING: Unable to import dependencies. Please install via:\n\n pip3 install --user pyqt5 pyqtgraph numpy PyOpenGL atom SimpleWebSocketServer qtmodern pywin32\n")
    else:
        print("*** WARNING: Unable to import dependencies. Please install via:\n\n pip3 install --user pyqt5 pyqtgraph numpy PyOpenGL atom SimpleWebSocketServe qtmodern\n")

# For Qt qrc file loading of resources
import resources

class MainWindow(QMainWindow):
    def __init__(self, exe_path=None, auto_launch=True, parent = None):
        super(MainWindow, self).__init__(parent)

        if exe_path:
            print("An executable process was provided: %s" % exe_path)
            self.exePath = exe_path

        self.log_reader_thread = None
        self.executable_process = None

        # An Optional WebSocket, to serve control point data
        self.webSocket = None
        self.webSocketActive = False

        self.processingSDKLog = False
        self.my_env = None

        self.statusBar = QStatusBar()
        self.openProcessButton = QPushButton("")
        self.openProcessButton.setIcon(QIcon(":/icons/open.png"))
        self.openProcessButton.setToolTip("Open Process")
        self.statusBar.addWidget(self.openProcessButton)
        self.setStatusBar(self.statusBar)

        self.bookmarksManager = BookmarksManager()
        self.items = QDockWidget("Bookmarks", self)
        self.bookmarkListWidget = QListWidget()
        self.bookmarkListWidget.itemDoubleClicked.connect(self.bookmarkDoubleClicked)
        self.items.setWidget(self.bookmarkListWidget)
        self.items.setFloating(False)

        self.viewer = UHSDKLogViewer(exe_path=exe_path, auto_launch=auto_launch)
        self.setCentralWidget(self.viewer)
        self.addDockWidget(Qt.BottomDockWidgetArea, self.items)

        # MenuBar actions
        self.openProcessAction = QAction("Open Process", self)
        self.openProcessAction.setShortcut("Ctrl+O")
        self.openProcessAction.triggered.connect(self.launchProcessFromFileDialog)
        
        self.openProcessButton.clicked.connect(self.launchProcessFromFileDialog)       

        self.clearBookmarksAction = QAction("Clear Bookmarks", self)
        self.clearBookmarksAction.setShortcut("Ctrl+X")
        self.clearBookmarksAction.triggered.connect(self.clearBookmarksAndUpdate)

        self.exitAction = QAction("Shutdown", self)
        self.exitAction.setShortcut("Esc")
        self.exitAction.triggered.connect(self.shutDown)

        self.quit_action = QAction("Exit", self)
        self.quit_action.triggered.connect(self.shutDown)

        self.toggleVisualizer_action = QAction("Hide Visualizer", self)
        self.toggleVisualizer_action.triggered.connect(self.toggleVisualizerShown)

        self.webSocket_enableDisable_action = QAction("Enable Web Socket", self)
        self.webSocket_enableDisable_action.triggered.connect(self.toggleWebSocketEnabled)

        # Init QSystemTrayIcon
        self.tray_icon = QSystemTrayIcon(self)
        if IS_WINDOWS:
            iconPath = ":/icons/uh_tray_win.png"
        else:
            iconPath = ":/icons/uh_tray.png"
        self.tray_icon.setIcon(QIcon(iconPath))
        self.setWindowIcon(QIcon(":/icons/uh_tray.png"))

        tray_menu = QMenu()
        tray_menu.addAction(self.openProcessAction)
        tray_menu.addAction(self.toggleVisualizer_action)
        tray_menu.addAction(self.webSocket_enableDisable_action)
        tray_menu.addAction(self.clearBookmarksAction)
        tray_menu.addAction(self.quit_action)
        self.tray_icon.setContextMenu(tray_menu)
        self.tray_icon.show()                

        # Set up an empty log file location
        self.setEnvironmentForLogging()
        self.startPollingLogReaderThread()

        # Optionally launch an executable on launch of the dialog
        if auto_launch:
            self.launchExecutable()

        # Setup the bookmarks list
        self.updateBookmarkList()

    def logMessage(self, msg):
        print(msg)
        self.statusBar.showMessage(msg, 2000)

    def toggleVisualizerShown(self):
        if self.isHidden():
            self.show()
            self.toggleVisualizer_action.setText("Hide Visualizer")
        else:
            self.hide()
            self.toggleVisualizer_action.setText("Show Visualizer")

    def launchProcessFromFileDialog(self):
        dialog = QFileDialog()
        fname = dialog.getOpenFileName(None, 'Open Ultrahaptics Process', '.', '*',    '*', QFileDialog.DontUseNativeDialog)
        if os.path.isfile(fname[0]):
            if self.executable_process:
                self.killMonitoredProcess()
            self.exePath = fname[0]
            self.bookmarksManager.addNewBookmark(self.exePath)
            self.updateBookmarkList()
            self.launchExecutable(ask=True)

    def killMonitoredProcess(self):
        try:
            self.executable_process.kill()
        except Exception as e:
            print(e)
            print("Unable to kill the executable process: " + str(self.executable_process))

    def updateBookmarkList(self):
        self.bookmarkListWidget.clear()
        for bookmark in self.bookmarksManager.getBookmarks():
            self.bookmarkListWidget.addItem(bookmark)

    def clearBookmarksAndUpdate(self):
        self.bookmarksManager.clearBookmarks()
        self.updateBookmarkList()

    def bookmarkDoubleClicked(self, value):
        self.exePath = value.text()
        self.launchExecutable(ask=True)

    def shutDown(self):
        if self.executable_process:
            msg = QMessageBox()
            msg.setIcon(QMessageBox.Information)
            msg.setText("Closing...")
            msg.setInformativeText("Do you want to kill your monitored process?")
            msg.setWindowTitle("Quit")
            msg.setStandardButtons(QMessageBox.Yes | QMessageBox.No)
            retval = msg.exec_()
            if retval == QMessageBox.No:
                return
            else:
                self.killMonitoredProcess()

        sys.exit(app.exec_())

    def closeEvent(self, event):
        self.shutDown()
        return QMainWindow.closeEvent(self, event)

    def launchExecutable(self, ask=False):
        self.logMessage("Launching: %s" % (self.exePath))

        if not os.path.isfile(self.exePath):
            self.logMessage("WARNING: Unable to launch: (%s) - check path exists and is executable" % self.exePath)
            return

        exe_root = os.path.dirname(self.exePath)

        if not ask:
            self.executable_process = Popen([self.exePath], env=self.my_env, cwd=exe_root)
        else:
            msg = QMessageBox()
            msg.setIcon(QMessageBox.Information)
            msg.setText("Launching:\n%s" % self.exePath)
            msg.setInformativeText("Do you want to start monitoring this App?");
            msg.setStandardButtons(QMessageBox.Yes | QMessageBox.No)
            retval = msg.exec_()
            if retval == QMessageBox.No:
                return
            else:
                try:
                    self.executable_process = Popen([self.exePath], env=self.my_env, cwd=exe_root)
                except:
                    self.logMessage("Unable to kill the process: " + str(self.exePath))            

    def setEnvironmentForLogging(self):
        self.logHandler = SDKLogPipeHandler(is_windows=IS_WINDOWS)
        self.logHandler.setupNamedPipe()        
        os.environ["UH_LOG_LEVEL"] = "4"
        os.environ["UH_LOG_DEST"] = self.logHandler.pipe_name
        os.environ["UH_LOG_LEVEL_FORCE"] = "1"
        os.environ["UH_LOG_DEST_FORCE"] = "1"

        # Store a copy of the environment, so it can be passed to the subprocess
        self.my_env = os.environ.copy()

    # For serving control point data over websocket
    def serveControlPoints(self, data):
        if self.webSocketActive:
            for client in get_clients():
                if data:
                    msg = json.dumps({'x': data[1], 'y': data[2], 'z': data[3]})
                    client.sendMessage(str(msg))

    # Method for thread to process the Log on Unix - consider moving to SDKLogHandler Class
    def processLogUnix(self):
        with open(self.logHandler.pipe_name) as fifo:
            while self.processingSDKLog:
                try:               
                    data = fifo.readline()
                    match = re.search(self.logHandler.xyzi_regex, data)
                    self.viewer.setControlPointsFromFromRegexMatch(match)
                    self.serveControlPoints(match)
                except Exception as e:
                    print (e)

    # Methof for thread to process the Log on Windows - consider moving to SDKLogHandler Class
    def processLogWindows(self):
        while self.processingSDKLog:
            if not self.logHandler.namedPipe:
                self.logHandler.setupNamedPipe()
                self.logHandler.connectToSDKPipe()
            try:
                data = self.logHandler.getDataFromNamedPipe()
            except Exception as e:
                print ("Errors processing log on Windows: " + str(e))
                self.logHandler.namedPipe = None
                continue

            if len(data)<2:
                self.logMessage("No valid Pipe data available")
                continue

            lines = str(data[1], "utf-8").split(os.linesep)

            for line in lines:
                match = re.search(self.logHandler.xyzi_regex, line)
                self.viewer.setControlPointsFromFromRegexMatch(match)
                self.serveControlPoints(match)

    def startPollingLogReaderThread(self):
        if IS_UNIX:
            self.log_reader_thread = threading.Thread(target=self.processLogUnix)
        elif IS_WINDOWS:
            self.log_reader_thread = threading.Thread(target=self.processLogWindows)

        self.log_reader_thread.daemon = True
        self.processingSDKLog = True
        self.log_reader_thread.start()

    def stopPollingLogReaderThread(self):
        self.processingSDKLog = False
        if self.log_reader_thread.is_alive():
            # Fix this - it will quit the process!
            self.log_reader_thread.join()

    def toggleWebSocketEnabled(self):
        if not self.webSocketActive:
            self.startWebSocketServerThread()
        else:
            self.stopWebSocketServerThread()

    def startWebSocketServerThread(self):
        self.logMessage("Starting WebSocket Server")
        try:
            if not socketIsOpen():
                self.webSocket = createWebSocketServer()
                self.webSocketThread = threading.Thread(target=self.webSocket.serveforever)
                self.webSocketThread.daemon = True
                self.webSocketThread.start()
                self.webSocketActive = True
            else:
                self.logMessage("SOCKET PORT ALREADY OPEN.")
                self.webSocketActive = True
            self.webSocket_enableDisable_action.setText("Disable Web Socket")
        except Exception as e:
            print(e)


    def stopWebSocketServerThread(self):
        self.logMessage("Stopping Server")
        if self.webSocket:
            try:
                # TODO: Reliably close the Socket
                self.webSocketThread.join(0.25)
            except Exception as e:
                print("Closing, exception:" + str(e))                
            self.webSocketActive = False
            self.webSocket_enableDisable_action.setText("Enable Web Socket")

    def toggleProcessingLog(self):
        self.processingSDKLog = not self.processingSDKLog
        if not self.processingSDKLog:
            self.stopPollingLogReaderThread()
        else:
            self.startPollingLogReaderThread()


if __name__ == '__main__':
    app = QApplication(sys.argv)

    app.setOrganizationName("Ultraleap");
    app.setOrganizationDomain("com.ultraleap");
    app.setApplicationName("Ultraleap Visualizer");
    app.setQuitOnLastWindowClosed(False)

    parser = argparse.ArgumentParser(usage="-e <executable path> -a <add to automatically launch the executable>")
    parser.add_argument('-e', '--exePath', required=False, help='The executable process to lauch. If specified, the specified executable will be launched and monitored.')
    parser.add_argument('-a', '--autoLaunch', action="store_true", default=False, required=False, help='If specified, will automatically launch the specified executable on launch.')
    args = parser.parse_args()

    exePath = args.exePath
    autoLaunch = args.autoLaunch
    
    ex = MainWindow(exe_path = exePath, auto_launch = autoLaunch)
    darkMode(app)

    # TODO: Understand why qtmodern.ModernWindow renders differently on macOS/Windows
    if not IS_WINDOWS:
        mw = qtmodern.windows.ModernWindow(ex)
        mw.setWindowTitle("Ultraviz")
        mw.show()
    else:
        ex.setWindowTitle("Ultraviz")
        ex.show()
    sys.exit(app.exec_())
