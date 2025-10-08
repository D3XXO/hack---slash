// EnemySpawner.cs
using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] float spawnInterval;

    [Header("Spawn Area")]
    [SerializeField] float minSpawnRadius;
    [SerializeField] float maxSpawnRadius;
    
    private Transform playerTransform;

    void Start()
    {
        playerTransform = FindObjectOfType<TopDownPlayer>().transform;

        if (playerTransform == null) return;

        StartCoroutine(SpawnEnemyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 spawnPosition = playerTransform.position + (Vector3)(randomDirection * randomDistance);

            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}