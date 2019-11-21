using System;
using Ultrahaptics;
using UnityEngine;
using Vector3 = Ultrahaptics.Vector3;

/// <summary>
/// This script will generate a 'Circle' haptic, whose draw frequency and intensity is modulated based on values
/// calculated in the HapticRenderer script. Separate hand positions can be used for each hand in the scene. These positions
/// can be different to the scanning location set within HapticRenderer. Currently, the only sensation available is 
/// a 'Circle' haptic, however its radius can be altered in the Inspector window.
/// </summary>
public class HapticRunner : MonoBehaviour
{
    public static HapticCircle[] Circles { get; set; }

    [SerializeField]
    private UnityEngine.Transform[] HandPositions = new UnityEngine.Transform[2];

    [SerializeField]
    private float _circleRadius;

    private static TimePointStreamingEmitter _emitter;
    private static bool _firstTime = true;
    private static double _startTime;

    public void Start()
    {
        Circles = new HapticCircle[HandPositions.Length];

        for (int i = 0; i < Circles.Length; i++)
        {
            Circles[i] = new HapticCircle()
            {
                Position = new Vector3(0, 0, 0),
                Radius = _circleRadius,
            };
        }

        // Create a timepoint streaming emitter
        // Note that this automatically attempts to connect to the device, if present
        _emitter = new TimePointStreamingEmitter();

        // Inform the SDK how many control points you intend to use
        // This also calculates the resulting sample rate in Hz, which is returned to the user
        uint sample_rate = _emitter.setMaximumControlPointCount((uint)HandPositions.Length);
        _emitter.setExtendedOption("setFilterCutoffFrequencies", "0 0 0 0");

        // Set our callback to be called each time the device is ready for new points
        _emitter.setEmissionCallback(Callback, null);

        // Instruct the device to call our callback and begin emitting
        bool started = _emitter.start();

        if (!started)
        {
            // We couldn't use the emitter, so exit immediately
            Console.WriteLine("Could not start emitter.");
        }
    }

    public void Update()
    {
        for (int i = 0; i < Circles.Length; i++)
        {
            Circles[i].SetPosition(HandPositions[i]);
        }
    }

    private void OnDestroy()
    {
        // Dispose/destroy the emitter
        _emitter.stop();
        _emitter.Dispose();
        _emitter = null;
    }

    // This callback is called every time the device is ready to accept new control point information
    private static void Callback(TimePointStreamingEmitter emitter, OutputInterval interval, TimePoint deadline, object userObj)
    {
        // For each time point in this interval...
        foreach (TimePointOnOutputInterval tPoint in interval)
        {
            if (_firstTime)
            {
                _startTime = tPoint.seconds();
                _firstTime = false;
            }
            double t = tPoint.seconds() - _startTime;

            for (int i = 0; i < Circles.Length; i++)
            {
                Vector3 pos = Circles[i].EvaluateAt(t);
                tPoint.persistentControlPoint(i).setPosition(pos);
                tPoint.persistentControlPoint(i).setIntensity(Circles[i].Intensity);
            }
        }
    }
}