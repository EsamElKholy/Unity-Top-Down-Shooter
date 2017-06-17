using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Vector3 velocity;
    Rigidbody myRigidbody;

	// Use this for initialization
	void Start ()
    {
        myRigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        myRigidbody.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
	}

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void LookAt(Vector3 point)
    {
        Vector3 fixedHeight = new Vector3(point.x, transform.position.y, point.z);

        //transform.LookAt(transform.position - fixedHeight);
        transform.rotation = Quaternion.LookRotation((fixedHeight - transform.position).normalized);
        transform.Rotate(Vector3.up, -90);
    }
}
