using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, bool isCrit);
    void ApplyKnockback(Vector2 direction, float force);
}