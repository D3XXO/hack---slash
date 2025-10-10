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

    [Header("Overlap Prevention")]
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float enemyRadius;
    [SerializeField] int maxSpawnAttempts;

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

            bool positionFound = false;
            Vector3 spawnPosition = Vector3.zero;

            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
                spawnPosition = playerTransform.position + (Vector3)(randomDirection * randomDistance);

                Collider2D overlap = Physics2D.OverlapCircle(spawnPosition, enemyRadius, enemyLayer);

                if (overlap == null)
                {
                    positionFound = true;
                    break;
                }
            }

            if (positionFound)
            {
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }
}