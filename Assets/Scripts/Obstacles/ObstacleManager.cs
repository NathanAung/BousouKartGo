using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    // Time in seconds between each spawn attempt.
    public float spawnInterval = 3f;

    // Maximum number of active obstacles allowed on the map.
    public int maxObstacles = 20;

    [Header("Distance Safety")]
    public Transform[] players;

    // Spawners closer than this distance to any player will be ignored.
    public float minSafeDistance = 40f;

    private ObstacleSpawner[] allSpawners;
    [SerializeField] private List<GameObject> activeObstacles = new List<GameObject>();
    private float timer;

    void Start()
    {
        // Grab all the spawners in the scene right when the game starts
        allSpawners = FindObjectsByType<ObstacleSpawner>(FindObjectsSortMode.None);

        // Grab all the players in the scene right when the game starts
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players = new Transform[playerObjects.Length];
        for (int i = 0; i < playerObjects.Length; i++)
        {
            players[i] = playerObjects[i].transform;
        }
    }

    void Update()
    {
        // Clean up the list (removes null entries if barrels explode)
        activeObstacles.RemoveAll(item => item == null);

        // Run the timer
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f; // Reset timer

            // Check if we are under the global maximum limit
            if (activeObstacles.Count < maxObstacles)
            {
                TriggerRandomSpawner();
            }
        }
    }

    void TriggerRandomSpawner()
    {
        if (allSpawners.Length == 0) return;

        // Filter out spawners that are too close to the players
        List<ObstacleSpawner> safeSpawners = new List<ObstacleSpawner>();

        foreach (ObstacleSpawner spawner in allSpawners)
        {
            bool isSafe = true;
            foreach (Transform player in players)
            {
                if (player != null && Vector3.Distance(spawner.transform.position, player.position) < minSafeDistance)
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
            {
                safeSpawners.Add(spawner);
            }
        }

        // If all spawners are unsafe, skip this spawn cycle
        if (safeSpawners.Count == 0) return;

        // Pick a random safe spawner
        ObstacleSpawner chosenSpawner = safeSpawners[Random.Range(0, safeSpawners.Count)];

        // Tell the spawner to do its job, and track what it makes
        GameObject newObstacle = chosenSpawner.SpawnObstacle();

        if (newObstacle != null)
        {
            activeObstacles.Add(newObstacle);
        }
    }
}