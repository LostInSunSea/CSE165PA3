using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {
	public GameObject next;
	public GameObject particleEffect;
    public GameObject particleEffectActive;
    public AudioSource checkpointSound;
    public bool active = false;

	public float spawnRate = 0.5f;

    private float startTime;

	void Start () {
        startTime = Time.time;
	}
	
	void Update () {
		if (Time.time - startTime >= spawnRate && next) {
            startTime = Time.time;
            if (next.GetComponent<Waypoint>().active) {
                WaypointTrail trail = Instantiate(particleEffectActive, transform.position, Quaternion.identity).GetComponent<WaypointTrail>();
                trail.target = next;
            }
            else {
                WaypointTrail trail = Instantiate(particleEffect, transform.position, Quaternion.identity).GetComponent<WaypointTrail>();
                trail.target = next;
            }
        }
	}

    void OnTriggerEnter(Collider other) {
        Debug.Log("OH BABY A TRIPLE");
        if (active && other.gameObject.tag.Equals("Player")) {
            RaceManager.nextWaypoint();
            checkpointSound.Play();
            Debug.Log("OH BABY A TRIPLE");
        }
    }
}
