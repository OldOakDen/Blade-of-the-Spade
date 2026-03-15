using UnityEngine;

public class ClayEffect : MonoBehaviour
{
    public GameObject clayPrefab; // Placeholder for the clay-like object
    public int numClayObjects = 10; // Number of clay objects to create
    public float sizeCoefficient = 1.0f; // Koeficient velikosti pro úpravu velikosti hliny

    private void OnEnable()
    {
        // Get the main object (mesh) where clay objects will be placed
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("No MeshFilter found on the main object!");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        // Create and place clay objects randomly on selected vertices
        for (int i = 0; i < numClayObjects; i++)
        {
            int randomVertexIndex = Random.Range(0, vertices.Length);
            Vector3 randomVertex = transform.TransformPoint(vertices[randomVertexIndex]); // Convert local vertex position to global position

            Quaternion randomRotation = Random.rotation;
            Vector3 randomScale = new Vector3(Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f));

            // Apply the size coefficient to the random scale
            randomScale *= sizeCoefficient;

            GameObject clayObject = Instantiate(clayPrefab, randomVertex, randomRotation);
            clayObject.transform.localScale = randomScale;
            // Set the parent of the clay object to the main object
            clayObject.transform.parent = transform;

            // Attach a script to the clay object to handle mouse clicks
            clayObject.AddComponent<ClayClickHandler>();
        }
    }
}
