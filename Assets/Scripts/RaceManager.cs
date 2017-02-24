using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour {
	public GameObject player;
	public GameObject ringPrefab;

    public Text welcomeMessage;
    public Text countdownText;
    public Text timerText;
    public Text finishText;
    public Text restartText;
    public AudioSource finishSong;
    public AudioSource countDownSound;
    public AudioSource countDownDoneSound;

    public float fadeDuration = 1f;
    public int countdown = 3;

    public GameObject targetWaypoint;
    public GameObject currentWaypoint;
    private GameObject firstWaypoint;

    private const float inToMeters = 0.0254f;

    public bool raceStarted = false;
    public bool raceFinished = false;
    public bool countdownStarted = false;
    private float startTime;

    private static RaceManager _instance;
    public static RaceManager Instance { get { return _instance; } }
    
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        }
        else {
            _instance = this;
        }
    }

    void Start () {
		try {
			using (StreamReader sr = new StreamReader("Assets/Data/waypoints.txt")) {
				string line;
				Vector3 prevPosition = new Vector3();
				GameObject prevWayPoint = null;

				if ((line = sr.ReadLine()) != null) {
					String[] coords = line.Split(' ');
					Vector3 position = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
					position *= inToMeters;

					player.transform.position = position;

                    GameObject waypoint = Instantiate(ringPrefab, position, Quaternion.identity);
                    firstWaypoint = waypoint;
                    currentWaypoint = waypoint;

                    prevPosition = position;
					prevWayPoint = waypoint;
				}

                bool firstLoop = true;

				while ((line = sr.ReadLine()) != null) {
					String[] coords = line.Split(' ');
					Vector3 position = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
					position *= inToMeters;
					GameObject waypoint = Instantiate(ringPrefab, position, Quaternion.identity);
                    Waypoint wp = prevWayPoint.GetComponent<Waypoint>();
                    if (wp) {
                        wp.next = waypoint;
                    }

                    if (firstLoop) {
                        firstLoop = false;
                        waypoint.GetComponent<Waypoint>().active = true;
                        targetWaypoint = waypoint;
                    }

					if (prevWayPoint) {
						Debug.Log(position - prevPosition);
						prevWayPoint.transform.rotation = Quaternion.LookRotation(position - prevPosition);
					}

					prevPosition = position;
					prevWayPoint = waypoint;
				}
			}
		}
		catch (Exception e) {
			Debug.Log("fukc uyo");
		}
        _instance.countdownText.text = "" + countdown;
    }
	
	void Update () {
		if (_instance.countdownStarted) {
            float t = (Time.time - startTime) / fadeDuration;
            if (t >= 1 && countdown > 0) {
                startTime = Time.time;
                countdown--;
                if (countdown>0) {
                    countDownSound.Play();
                }
                else {
                    countDownDoneSound.Play();
                }
                _instance.countdownText.text = "" + countdown;
                Debug.Log(countdown);
            }
            countdownText.transform.localScale = new Vector3(t, t, t);
            countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, 1-t);

            if (t >= 1 && countdown == 0 && !_instance.raceStarted) {
                raceStarted = true;
                _instance.countdownText.text = "GO";
                _instance.timerText.gameObject.SetActive(true);
                _instance.timerText.text = "0.00s";
                startTime = Time.time;
                Debug.Log("ITS TIME");
            }
        }
        if (_instance.raceStarted) {
            _instance.timerText.text = Math.Round((Time.time - startTime) * 100)/100f + "s";
        }
	}

    public static void nextWaypoint() {
        _instance.currentWaypoint = _instance.targetWaypoint;
        _instance.targetWaypoint.GetComponent<Waypoint>().active = false;
        _instance.targetWaypoint = _instance.targetWaypoint.GetComponent<Waypoint>().next;
        if (_instance.targetWaypoint) {
            _instance.targetWaypoint.GetComponent<Waypoint>().active = true;
        }
        else {
            Debug.Log("FINISH");
            finishRace();
            _instance.finishSong.Play();
        }
    }

    public static void finishRace() {
        _instance.raceFinished = true;
        _instance.timerText.gameObject.SetActive(false);
        _instance.finishText.gameObject.SetActive(true);
        _instance.finishText.text = Time.time - _instance.startTime + "s";
        _instance.restartText.gameObject.SetActive(true);
    }

    public static void startCountdown() {
        _instance.countdownStarted = true;
        _instance.startTime = Time.time;
        _instance.welcomeMessage.gameObject.SetActive(false);
        _instance.countDownSound.Play();
    }

    public static void startRace() {
        _instance.raceStarted = true;
        _instance.startTime = Time.time;
    }
}
