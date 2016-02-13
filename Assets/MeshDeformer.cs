using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	public float springForce = 20f;
	public float damping = 5f;

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;
	Vector3[] vertexVelocities;

	float uniformScale = .1f;

	Material material;
	int leftPoleInd = 0;
	int rightPoleInd = 0;

	void Start () {
		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}
		vertexVelocities = new Vector3[originalVertices.Length];

		MeshRenderer mesh = GetComponent<MeshRenderer> ();
		material = mesh.material;
		getPoles ();
	}

    bool set = false;
	void Update () {
		uniformScale = transform.localScale.x;
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i);
		}
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();

        if (!set)
        {
            Vector3 leftPole = transform.TransformPoint(displacedVertices[leftPoleInd]);
            Vector3 rightPole = transform.TransformPoint(displacedVertices[rightPoleInd]);
            material.SetVector("_LeftPole", new Vector4(leftPole.x, leftPole.y, leftPole.z));
            material.SetVector("_RightPole", new Vector4(rightPole.x, rightPole.y, rightPole.z));
            set = true;
        }
	}

	void UpdateVertex (int i) {
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];
		displacement *= uniformScale;
		velocity -= displacement * springForce * Time.deltaTime;
		velocity *= 1f - damping * Time.deltaTime;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
        //originalVertices[i] += .1f * velocity * (Time.deltaTime / uniformScale);
        //originalVertices[i] += .1f * displacement * (Time.deltaTime / uniformScale);
	}

	public void AddDeformingForce (int vertexInd, Vector3 pullDir, float forceMag) {
		Vector3 point = deformingMesh.vertices [vertexInd];
		for (int i = 0; i < displacedVertices.Length; i++) {
			AddForceToVertex(i, point, pullDir, forceMag);
		}
	}

	void AddForceToVertex (int i, Vector3 point, Vector3 pullDir, float forceMag) {
		point = transform.InverseTransformPoint (point);
		pullDir = transform.InverseTransformVector (pullDir);

		Vector3 pointToVertex = 2*(displacedVertices[i] - point);
		pointToVertex *= uniformScale;
		float attenuatedForce = forceMag / (1.0f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * Time.deltaTime;
		vertexVelocities[i] += pullDir * velocity;
	}

	public Vector3 getVertPos (int vertexInd) {
		return transform.TransformPoint(displacedVertices [vertexInd]);
	}

	void getPoles () {
		float leftPos = originalVertices [0].x;
		float rightPos = originalVertices [0].x;
		for (int i = 1; i < originalVertices.Length; i++) {
			if (originalVertices [i].x < leftPos) {
				leftPos = originalVertices [i].x;
				leftPoleInd = i;
			} else if (originalVertices [i].x > rightPos) {
				rightPos = originalVertices [i].x;
				rightPoleInd = i;
			}
		}
	}
}