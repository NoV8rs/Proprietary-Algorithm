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
    public GameObject buildingPrefab;
    public int minBuildingHeight = 1;
    public int maxBuildingHeight = 5;

    [Header("Road Settings")]
    public GameObject roadPrefab;
    public GameObject roadIntersectionPrefab;
    public GameObject roadCornerPrefab;
    public GameObject roadEndPrefab;
    public int roadFrequency = 4;

    [Header("Stop Sign Settings")]
    public GameObject stopSignPrefab; // Add your stop sign prefab here
    public float stopSignOffset = 0.5f; // Offset from the center of the intersection

    [Header("Spawn Delay Settings")]
    public float roadSpawnDelay = 0.05f;
    public float buildingSpawnDelay = 0.1f;
    public float stopSignSpawnDelay = 0.1f;
    public float randomizeSpawnRate = 0.02f;

    private bool[,] isRoad;

    private void Start()
    {
        StartCoroutine(GenerateCity());
    }

    private IEnumerator GenerateCity()
    {
        isRoad = new bool[cityWidth, cityLength];

        // Phase 1: Base Road Generation
        yield return StartCoroutine(GenerateBaseRoadLayout());

        // Phase 2: Road Decoration and Stop Signs
        yield return StartCoroutine(DetailRoadsWithDelay());

        // Phase 3: Building Placement
        yield return StartCoroutine(GenerateBuildingsWithDelay());
        
        // Phase 4: Place stop signs
        yield return StartCoroutine(PlaceStopSignsWithDelay());
    }

    private IEnumerator GenerateBaseRoadLayout()
    {
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
        yield return null;
    }
    
    private IEnumerator PlaceStopSignsWithDelay()
    {
        for (int x = 0; x < cityWidth; x++)
        {
            for (int z = 0; z < cityLength; z++)
            {
                if (isRoad[x, z])
                {
                    Vector3 position = new Vector3(x * blockSpacing, 0, z * blockSpacing);
                    (GameObject roadToPlace, Quaternion rotation) = DetermineRoadPrefab(isRoad, x, z);

                    if (roadToPlace == roadIntersectionPrefab)
                    {
                        SpawnStopSigns(position);
                        
                        float randomDelay = stopSignSpawnDelay + Random.Range(-randomizeSpawnRate, randomizeSpawnRate);
                        yield return new WaitForSeconds(Mathf.Max(0, randomDelay));
                    }
                }
            }
        }
        yield return null;
    }

    private IEnumerator DetailRoadsWithDelay()
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

                    float randomDelay = roadSpawnDelay + Random.Range(-randomizeSpawnRate, randomizeSpawnRate);
                    yield return new WaitForSeconds(Mathf.Max(0, randomDelay));
                }
            }
        }
    }

    private void SpawnStopSigns(Vector3 intersectionPosition)
    {
        intersectionPosition.y = 0.5f;
        
        Vector3[] stopSignPositions = new Vector3[]
        {
            intersectionPosition + new Vector3(-stopSignOffset, 0, stopSignOffset),
            intersectionPosition + new Vector3(stopSignOffset, 0, -stopSignOffset)
        };

        Quaternion[] stopSignRotations = new Quaternion[]
        {
            Quaternion.Euler(0, 90, 0),
            Quaternion.Euler(0, -180, 0)
        };

        for (int i = 0; i < stopSignPositions.Length; i++)
        {
            Instantiate(stopSignPrefab, stopSignPositions[i], stopSignRotations[i], transform);
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
                    GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity, transform);

                    building.transform.localScale = new Vector3(1, buildingHeight, 1);
                    building.transform.position += new Vector3(0, buildingHeight / 2f, 0);

                    float randomDelay = buildingSpawnDelay + Random.Range(-randomizeSpawnRate, randomizeSpawnRate);
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
            if (left) return (roadEndPrefab, Quaternion.Euler(0, 180, 0));
            if (right) return (roadEndPrefab, Quaternion.Euler(0, -180, 0));
            if (down) return (roadEndPrefab, Quaternion.Euler(0, 90, 0));
            return (roadEndPrefab, Quaternion.identity);
        }
        else if (adjacentRoads == 2)
        {
            if (left && right) return (roadPrefab, Quaternion.Euler(0, 180, 0));
            if (up && down) return (roadPrefab, Quaternion.Euler(0, 90, 0));
            if (up && right) return (roadCornerPrefab, Quaternion.Euler(0, 90, 0));
            if (up && left) return (roadCornerPrefab, Quaternion.Euler(0, 0, 0));
            if (down && right) return (roadCornerPrefab, Quaternion.Euler(0, -180, 0));
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



