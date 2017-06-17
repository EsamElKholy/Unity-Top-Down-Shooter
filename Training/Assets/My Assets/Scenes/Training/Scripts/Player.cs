using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PlayerController))]
[RequireComponent (typeof(GunController))]
public class Player : LivingEntity
{
    public float speed = 5;
    public CrossHairs crossHairs;

    Camera viewCamera;
    PlayerController controller;
    GunController gunController;

    private void Awake()
    {
        viewCamera = Camera.main;
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();

        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int index)
    {
        currentHealth = maxHealth;
        gunController.EquipGun(index - 1);
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();        
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Movement
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * speed;

        controller.Move(moveVelocity);

        // Mouse Look
        //print("x: " + Input.mousePosition.x + " ,y: " + Input.mousePosition.y + " ,z: " + Input.mousePosition.z);
        Ray camRay = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDist;

        if (ground.Raycast(camRay, out rayDist))
        {
            Vector3 point = camRay.GetPoint(rayDist);

            //Debug.DrawLine(camRay.origin, point, Color.red);

            crossHairs.transform.position = point;
            crossHairs.DetectTargets(camRay);

            controller.LookAt(point);

            //if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            //{
            //    gunController.Aim(point);
            //}
        }

        // Weapon input
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }

        //
        if (transform.position.y < -10)
        {
            TakeDamage(currentHealth);
        }
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("player death", transform.position);

        base.Die();
    }
}
