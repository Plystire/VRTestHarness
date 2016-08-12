using UnityEngine;
using System.Collections;

public class AntiGravity : MonoBehaviour {

    Rigidbody rig;

    Vector3 origPos;
    float lastTime = 0.0f;
    float diffTime = 0.0f;

	// Use this for initialization
	void Start () {
        rig = GetComponent<Rigidbody>();

        origPos = transform.position;
	}
	
	// FixedUpdate is called once per frame
	void Update () {
        //rig.velocity -= Physics.gravity * Time.fixedDeltaTime;

        if (transform.position.y < origPos.y)
        {   //
            // Reset position
            transform.position = origPos;
            // Apply impulse velocity to counteract gravity for a time
            rig.velocity = -Physics.gravity * (0.33f / 2.0f);
            Debug.Log("Vel: " + rig.velocity);
            diffTime = Time.time - lastTime;
            Debug.Log("DiffTime: " + diffTime);
            lastTime = Time.time;
        }
	}
}
