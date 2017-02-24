using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointTrail : MonoBehaviour {
	public GameObject target;
	public float speed = 1f;
    private bool active = true;
	
	void Update () {
        if (target) {
            if (active) {
                Vector3 deltaPos = target.transform.position - transform.position;
                Vector3 direction = deltaPos.normalized * speed;
                transform.Translate(direction * Time.deltaTime);

                if (deltaPos.magnitude <= speed / 50f) {
                    transform.position = target.transform.position;
                    StartCoroutine(StartDeath());
                }
            }
        }
        else {
            Destroy(this.gameObject);
        }
	}

    IEnumerator StartDeath() {
        GetComponent<ParticleSystem>().Stop();
        active = false;
        yield return new WaitForSeconds(2);
        Destroy(this.gameObject);
    }
}
