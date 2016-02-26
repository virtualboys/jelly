using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	public float springForce = 20f;
	public float damping = 5f;
    public const float deformFalloff = 1f;

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;
	Vector3[] vertexVelocities;

	float uniformScale = .1f;

    MeshRenderer meshRenderer;
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

        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
		
        getPoles ();
        Vector3 leftPole = transform.TransformPoint(displacedVertices[leftPoleInd]);
        Vector3 rightPole = transform.TransformPoint(displacedVertices[rightPoleInd]);
        material.SetVector("_LeftPole", new Vector4(leftPole.x, leftPole.y, leftPole.z));
        material.SetVector("_RightPole", new Vector4(rightPole.x, rightPole.y, rightPole.z));
	}

	void Update () {
		uniformScale = transform.localScale.x;
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i);
		}
        //deformingMesh.Clear();
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
	}

    public void SelectMesh(Material selectedMat)
    {
        meshRenderer.material = selectedMat;
    }

    public void DeselectMesh()
    {
        meshRenderer.material = material;
    }

	void UpdateVertex (int i) {
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];

		displacement *= uniformScale;
		velocity -= displacement * springForce * Time.deltaTime;
		velocity *= 1f - damping * Time.deltaTime;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
        originalVertices[i] += .1f * velocity * (Time.deltaTime / uniformScale);
        //originalVertices[i] += .1f * displacement * (Time.deltaTime / uniformScale);
	}

	public void AddDeformingForce (int vertexInd, Vector3 pullDir, float forceMag, float falloff = deformFalloff) {
		Vector3 point = deformingMesh.vertices [vertexInd];
		AddDeformingForce(point, pullDir, forceMag, falloff);
	}
    public void AddDeformingForceWorld(Vector3 point, Vector3 pullDir, float forceMag, float falloff = deformFalloff)
    {
        point = transform.InverseTransformPoint(point);
        AddDeformingForce(point, pullDir, forceMag, falloff);
    }

    public void AddDeformingForce(Vector3 point, Vector3 pullDir, float forceMag, float falloff)
    {
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, pullDir, forceMag, falloff);
        }
    }

	void AddForceToVertex (int i, Vector3 point, Vector3 pullDir, float forceMag, float falloff) {
		//point = transform.InverseTransformPoint (point);
		pullDir = transform.InverseTransformVector (pullDir);

		Vector3 pointToVertex = falloff * (displacedVertices[i] - point);
		pointToVertex *= uniformScale;
		float attenuatedForce = forceMag / (1.0f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * Time.deltaTime;
		vertexVelocities[i] += pullDir * velocity;
	}

	public Vector3 getVertPos (int vertexInd) {
		return transform.TransformPoint(displacedVertices [vertexInd]);
	}

    public Vector3 getVertNormal(int vertexInd)
    {
        return transform.TransformVector(deformingMesh.normals[vertexInd]);
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