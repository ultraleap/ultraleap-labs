using Leap;
using Leap.Unity;
using UnityEngine;

/// <summary>
/// This script will capture Leap Motion Controller information related to hand locations per frame for both left and right hands.
/// </summary>

public class HandData : MonoBehaviour
{
    public static Hand[] TwoHandData { get; set; }

    [SerializeField]
    private LeapProvider _leapProvider;

    private Hand _emptyHand = new Hand();

    private void OnEnable()
    {
        TwoHandData = new Hand[2] { _emptyHand, _emptyHand };

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
        if (_leapProvider != null)
        {
            _leapProvider.OnUpdateFrame -= LeapProviderOnUpdateFrame;
        }
    }

    private void LeapProviderOnUpdateFrame(Frame frame)
    {
        if (frame.Hands.Count <= 0)
        {
            TwoHandData[0] = _emptyHand;
            TwoHandData[1] = _emptyHand;
            return;
        }

        if (Hands.Left == null)
        {
            TwoHandData[0] = _emptyHand;
        }
        else
        {
            TwoHandData[0] = Hands.Left;
        }

        if (Hands.Right == null)
        {
            TwoHandData[1] = _emptyHand;
        }
        else
        {
            TwoHandData[1] = Hands.Right;
        }
    }
}
