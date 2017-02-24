using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class PlayerController : MonoBehaviour {
    public Camera camera;
    public GameObject vehicle;
    public GameObject handTracker;
    public float speed = 30f;
    public float rotateSpeed = 100f;

    public UnityEngine.UI.Image wastedText;
    public AudioSource wastedSound;
    public AudioSource airhorn;
    public UnityEngine.UI.Image warmer;
    public UnityEngine.UI.Image colder;
    public ColorCorrectionCurves colorCurve;
    private const float MIN_SATURATION = 0.35f;

    private LeapProvider lp;
    private Hand leftHand;
    private Hand rightHand;

    private bool moving = false;
    private Vector3 startHandPos;

    public float dieDuration = 5f;
    public float dieFade = 1f;
    private bool isDying = false;
    private float startTime;

    private bool lightOn = true;
    private float distanceToCheckpoint=0;
    private float flashRate;
    private int lightCounter=0;

    private bool hasPlayedDeath = false;
    private bool hasPlayedDoubleFlipOff = false;


    void Start () {
        lp = FindObjectOfType<LeapProvider>();
        warmer.gameObject.SetActive(false);
        colder.gameObject.SetActive(false);
    }

    void Update () {
        leftHand = null;
        rightHand = null;

        Frame frame = lp.CurrentFrame;
        if (frame.Hands.Count > 0) {
            foreach (Hand hand in frame.Hands) {
                if (hand.IsLeft) {
                    leftHand = hand;
                }
                else if (hand.IsRight) {
                    rightHand = hand;
                }
            }
        }

        bool doubleFlipOff = true;
        if (rightHand != null) {
            bool flipOff = true;
            /*
            foreach (Finger finger in rightHand.Fingers) {
                if (finger.Type == Finger.FingerType.TYPE_MIDDLE && !finger.IsExtended) {
                    flipOff = false;
                }
                else if (finger.Type != Finger.FingerType.TYPE_MIDDLE && finger.IsExtended) {
                    flipOff = false;
                }
            }
            */
            foreach (Finger finger in rightHand.Fingers)
            {
                if (finger.Type == Finger.FingerType.TYPE_MIDDLE && !finger.IsExtended)
                {
                    flipOff = false;
                }
                else if (finger.Type != Finger.FingerType.TYPE_MIDDLE && finger.IsExtended)
                {
                    flipOff = false;
                }
            }
            if (!flipOff)
            {
                doubleFlipOff = false;
                hasPlayedDoubleFlipOff = false;
            }

            if (flipOff && RaceManager.Instance.raceStarted) {
                Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
                Vector3 localHandPos = handTracker.transform.localPosition;
                if (!moving) {
                    moving = true;
                    startHandPos = localHandPos;
                }

                float deltaX = localHandPos.x - startHandPos.x;
                float deltaY = localHandPos.y - startHandPos.y;
                transform.Rotate(0, rotateSpeed * deltaX * Time.deltaTime, 0);

                transform.Translate(localForward * speed * Time.deltaTime + new Vector3(0, deltaY * 3f * speed * Time.deltaTime, 0));
            }
            else if (flipOff && !RaceManager.Instance.countdownStarted) {
                RaceManager.startCountdown();
            }
        }
        else doubleFlipOff = false;

        if (leftHand != null)
        {
            bool flipOff = true;
            foreach (Finger finger in leftHand.Fingers)
            {
                if (finger.Type == Finger.FingerType.TYPE_MIDDLE && !finger.IsExtended)
                {
                    flipOff = false;
                }
                else if (finger.Type != Finger.FingerType.TYPE_MIDDLE && finger.IsExtended)
                {
                    flipOff = false;
                }
            }
            if (!flipOff)
            {
                doubleFlipOff = false;
                hasPlayedDoubleFlipOff = false;
            }
        }
        else doubleFlipOff = false;

        if (doubleFlipOff && !hasPlayedDoubleFlipOff)
        {
            hasPlayedDoubleFlipOff = true;
            airhorn.Play();
        }

        if (isDying) {
            float t = (Time.time - startTime) / dieFade;
            float tClamp = t * 2 > 1 ? 1 : t * 2;
            colorCurve.saturation = 1 - tClamp * (1 - MIN_SATURATION);
            Time.timeScale = 1 - tClamp * (1 - MIN_SATURATION);
            if (!hasPlayedDeath) {
                wastedSound.Play();
                hasPlayedDeath = true;
            }
            if (t >= 1) {
                wastedText.gameObject.SetActive(true);
            }

            if (Time.time - startTime > dieDuration) {
                isDying = false;
                hasPlayedDeath = false;
                wastedText.gameObject.SetActive(false);
                colorCurve.saturation = 1;
                Time.timeScale = 1;
                transform.position = RaceManager.Instance.currentWaypoint.transform.position;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                    RaceManager.Instance.currentWaypoint.transform.rotation.eulerAngles.y,
                    transform.rotation.eulerAngles.z);
            }
        }

        if (RaceManager.Instance.raceFinished) {
            foreach(Hand hand in frame.Hands) {
                bool thumbUp = true;
                foreach (Finger finger in hand.Fingers) {
                    if (finger.Type == Finger.FingerType.TYPE_THUMB && !finger.IsExtended) {
                        thumbUp = false;
                    }
                    else if (finger.Type != Finger.FingerType.TYPE_THUMB && finger.IsExtended) {
                        thumbUp = false;
                    }
                }
                if (thumbUp) {
                    Debug.Log("ASDASDASD");
                    SceneManager.LoadScene(0);
                }
            }
        }
        //changes the flashing light
        if (RaceManager.Instance.raceStarted && !RaceManager.Instance.raceFinished)
        {
            distanceToCheckpoint = RaceManager.Instance.targetWaypoint.transform.position.magnitude - camera.transform.position.magnitude;
            flashRate = Mathf.Abs(distanceToCheckpoint);
            if (lightCounter > flashRate)
            {
                lightCounter = 0;
            }
            else if (lightCounter > flashRate / 2) {
                lightCounter++;
                warmer.gameObject.SetActive(false);
            }
            else {
                lightCounter++;
                warmer.gameObject.SetActive(true);
            }
        }
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag.Equals("Ground") && !isDying) {
            isDying = true;
            startTime = Time.time;
        }
    }
}
