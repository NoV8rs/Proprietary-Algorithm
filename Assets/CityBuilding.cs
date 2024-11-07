using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    [Header("City Settings")]
    public int cityWidth = 10;
    public int cityLength = 10;
    public float blockSpacing = 1.2f;

    [Header("Building Settings")]
    public GameObject[] buildingPrefabs;
    public int minBuildingHeight = 1;
    public int maxBuildingHeight = 5;

    [Header("Road Settings")]
    public GameObject roadPrefab;
    public GameObject roadIntersectionPrefab;
    public GameObject roadCornerPrefab;
    public GameObject roadEndPrefab;
    public int roadFrequency = 4;

    private void Start()
    {
        GenerateCity();
    }
    
    public void GenerateCity()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Create a grid to store road positions
        bool[,] isRoad = new bool[cityWidth, cityLength];

        // How frequently to place roads
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityLength; z++)
            {
                if (x % roadFrequency == 0 || z % roadFrequency == 0)
                {
                    isRoad[x, z] = true;
                }
            }
        }

        // Buildings placement and road placement
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityLength; z++)
            {
                Vector3 position = new Vector3(x * blockSpacing, 0, z * blockSpacing);

                // isRoad position
                if (isRoad[x, z])
                {
                    (GameObject roadToPlace, Quaternion rotation) = DetermineRoadPrefab(isRoad, x, z);

                    if (roadToPlace != null)
                    {
                        Instantiate(roadToPlace, position, rotation, transform);
                    }
                    continue;
                }

                int buildingHeight = Random.Range(minBuildingHeight, maxBuildingHeight + 1);
                GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity, transform);
                
                building.transform.localScale = new Vector3(1, buildingHeight, 1);
                building.transform.position += new Vector3(0, buildingHeight / 2f, 0);
            }
        }
    }

    // Spawns the Roads for the City
    private (GameObject, Quaternion) DetermineRoadPrefab(bool[,] isRoad, int x, int z)
    {
        int adjacentRoads = CountAdjacentRoads(isRoad, x, z);
        bool left = (x > 0 && isRoad[x - 1, z]);
        bool right = (x < cityWidth - 1 && isRoad[x + 1, z]);
        bool down = (z > 0 && isRoad[x, z - 1]);
        bool up = (z < cityLength - 1 && isRoad[x, z + 1]);
        
        if (adjacentRoads == 1) // End cap and Values for rotation
        {
            if (left) return (roadEndPrefab, Quaternion.Euler(0, 0, 0));
            if (right) return (roadEndPrefab, Quaternion.Euler(0, 180, 0));
            if (down) return (roadEndPrefab, Quaternion.Euler(0, 90, 0));
            return (roadEndPrefab, Quaternion.identity); // Up
        }
        else if (adjacentRoads == 2) // Straight or corner and Values for rotation
        { 
            if (left && right) return (roadPrefab, Quaternion.Euler(0, 180, 0));
            if (up && down) return (roadPrefab, Quaternion.Euler(0, 90, 0));
            if (up && right) return (roadCornerPrefab, Quaternion.Euler(0, 90, 0));
            if (up && left) return (roadCornerPrefab, Quaternion.Euler(0, 180, 0));
            if (down && right) return (roadCornerPrefab, Quaternion.Euler(0, -180, 0));
            return (roadCornerPrefab, Quaternion.Euler(0, 180, 0));
        }
        else if (adjacentRoads >= 3) // Intersection
        {
            return (roadIntersectionPrefab, Quaternion.identity);
        }

        return (roadPrefab, Quaternion.identity);
    }
    
    private int CountAdjacentRoads(bool[,] isRoad, int x, int z)
    {
        int adjacentRoads = 0;

        // Check all four neighbors (up, down, left, right)
        if (x > 0 && isRoad[x - 1, z]) adjacentRoads++; // Left
        if (x < cityWidth - 1 && isRoad[x + 1, z]) adjacentRoads++; // Right
        if (z > 0 && isRoad[x, z - 1]) adjacentRoads++; // Down
        if (z < cityLength - 1 && isRoad[x, z + 1]) adjacentRoads++; // Up

        return adjacentRoads;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateCity();
        }
    }
}



