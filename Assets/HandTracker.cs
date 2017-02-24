using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class HandTracker : MonoBehaviour {
    public bool isRightHand = true;

    private LeapProvider lp;

    private void Start() {
        lp = FindObjectOfType<LeapProvider>();
    }

	void Update () {
        Frame frame = lp.CurrentFrame;

        foreach(Hand hand in frame.Hands) {
            if (hand.IsRight && isRightHand) {
                transform.position = hand.PalmPosition.ToVector3();
            }
            else if (hand.IsLeft && !isRightHand) {
                transform.position = hand.PalmPosition.ToVector3();
            }
        }
	}
}
