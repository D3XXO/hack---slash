using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Dummy : MonoBehaviour
{
    [System.Serializable]
    public struct LootItem
    {
        public ItemData itemData;
        [Range(0, 100)]
        public float dropChance;
    }

    [Header("Loot Settings")]
    [SerializeField] private GameObject itemPickupPrefab;
    [SerializeField] [Range(0, 100)] float chanceToDropAnything;
    [SerializeField] private List<LootItem> lootTable;

    [Header("Stats")]
    [SerializeField] float maxHealth;
    [SerializeField] float moveSpeed;
    [SerializeField] private float contactDamage;

    [Header("AI Settings")]
    [SerializeField] float avoidanceRayDistance;
    [SerializeField] LayerMask obstacleLayer;

    [Header("Knockback Settings")]
    [SerializeField] float knockbackDuration;
    [SerializeField] float knockbackForceOnPlayer;
    [SerializeField] float knockbackForceOnEachOther;
    
    [Header("Damage Number")]
    [SerializeField] GameObject damageNumberPrefab;
    [SerializeField] Transform damageNumberSpawnPoint;
    [SerializeField] float spawnRadius;
    
    private float currentHealth;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Canvas gameCanvas;
    
    private bool isBeingKnockedBack = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        gameCanvas = FindObjectOfType<Canvas>();
        
        playerTransform = FindObjectOfType<TopDownPlayer>()?.transform;
        
        if (playerTransform == null)
        {
            this.enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null || isBeingKnockedBack) return;
        
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 directionToPlayerRaw = playerTransform.position - transform.position;
        Vector2 directionToPlayer;

        if (Mathf.Abs(directionToPlayerRaw.x) > Mathf.Abs(directionToPlayerRaw.y))
        {
            directionToPlayer = new Vector2(directionToPlayerRaw.x, 0f).normalized;
        }
        else
        {
            directionToPlayer = new Vector2(0f, directionToPlayerRaw.y).normalized;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, avoidanceRayDistance, obstacleLayer);

        Vector2 moveDirection;

        if (hit.collider != null)
        {
            moveDirection = Vector2.Reflect(directionToPlayer, hit.normal);
            Debug.DrawRay(transform.position, moveDirection * avoidanceRayDistance, Color.red);
        }
        else
        {
            moveDirection = directionToPlayer;
            Debug.DrawRay(transform.position, moveDirection * avoidanceRayDistance, Color.green);
        }

        rb.velocity = moveDirection * moveSpeed * Time.deltaTime;
    }

    public void TakeDamage(float damage, bool isCrit)
    {
        currentHealth -= damage;

        if (damageNumberPrefab != null && gameCanvas != null)
        {
            Vector3 basePosition = damageNumberSpawnPoint.position;
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = basePosition + new Vector3(randomOffset.x, randomOffset.y, 0);
            GameObject damageNumberInstance = Instantiate(damageNumberPrefab, gameCanvas.transform);
            DamageNumber dnScript = damageNumberInstance.GetComponent<DamageNumber>();
            dnScript.worldPositionToFollow = spawnPosition;
            dnScript.SetDamage(damage, isCrit);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void ApplyKnockback(Vector2 direction, float force)
    {
        StopAllCoroutines();
        StartCoroutine(KnockbackCoroutine(direction, force));
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
    {
        isBeingKnockedBack = true;
        
        rb.velocity = direction * force;

        yield return new WaitForSeconds(knockbackDuration);
        
        rb.velocity = Vector2.zero;
        isBeingKnockedBack = false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<TopDownPlayer>(out TopDownPlayer player))
        {
            player.TakeDamage(contactDamage);

            Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
            player.ApplyKnockback(knockbackDirection, knockbackForceOnPlayer);

            if (player.mainCameraController != null)
            {
                player.mainCameraController.TriggerShake(0.15f, 0.2f);
            }
        }
        else if (collision.gameObject.TryGetComponent<Dummy>(out Dummy otherDummy))
        {
            Vector2 knockbackDirection = (transform.position - otherDummy.transform.position).normalized;
            ApplyKnockback(knockbackDirection, knockbackForceOnEachOther);
        }
    }

    private void HandleLootDrop()
    {
        float roll = Random.Range(0f, 100f);
        if (roll > chanceToDropAnything)
        {
            return;
        }

        foreach (LootItem item in lootTable)
        {
            float itemRoll = Random.Range(0f, 100f);
            if (itemRoll <= item.dropChance)
            {
                SpawnItem(item.itemData);
            }
        }
    }

    private void SpawnItem(ItemData itemData)
    {
        if (itemPickupPrefab != null)
        {
            GameObject spawnedItemObject = Instantiate(itemPickupPrefab, transform.position, Quaternion.identity);
            
            ItemPickup pickupScript = spawnedItemObject.GetComponent<ItemPickup>();
            if (pickupScript != null)
            {
                pickupScript.Setup(itemData, 1);
            }
        }
    }
    
    private void Die()
    {
        HandleLootDrop();
        Destroy(gameObject);
    }
}