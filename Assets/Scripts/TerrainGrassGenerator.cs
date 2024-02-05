using UnityEngine;

public class TerrainGrassGenerator : MonoBehaviour
{
    public Terrain terrain;
    public GameObject grassPrefab;
    public float grassDensity = 0.1f;

    void Start()
    {
        GenerateGrass();
    }

    void GenerateGrass()
    {
        // Get the terrain data
        TerrainData terrainData = terrain.terrainData;

        // Loop through the terrain's size and randomly place grass
        for (int x = 0; x < terrainData.size.x; x++)
        {
            for (int z = 0; z < terrainData.size.z; z++)
            {
                if (Random.value < grassDensity)
                {
                    // Calculate the world position of the current terrain point
                    float worldX = x * terrainData.size.x / terrainData.heightmapResolution + terrain.transform.position.x;
                    float worldZ = z * terrainData.size.z / terrainData.heightmapResolution + terrain.transform.position.z;
                    float worldY = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));

                    // Place the grass at the terrain point
                    GameObject grass = Instantiate(grassPrefab, new Vector3(worldX, worldY, worldZ), Quaternion.identity);
                    grass.transform.parent = transform;
                }
            }
        }
    }
}