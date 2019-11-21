using UnityEngine;
using System.Collections.Generic;
using Leap;
using Leap.Unity;

/// <summary>
/// This script will evaluate a particular texture within a Unity scene provided it has been hit from each 
/// raycast projected from the provided hand scanning position.This process will be done independently for each hand
/// in the scene. For each texture, its height map is evaluated and an intensity value is calculated and sent to the
/// HapticRunner script in order to set the appropriate values for each haptic sensation.Additional modulation can be
/// applied that utilises a user's hand velocity to modulate both haptic sensation draw frequency and intensity.
/// </summary>

public class HapticRenderer : MonoBehaviour
{

    [System.Serializable]
    public class TextureHandProperties
    {
        public Chirality chirality;
        public Transform scanPosition = null;
        public RaycastHit handRayResult;
        [ReadOnly] public Vector3 handVelocity = Vector3.zero;
        [ReadOnly] public float handMagnitude = 0f;
    }

    [SerializeField]
    private LeapProvider _leapProvider = null;

    [SerializeField]
    private List<TextureHandProperties> _hands = new List<TextureHandProperties>();

    [SerializeField]
    private float _handVelocityThreshold, _raycastLengthDown;

    [SerializeField]
    private AnimationCurve _frequencyCurve, _intensityCurve;

    [SerializeField]
    private bool _modulateIntensityByHandVelocity = false, _modulateFrequencyByHandVelocity = false, _alwaysOn = false;

    private float _currentHandVelocityThreshold;

    private void OnEnable()
    {
        if (_leapProvider == null)
        {
            _leapProvider = Hands.Provider;
        }
        if (_leapProvider != null)
        {
            _leapProvider.OnUpdateFrame += LeapProviderOnUpdateFrame;
        }
    }

    private void OnDisable()
    {
        if(_leapProvider != null)
        {
            _leapProvider.OnUpdateFrame -= LeapProviderOnUpdateFrame;
        }
    }

    void LeapProviderOnUpdateFrame(Frame frame)
    {
        foreach (TextureHandProperties hand in _hands)
        {
            UpdateHand(hand);
        }
    }

    private void UpdateHand(TextureHandProperties _hand)
    {
        _hand.handVelocity = HandData.TwoHandData[(int)_hand.chirality].PalmVelocity.ToVector3();
        _hand.handMagnitude = _hand.handVelocity.magnitude;
        CalcScanFeatures(_hand, RaycastToTexture(_hand.scanPosition, _raycastLengthDown, out _hand.handRayResult));

    }

    private void CalcScanFeatures(TextureHandProperties _hand, TextureAttributes _attribute)
    {
        if (_alwaysOn)
        {
            _currentHandVelocityThreshold = 0;
        }
        else
        {
            _currentHandVelocityThreshold = _handVelocityThreshold;
        }
        if (_attribute != null && _hand.handMagnitude > _currentHandVelocityThreshold)
        {
                if (_modulateIntensityByHandVelocity)
                {
                    HapticRunner.Circles[(int)_hand.chirality].Intensity = HeightValue(
                                _hand.handRayResult.textureCoord,
                                _attribute.HeightMap,
                                _attribute.MinIntensity,
                                _attribute.MaxIntensity * GetModulatedIntensityByHandVelocity(_hand.handMagnitude));
                }
                else
                {
                    HapticRunner.Circles[(int)_hand.chirality].Intensity = HeightValue(
                                _hand.handRayResult.textureCoord,
                                _attribute.HeightMap,
                                _attribute.MinIntensity,
                                _attribute.MaxIntensity);
                }
                if (_modulateFrequencyByHandVelocity)
                {
                    HapticRunner.Circles[(int)_hand.chirality].Frequency = _attribute.Smoothness + ((_attribute.Smoothness / 2) * GetModulatedFrequencyByHandVelocity(_hand.handMagnitude));
                }
                else
                {
                    HapticRunner.Circles[(int)_hand.chirality].Frequency = _attribute.Smoothness;
                }
        }
        else
        {
            HapticRunner.Circles[(int)_hand.chirality].Intensity = 0;
        }
    }

    private float GetModulatedFrequencyByHandVelocity(float _velocity)
    {
        if (_velocity > _handVelocityThreshold)
        {
            return _frequencyCurve.Evaluate(_velocity);
        }
        return 0f;
    }    
    
    private float GetModulatedIntensityByHandVelocity(float _velocity)
    {
        if (_velocity > _handVelocityThreshold)
        {
            return _intensityCurve.Evaluate(_velocity);
        }
        return 0f;
    }

    private TextureAttributes RaycastToTexture(Transform _transform, float _rayDistance, out RaycastHit _raycastHit)
    {
        TextureAttributes ta = null;
        if (Physics.Raycast(_transform.position, -_transform.up, out _raycastHit, _rayDistance))
        {
            if (_raycastHit.transform != null)
            {
                ta = _raycastHit.transform.GetComponent<TextureAttributes>();
                if (ta != null)
                {
                    return ta;
                }
            }
        }
        return ta;
    }

    private float HeightValue(Vector2 textureLocation, Texture2D heightMap, float minInt, float maxInt)
    {
        return (heightMap.GetPixel(
            (int)(textureLocation.x * heightMap.width),
            (int)(textureLocation.y * heightMap.height)
            ).grayscale * (maxInt - minInt)) + minInt;
    }
}
