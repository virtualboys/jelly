using UnityEngine;
using System.Collections;

public class MoveMesh : MonoBehaviour {

	public Rigidbody rigidbody;

	// Use this for initialization
	void Start () {
		//transform.SetParent (rigidbody.transform);
	}
	
	// Update is called once per frame
	void Update () {
		//rigidBody.AddForce (new Vector3 (0, 0, 0));
        transform.position = rigidbody.transform.position;
        transform.rotation = rigidbody.transform.rotation;
	}
	void FixedUpdate() {
        
		
        //Debug.Log("rigid body:" + rigidbody.transform.position + "game object position:" + transform.position);
	}

	public void dragObj(Vector3 grabPos, Vector3 force) {
		rigidbody.AddForceAtPosition (force, grabPos);
		//Debug.Log (force.magnitude);
	}
}
