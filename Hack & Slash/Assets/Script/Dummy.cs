using UnityEngine;

public class Dummy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float maxHealth;
    [SerializeField] float moveSpeed;
    [SerializeField] private float contactDamage;

    [Header("AI Settings")]
    [SerializeField] float avoidanceRayDistance;
    [SerializeField] LayerMask obstacleLayer;

    [Header("Damage Number")]
    [SerializeField] GameObject damageNumberPrefab;
    [SerializeField] Transform damageNumberSpawnPoint;
    [SerializeField] float spawnRadius;
    
    private float currentHealth;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Canvas gameCanvas;

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
        if (playerTransform == null) return;
        
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<TopDownPlayer>(out TopDownPlayer player))
        {
            player.TakeDamage(contactDamage);
        }
    }
    
    private void Die()
    {
        Debug.Log($"{gameObject.name} has been destroyed!");
        Destroy(gameObject);
    }
}