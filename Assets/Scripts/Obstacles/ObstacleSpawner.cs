using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawner Specifics")]
    // The specific obstacles this spawner can create.
    public GameObject[] specificObstacles;

    // The radius around this point where obstacles can spawn.
    public float spawnRadius = 8f;


    // The Manager will call this method when it's time to spawn
    public GameObject SpawnObstacle()
    {
        // Safety check to ensure we have something to spawn
        if (specificObstacles == null || specificObstacles.Length == 0) return null;

        // Pick a random prefab from this spawner's unique list
        GameObject prefabToSpawn = specificObstacles[Random.Range(0, specificObstacles.Length)];

        // Calculate a random position within the radius on the XZ plane
        // Using insideUnitCircle ensures they spread out horizontally, not up/down
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        // Add that random offset to the spawner's current position
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        // Instantiate the object and return it back to the Manager
        return Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }


    // Visualizer for the Unity Editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}