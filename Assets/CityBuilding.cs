using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    [Header("City Settings")]
    public int cityWidth = 10;             // Number of blocks wide
    public int cityLength = 10;            // Number of blocks long
    public float blockSpacing = 1.2f;      // Space between buildings

    [Header("Building Settings")]
    public GameObject buildingPrefab;      // The building prefab
    public int minBuildingHeight = 1;      // Minimum height for a building
    public int maxBuildingHeight = 5;      // Maximum height for a building

    [Header("Road Settings")]
    public GameObject roadPrefab;          // Road prefab
    public int roadFrequency = 4;          // Frequency of roads (every X blocks)

    private void Start()
    {
        GenerateCity();
    }

    // Generates the entire city
    public void GenerateCity()
    {
        // Clear existing city (if regenerating)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Generate city grid
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityLength; z++)
            {
                Vector3 position = new Vector3(x * blockSpacing, 0, z * blockSpacing);

                // Road creation: Create roads at intervals (e.g., every 4 blocks)
                if (x % roadFrequency == 0 || z % roadFrequency == 0)
                {
                    Instantiate(roadPrefab, position, Quaternion.identity, transform);
                    continue;  // Skip building placement for roads
                }

                // Determine building height
                int buildingHeight = Random.Range(minBuildingHeight, maxBuildingHeight + 1);

                // Create the building at the calculated position
                GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity, transform);

                // Scale building height
                building.transform.localScale = new Vector3(1, buildingHeight, 1);
                building.transform.position += new Vector3(0, buildingHeight / 2f, 0); // Center on Y-axis
            }
        }
    }

    // Regenerate the city on command
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateCity();
        }
    }
}

