using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionLayer;
    public Color trailColor;

    float speed = 10;
    float damage = 1;
    float lifeTime = 3;
    float skinWidth = 0.1f;

	// Use this for initialization
	void Start ()
    {
        Destroy(gameObject, lifeTime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionLayer);

        if (initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0], transform.position);
        }

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
	}
	
	// Update is called once per frame
	void Update ()
    {
        float moveDistance = Time.deltaTime * speed;

        CheckCollisions(moveDistance);

        transform.Translate(Vector3.forward * moveDistance);
	}

    public void SetSpeed(float s)
    {
        speed = s;
    }

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionLayer, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }    

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamagable damagableObject = c.GetComponent<IDamagable>();

        if (damagableObject != null)
        {
            damagableObject.TakeHit(damage, hitPoint, transform.forward);
        }

        GameObject.Destroy(gameObject);
    }
}
