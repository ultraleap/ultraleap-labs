using Ultrahaptics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// This script will first check to see which array is currently plugged in to the pc. 
/// It will then adjust the positioning of the Lead Controller origin depending on whether 
/// it has found an Ultrahaptics Stratos Inspire or Explore haptics array.
/// </summary>

public class ArrayAligner : MonoBehaviour
{
    private const string USI = "USI";
    private const string USX = "USX";

    [SerializeField]
    private GameObject _leapHandController;

    private string[] _deviceInfo;
    private UltrahapticsLibrary _UHLibrary;

    private void Awake()
    {
        _UHLibrary = new UltrahapticsLibrary();
        _deviceInfo = _UHLibrary.getDeviceIdentifiers();

        if(_deviceInfo.Length == 0)
        {
            Debug.LogWarning("No array detected");
            return;
        }

        if (_deviceInfo[0].Contains(USI))
        {
            _leapHandController.transform.position = new Vector3(0, -0.00006f, -0.089f);
        }

        if (_deviceInfo[0].Contains(USX))
        {
            _leapHandController.transform.position = new Vector3(0, 0, 0.121f);
        }
    }
}
