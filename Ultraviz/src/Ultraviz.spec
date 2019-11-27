# -*- mode: python ; coding: utf-8 -*-

block_cipher = None


a = Analysis(['Ultraviz.py'],
             pathex=['/Users/antony.nasce/workspace/ultrahaptics-labs-internal/Ultraviz/src'],
             binaries=[],
             datas=[('/Library/Frameworks/Python.framework/Versions/3.7/lib/python3.7/site-packages/qtmodern/resources/frameless.qss', 'qtmodern/resources'), ('/Library/Frameworks/Python.framework/Versions/3.7/lib/python3.7/site-packages/qtmodern/resources/style.qss', 'qtmodern/resources')],
             hiddenimports=[],
             hookspath=[],
             runtime_hooks=[],
             excludes=[],
             win_no_prefer_redirects=False,
             win_private_assemblies=False,
             cipher=block_cipher,
             noarchive=False)
pyz = PYZ(a.pure, a.zipped_data,
             cipher=block_cipher)
exe = EXE(pyz,
          a.scripts,
          a.binaries,
          a.zipfiles,
          a.datas,
          [],
          name='Ultraviz',
          debug=False,
          bootloader_ignore_signals=False,
          strip=False,
          upx=True,
          upx_exclude=[],
          runtime_tmpdir=None,
          console=False , icon='../icons/icon.icns')
app = BUNDLE(exe,
             name='Ultraviz.app',
             icon='../icons/icon.icns',
             bundle_identifier=None)
