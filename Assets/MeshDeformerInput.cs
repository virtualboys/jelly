﻿using UnityEngine;

public class MeshDeformerInput : MonoBehaviour {
	
	private float force = -10f;
	private float forceOffset = 1f;
    
    public MouseLook mouseLookX;
    public MouseLook mouseLookY;
    public FirstPersonDrifter fpd;
    private HeadBob headBob;

    public float sRamp=3f;
	private int selectedVertex;
	private MeshDeformer selectedMesh;
	private MoveMesh meshMover;

    void Start()
    {
        headBob = GetComponentInChildren<HeadBob>();
    }

	void Update () {

		if (Input.GetMouseButton (0)) {
			if (selectedMesh == null)
				GetMesh ();
			else {
				DeformMesh ();
			}
		} else {
			selectedMesh = null;
			meshMover = null;
			selectedVertex = -1;
		}

        if(selectedMesh!=null){
            float distance=(transform.position - selectedMesh.transform.position).magnitude;
            float a = 1 / distance;
            float b = (1.0f - .1f) / (1.0f - 10.0f);
           // mouseLook.sensMod = b * Mathf.Clamp(distance, 1, 10)+1;]
            mouseLookX.sensMod = mouseLookY.sensMod = .2f;
            fpd.speedMod = .25f;
            headBob.bobMod = .5f;
        } else {
            mouseLookX.sensMod = mouseLookY.sensMod = 1;
            fpd.speedMod = 1f;
            headBob.bobMod = 1.0f;
        }
	}

	void GetMesh () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast(inputRay, out hit)) {
			MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
			if (deformer) {
				selectedMesh = deformer;
				meshMover = hit.collider.GetComponent<MoveMesh> ();

				MeshFilter mesh = hit.collider.GetComponent<MeshFilter>();
				int[] meshTris = mesh.mesh.triangles;
				//mesh
//				Debug.Log ("tris length " + meshTris.Length);
//				Debug.Log ("tri ind " + hit.triangleIndex);
				//Debug.Log
				selectedVertex = meshTris[hit.triangleIndex*3];

//				Vector3 point = hit.point;
//				point += hit.normal * forceOffset;
//				deformer.AddDeformingForce(point, force);
			}
		}
	}

	void DeformMesh () {
		Vector3 grabPos = selectedMesh.getVertPos (selectedVertex);
		Vector3 vertPos= Camera.main.WorldToScreenPoint(grabPos);
		Vector3 force = Input.mousePosition - vertPos;
		Vector3 worldForce = (force.x * Camera.main.transform.right);
		worldForce += force.y * Camera.main.transform.up;
		worldForce.Normalize ();

		float forceMag = force.magnitude/5f;
		//Debug.Log (forceMag);
		selectedMesh.AddDeformingForce (selectedVertex, worldForce.normalized, forceMag);

		meshMover.dragObj (grabPos, worldForce * Time.deltaTime * forceMag);
	}
}