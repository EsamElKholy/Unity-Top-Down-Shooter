using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State { Idle, Chasing, Attacking };
    State currentState;

    public NavMeshAgent pathfinder;
    Transform target;

    public ParticleSystem deathEffect;

    public static event System.Action OnDeathStatic;

    float attackDistance = 1.5f;
    float damage = 1;
    float timeBetweenAttacks = 1;
    float nextAttackTime;

    float myCollisionRadius;
    float targetCollisionRadius;

    Material skinMaterial;
    Color originalColor;

    LivingEntity targetEntity;
    bool hasTarget;

    void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();

        if (hasTarget)
        {
            targetEntity.OnDeath += OnTargetDeath;

            currentState = State.Chasing;

            StartCoroutine(UpdatePath());
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime && target != null)
            {
                float sqrDist = (target.position - transform.position).sqrMagnitude;
                if (sqrDist < Mathf.Pow(attackDistance + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;

                    AudioManager.instance.PlaySound("enemy attack", transform.position);

                    StartCoroutine(Attack());
                }
            }
        }
	}

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float health, Color skinCol)
    {
        pathfinder.speed = moveSpeed;

        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.maxHealth / hitsToKillPlayer);
        }

        maxHealth = health;

        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinCol;

        originalColor = skinMaterial.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) 
    {
        AudioManager.instance.PlaySound("impacts", transform.position);

        if (damage >= currentHealth)
        {
            if (OnDeathStatic != null)
            {
                OnDeathStatic();
            }

            AudioManager.instance.PlaySound("enemy death", transform.position);

            GameObject newParticleSystem = Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject;
            Destroy(newParticleSystem, deathEffect.main.startLifetimeMultiplier);
        }

        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        if (Cursor.visible == false)
        {
            Cursor.visible = true;
        }

        hasTarget = false;

        currentState = State.Idle;
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPos = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPos = target.position - (dirToTarget * (myCollisionRadius));

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;

        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if (!hasAppliedDamage && percent >= 0.5f)
            {
                hasAppliedDamage = true;

                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = ((-percent * percent) + percent) * 4.0f;

            transform.position = Vector3.Lerp(originalPos, attackPos, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;

        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while (hasTarget)
        {
            if (currentState == State.Chasing && target != null)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPos = target.position - (dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistance / 2.0f));

                if (!dead)
                {
                    pathfinder.SetDestination(targetPos);
                }
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }
}
