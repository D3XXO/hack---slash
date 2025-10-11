using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Miniboss : MonoBehaviour, IDamageable
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

    [Header("QTE Settings")]
    [SerializeField] [Range(0.01f, 1f)] float qteHealthThreshold;
    [SerializeField] private GameObject qtePromptPrefab;
    [SerializeField] private Transform gameCanvas;
    [SerializeField] private Vector3 qtePromptOffset;
    
    private GameObject qtePromptInstance;
    private RectTransform qtePromptRectTransform;
    private Camera mainCamera;
    private bool isQteAvailable = false;
    private bool isQteTriggered = false;
    private QteManager qteManager;

    private float currentHealth;
    private Transform playerTransform;
    private Rigidbody2D rb;
    
    private bool isBeingKnockedBack = false;
    private bool isInQteState = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        playerTransform = FindObjectOfType<TopDownPlayer>()?.transform;
        qteManager = FindObjectOfType<QteManager>();

        mainCamera = Camera.main;

        if (gameCanvas == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                gameCanvas = canvas.transform;
            }
        }

        if (qteManager == null)
        {
            this.enabled = false;
        }

        if (playerTransform == null)
        {
            this.enabled = false;
        }
    }
    
    void LateUpdate()
    {
        if (qtePromptInstance != null && qtePromptInstance.activeInHierarchy)
        {
            Vector3 targetWorldPosition = damageNumberSpawnPoint.position + qtePromptOffset;
            
            Vector2 screenPoint = mainCamera.WorldToScreenPoint(targetWorldPosition);

            qtePromptRectTransform.position = screenPoint;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null || isBeingKnockedBack || isInQteState) return;
        
        HandleMovement();
    }

    public void TakeDamage(float damage, bool isCrit)
    {
        if (isInQteState) return;

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

        if (currentHealth / maxHealth <= qteHealthThreshold && !isQteAvailable)
        {
            MakeQteAvailable();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void MakeQteAvailable()
    {
        isQteAvailable = true;

        if (qtePromptPrefab != null && gameCanvas != null)
        {
            qtePromptInstance = Instantiate(qtePromptPrefab, gameCanvas);
            
            qtePromptRectTransform = qtePromptInstance.GetComponent<RectTransform>();
        }

        qteManager.RegisterQteTarget(this);
    }
    
    public void InitiateQteSequence()
    {
        isQteTriggered = true;
        isInQteState = true;
        rb.velocity = Vector2.zero;

        if (qtePromptInstance != null)
        {
            qtePromptInstance.SetActive(false);
        }
    }

    public void SetQteState(bool state)
    {
        isInQteState = state;
    }

    public bool IsQteAvailable()
    {
        return isQteAvailable;
    }

    public void HandleQtePartialFailure(int stagesCompleted)
    {
        float healthPoolForQte = maxHealth * qteHealthThreshold;

        float damagePerStage = healthPoolForQte / 4f;

        float targetHealth = healthPoolForQte - (stagesCompleted * damagePerStage);

        currentHealth = Mathf.Max(targetHealth, 1f);

        isQteAvailable = false;
        isQteTriggered = false;
        SetQteState(false);
    }

    public void Die()
    {
        if(isQteAvailable)
        {
            qteManager.UnregisterQteTarget(this);
        }
        
        if (qtePromptInstance != null)
        {
            Destroy(qtePromptInstance);
        }

        HandleLootDrop();
        Destroy(gameObject);
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

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (isInQteState) return;
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
        if (isInQteState) return;

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
}