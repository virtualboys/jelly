using UnityEngine;

public class MeshDeformerInput : MonoBehaviour {
	
	public float pullForce = 10f;
    public float deformForce = 1f;
    public float pokeForce = 10f;
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
            pull();
            
            mouseLookX.sensMod = mouseLookY.sensMod = .5f;
            fpd.speedMod = .5f;
            headBob.bobMod = .5f;

            if (Input.GetButton("poke"))
                poke();
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

				selectedVertex = meshTris[hit.triangleIndex*3];

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

		meshMover.dragObj (grabPos, worldForce * Time.deltaTime * forceMag * pullForce);
	}

    void pull()
    {
        float forceMag = Input.GetAxis("Left") * pullStrength;
        Vector3 dir = transform.position - selectedMesh.transform.position;
        meshMover.dragObj(meshMover.transform.position, dir * forceMag);
    }

    void poke()
    {
        Vector3 dir = -selectedMesh.getVertNormal(selectedVertex);
        selectedMesh.AddDeformingForce(selectedVertex, dir, pokeForce);
    }
}