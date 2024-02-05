using UnityEngine;


public class TerrainGenerator : MonoBehaviour
{
    public int width = 512;
    public int height = 512;
    public int depth = 20;
    public float scale = 20f;
    public float islandRadius = 128f;
    public float edgeBlend = 5f;
    public bool autoUpdate = false;
    public float aa = 1f;
    private Terrain terrain;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    private TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
    }

    private float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];

        // Calculate the center of the terrain
        float centerX = width / 2f;
        float centerY = height / 2f;

        // Generate Perlin noise values
        float[,] noise = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float perlinX = (float)x / width * scale;
                float perlinY = (float)y / height * scale;
                noise[x, y] = Mathf.PerlinNoise(perlinX, perlinY);
            }
        }

        // Calculate the height at each position based on distance from the center and Perlin noise
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distanceX = Mathf.Abs(x - centerX);
                float distanceY = Mathf.Abs(y - centerY);
                float distance = Mathf.Sqrt(Mathf.Pow(distanceX, 2f) + Mathf.Pow(distanceY, 2f));

                float heightValue = 0f;

                // Set the height to the Perlin noise value at low elevations
                if (distance <= islandRadius - edgeBlend)
                {
                    heightValue = 1f - noise[x, y];
                }
                // Blend the height between the Perlin noise value and the island edge height at higher elevations
                else if (distance <= islandRadius)
                {
                    float t = ((distance - (islandRadius - edgeBlend)) / edgeBlend);
                    heightValue = 1f - Mathf.Lerp(noise[x, y], 1f, t);
                }

                heights[x, y] = heightValue;
            }
        }

        return heights;
    }



    private void OnValidate()
    {
        if (autoUpdate)
        {
            Terrain terrain = GetComponent<Terrain>();
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
        }

    }
}
