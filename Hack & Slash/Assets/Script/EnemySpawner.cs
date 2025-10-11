using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public string enemyName;
        public GameObject enemyPrefab;
        public float spawnInterval;
        
        public bool isUnique;

        [HideInInspector]
        public float spawnTimer;
        [HideInInspector]
        public GameObject uniqueInstance;
    }

    [Header("Spawner Settings")]
    [SerializeField] private List<EnemySpawnConfig> enemySpawnList;

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
        playerTransform = FindObjectOfType<TopDownPlayer>()?.transform;

        if (playerTransform == null)
        {
            this.enabled = false;
            return;
        }

        foreach (var config in enemySpawnList)
        {
            config.spawnTimer = config.spawnInterval;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        foreach (var config in enemySpawnList)
        {
            if (config.isUnique)
            {
                HandleUniqueSpawn(config);
            }
            else
            {
                HandleRegularSpawn(config);
            }
        }
    }
    
    private void HandleRegularSpawn(EnemySpawnConfig config)
    {
        config.spawnTimer -= Time.deltaTime;
        if (config.spawnTimer <= 0f)
        {
            AttemptToSpawn(config);
            config.spawnTimer = config.spawnInterval;
        }
    }

    private void HandleUniqueSpawn(EnemySpawnConfig config)
    {
        if (config.uniqueInstance == null)
        {
            config.spawnTimer -= Time.deltaTime;
            if (config.spawnTimer <= 0f)
            {
                GameObject spawnedEnemy = AttemptToSpawn(config);

                if (spawnedEnemy != null)
                {
                    config.uniqueInstance = spawnedEnemy;
                    config.spawnTimer = config.spawnInterval;
                }
            }
        }
    }

    private GameObject AttemptToSpawn(EnemySpawnConfig config)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 spawnPosition = playerTransform.position + (Vector3)(randomDirection * randomDistance);

            Collider2D overlap = Physics2D.OverlapCircle(spawnPosition, enemyRadius, enemyLayer);

            if (overlap == null)
            {
                return Instantiate(config.enemyPrefab, spawnPosition, Quaternion.identity);
            }
        }

        return null;
    }
}