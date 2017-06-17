using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    protected bool dead;
    public float maxHealth;
    public float currentHealth { get; protected set; }

    public event System.Action OnDeath;

    // Use this for initialization
    protected virtual void Start ()
    {
        currentHealth = maxHealth;
	}

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0 && !dead)
        {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    public virtual void Die()
    {
        dead = true;        

        if (OnDeath != null)
        {
            OnDeath();
        }

        GameObject.Destroy(gameObject);
    }
}
