using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public Transform[] spawnZones; // 4 spawn points
    public Transform center;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 5f;

    void Start()
    {
        StartCoroutine(SpawnObstacles());
    }

    IEnumerator SpawnObstacles()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            Transform zone = spawnZones[Random.Range(0, spawnZones.Length)];
            SpawnObstacle(zone.position);
        }
    }

    void SpawnObstacle(Vector3 position)
    {
        GameObject obj = Instantiate(obstaclePrefab, position, Quaternion.identity);
        Obstacle obstacle = obj.GetComponent<Obstacle>();
        obstacle.target = center;
        obstacle.spawnTime = Time.time;
    }
}
