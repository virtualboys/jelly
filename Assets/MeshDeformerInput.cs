using UnityEngine;

public class MeshDeformerInput : MonoBehaviour {

    public Material selectedMat;
	
	public float dragStrength = 10f;
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
    private Vector3 grabPos;
	private MeshDeformer selectedMesh;
	private MoveMesh meshMover;

    private bool zoomed;

    void Start()
    {
        headBob = GetComponentInChildren<HeadBob>();
        fpd = GetComponent<FirstPersonDrifter>();
    }

    void Update()
    {
        if (Input.GetButtonDown("zoom"))
        {
            zoomed = true;
            if (selectedMesh != null)
                selectedMesh.SelectMesh(selectedMat);
        }
        else if (Input.GetButtonUp("zoom"))
        {
            zoomed = false;
            if (selectedMesh != null)
                selectedMesh.DeselectMesh();
        }
    }

	void FixedUpdate () {
        if (Input.GetAxis("Right") > .5f && meshMover != null)
        {
            DeformMesh();
            mouseLookX.sensMod = mouseLookY.sensMod = .5f;
            fpd.speedMod = .5f;
            headBob.bobMod = .5f;
        }
        else
        {
            GetMesh();
            mouseLookX.sensMod = mouseLookY.sensMod = 1;
            fpd.speedMod = 1f;
            headBob.bobMod = 1.0f;
        }

        if (selectedMesh != null)
        {            
            if (Input.GetButton("poke"))
                poke();
        }
        if (meshMover != null)
            pull();
	}

	void GetMesh () {
        MeshDeformer oldMesh = selectedMesh;
        selectedMesh = null;
        meshMover = null;
        selectedVertex = -1;

        Ray inputRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
		RaycastHit hit;
		
		if (Physics.Raycast(inputRay, out hit)) {
            grabPos = hit.point;
			MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            MoveMesh moveMesh = hit.collider.GetComponent<MoveMesh> ();
			if (deformer) {
				selectedMesh = deformer;
                meshMover = moveMesh;

				MeshFilter mesh = hit.collider.GetComponent<MeshFilter>();
				int[] meshTris = mesh.mesh.triangles;

				selectedVertex = meshTris[hit.triangleIndex*3];
            }
            else if (moveMesh)
            {
                meshMover = moveMesh;
            }
        }

        if (zoomed && selectedMesh != oldMesh)
        {
            if(oldMesh != null)
                oldMesh.DeselectMesh();
            if (selectedMesh != null)
                selectedMesh.SelectMesh(selectedMat);
        }
	}

	void DeformMesh () {
		Vector3 vertPos= Camera.main.WorldToScreenPoint(grabPos);
		Vector3 force = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
		Vector3 worldForce = (force.x * Camera.main.transform.right);
		worldForce += force.y * Camera.main.transform.up;
		worldForce.Normalize ();

		float forceMag = force.magnitude;

        if(selectedMesh != null) 
		    selectedMesh.AddDeformingForce (selectedVertex, worldForce, forceMag * deformForce);

        if(meshMover != null)
		    meshMover.dragObj (grabPos, worldForce * Time.deltaTime * forceMag * dragStrength);
	}

    void pull()
    {
        float forceMag = Input.GetAxis("Left") * pullStrength;
        Vector3 dir = transform.position - meshMover.transform.position;
        meshMover.dragObj(meshMover.transform.position, dir * forceMag);
    }

    void poke()
    {
        Vector3 dir = -selectedMesh.getVertNormal(selectedVertex);
        selectedMesh.AddDeformingForce(selectedVertex, dir, pokeForce, 2);
    }
}