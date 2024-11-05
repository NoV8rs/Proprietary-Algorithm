using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    // Script is in the CityManager game object
    
    [Header("City Settings")]
    public int cityWidth = 10;             
    public int cityLength = 10;            
    public float blockSpacing = 1.2f;      

    [Header("Building Settings")]
    public GameObject buildingPrefab;
    public int minBuildingHeight = 1;     
    public int maxBuildingHeight = 5;      

    [Header("Road Settings")]
    public GameObject roadPrefab;          
    public int roadFrequency = 4;         

    private void Start()
    {
        GenerateCity();
    }

    // Generates the entire city
    public void GenerateCity()
    {
        // Regenerate the city - Destroy all children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Generate city grid
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityLength; z++)
            {
                Vector3 position = new Vector3(x * blockSpacing, 0, z * blockSpacing); // Block spacing is the distance between each block

                
                if (x % roadFrequency == 0 || z % roadFrequency == 0) // How often to place a road, check inspector to see the value
                {
                    Instantiate(roadPrefab, position, Quaternion.identity, transform); // Create a road
                    continue;
                }
                
                int buildingHeight = Random.Range(minBuildingHeight, maxBuildingHeight + 1); // Building height can see the inspector to see the value
                
                GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity, transform); // After the roads are created, create the buildings
                
                building.transform.localScale = new Vector3(1, buildingHeight, 1); // Scale the building on the Y-axis
                building.transform.position += new Vector3(0, buildingHeight / 2f, 0); // Center on Y-axis
            }
        }
    }
    
    private void Update() // Update the city when the R key is pressed, for testing purposes - Add a button in the UI later
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateCity();
        }
    }
}

