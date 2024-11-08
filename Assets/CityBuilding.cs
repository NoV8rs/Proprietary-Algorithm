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
    public Color[] buildingColors;

    [Header("Road Settings")]
    public GameObject roadPrefab;
    public GameObject roadIntersectionPrefab;
    public GameObject roadCornerPrefab;
    public GameObject roadEndPrefab;
    public int roadFrequency = 4;
    
    [Header("Spawn Delay Settings")]
    public float randomizeSpawnDelay = 0.1f;
    public float buildingSpawnDelay = 0.1f;
    public float roadSpawnDelay = 0.1f;

    private bool[,] isRoad;

    private void Start()
    {
        StartCoroutine(GenerateCity());
    }
    
    private void DestroyPreviousCity()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator GenerateCity()
    {
        DestroyPreviousCity();
        
        isRoad = new bool[cityWidth, cityLength];

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

        yield return StartCoroutine(GenerateRoadsWithDelay());

        yield return StartCoroutine(GenerateBuildingsWithDelay());
    }

    private IEnumerator GenerateRoadsWithDelay()
    {
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityLength; z++)
            {
                if (isRoad[x, z])
                {
                    Vector3 position = new Vector3(x * blockSpacing, 0, z * blockSpacing);
                    (GameObject roadToPlace, Quaternion rotation) = DetermineRoadPrefab(isRoad, x, z);

                    if (roadToPlace != null)
                    {
                        Instantiate(roadToPlace, position, rotation, transform);
                    }

                    // Apply randomized spawn delay for roads
                    float randomDelay = roadSpawnDelay + Random.Range(-randomizeSpawnDelay, randomizeSpawnDelay);
                    yield return new WaitForSeconds(Mathf.Max(0, randomDelay));
                }
            }
        }
    }

    private IEnumerator GenerateBuildingsWithDelay()
    {
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityLength; z++)
            {
                if (!isRoad[x, z])
                {
                    Vector3 position = new Vector3(x * blockSpacing, 0, z * blockSpacing);

                    int buildingHeight = Random.Range(minBuildingHeight, maxBuildingHeight + 1);
                    GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                    GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity, transform);

                    building.transform.localScale = new Vector3(1, buildingHeight, 1);
                    building.transform.position += new Vector3(0, buildingHeight / 2f, 0);
                    
                    Color buildingColor = buildingColors[Random.Range(0, buildingColors.Length)];
                    Renderer buildingRenderer = buildingPrefab.GetComponent<Renderer>();
                    
                    float randomDelay = buildingSpawnDelay + Random.Range(-randomizeSpawnDelay, randomizeSpawnDelay);
                    yield return new WaitForSeconds(Mathf.Max(0, randomDelay));
                }
            }
        }
    }

    private (GameObject, Quaternion) DetermineRoadPrefab(bool[,] isRoad, int x, int z)
    {
        int adjacentRoads = CountAdjacentRoads(isRoad, x, z);
        bool left = (x > 0 && isRoad[x - 1, z]);
        bool right = (x < cityWidth - 1 && isRoad[x + 1, z]);
        bool down = (z > 0 && isRoad[x, z - 1]);
        bool up = (z < cityLength - 1 && isRoad[x, z + 1]);

        if (adjacentRoads == 1)
        {
            if (left) return (roadEndPrefab, Quaternion.Euler(0, 90, 0));
            if (right) return (roadEndPrefab, Quaternion.Euler(0, -180, 0));
            if (down) return (roadEndPrefab, Quaternion.Euler(0, 90, 0));
            return (roadEndPrefab, Quaternion.identity);
        }
        else if (adjacentRoads == 2)
        {
            if (left && right) return (roadPrefab, Quaternion.Euler(0, 180, 0));
            if (up && down) return (roadPrefab, Quaternion.Euler(0, 90, 0));
            if (up && right) return (roadCornerPrefab, Quaternion.Euler(0, 90, 0));
            if (up && left) return (roadCornerPrefab, Quaternion.Euler(0, 180, 0));
            if (down && right) return (roadCornerPrefab, Quaternion.Euler(0, -90, 0));
            return (roadCornerPrefab, Quaternion.Euler(0, 180, 0));
        }
        else if (adjacentRoads >= 3)
        {
            return (roadIntersectionPrefab, Quaternion.identity);
        }

        return (roadPrefab, Quaternion.identity);
    }

    private int CountAdjacentRoads(bool[,] isRoad, int x, int z)
    {
        int adjacentRoads = 0;

        if (x > 0 && isRoad[x - 1, z]) adjacentRoads++;
        if (x < cityWidth - 1 && isRoad[x + 1, z]) adjacentRoads++;
        if (z > 0 && isRoad[x, z - 1]) adjacentRoads++;
        if (z < cityLength - 1 && isRoad[x, z + 1]) adjacentRoads++;

        return adjacentRoads;
    }
    
    public void ButtonGenerate()
    {
        StartCoroutine(GenerateCity());
    }
}



