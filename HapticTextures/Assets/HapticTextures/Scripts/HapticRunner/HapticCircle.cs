using System;
using Ultrahaptics;

/* This script contains the parameters and calculations for the generation of a "Circle" haptic sensation.
The available params are set within the HapticRunner and HapticRenderer scripts. */

public class HapticCircle
{
    public Vector3 Position { get; set; }
    public float Intensity { get; set; }
    public float Radius { get; set; }
    public float Frequency { get; set; }

    public Vector3 EvaluateAt(double seconds)
    {
        Vector3 result = new Vector3
        (
            // Calculate the x and y positions of the circle and set the height
            (float)(Math.Cos(2 * Math.PI * Frequency * seconds) * Radius) * (float)Units.cm,
            (float)(Math.Sin(2 * Math.PI * Frequency * seconds) * Radius) * (float)Units.cm,
            0
        );

        result.x += Position.x;
        result.y += Position.y;
        result.z = Position.z;
        return result;
    }

    internal void SetPosition(UnityEngine.Transform position1)
    {
        Position = new Vector3
            (
                position1.position.x,
                position1.position.z,
                position1.position.y
            );
    }
}
