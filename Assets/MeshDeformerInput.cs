using UnityEngine;

public class MeshDeformerInput : MonoBehaviour {
	
	public float dragForce = 10f;
    public float deformForce = 1f;
	private float forceOffset = 1f;
    
    public MouseLook mouseLookX;
    public MouseLook mouseLookY;
    private FirstPersonDrifter fpd;
    public float pullStrength = 10f;
    private HeadBob headBob;

    public float sRamp=3f;
	private int selectedVertex;
	private MeshDeformer selectedMesh;
	private MoveMesh meshMover;

    void Start()
    {
        headBob = GetComponentInChildren<HeadBob>();
        fpd = GetComponent<FirstPersonDrifter>();
    }

	void FixedUpdate () {

        if (Input.GetAxis("Right") > .5f)
        {
            if (selectedMesh == null)
            {
                GetMesh();
            }
            else
            {
                DeformMesh();
            }
        }
        else
        {
            selectedMesh = null;
            meshMover = null;
            selectedVertex = -1;
        }

        if (selectedMesh != null)
        {
           
            float forceMag=Input.GetAxis("Left") *pullStrength;
            meshMover.dragObj(meshMover.transform.position,-Camera.main.transform.forward*forceMag);

            //float distance = (transform.position - selectedMesh.transform.position).magnitude;
            //float a = 1 / distance;
            //float b = (1.0f - .1f) / (1.0f - 10.0f);
            // mouseLook.sensMod = b * Mathf.Clamp(distance, 1, 10)+1;]
            mouseLookX.sensMod = mouseLookY.sensMod = .5f;
            fpd.speedMod = .5f;
            headBob.bobMod = .5f;
        }
        else
        {
            mouseLookX.sensMod = mouseLookY.sensMod = 1;
            fpd.speedMod = 1f;
            headBob.bobMod = 1.0f;
        }
	}

	void GetMesh () {
        Ray inputRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
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
		Vector3 force = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
		Vector3 worldForce = (force.x * Camera.main.transform.right);
		worldForce += force.y * Camera.main.transform.up;
		worldForce.Normalize ();

		float forceMag = force.magnitude;
		selectedMesh.AddDeformingForce (selectedVertex, worldForce, forceMag * deformForce);

		meshMover.dragObj (grabPos, worldForce * Time.deltaTime * forceMag * dragForce);
	}
}