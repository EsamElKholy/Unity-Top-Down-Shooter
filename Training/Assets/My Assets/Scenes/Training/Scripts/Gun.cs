using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;

    public Transform[] muzzles;
    public Projectile projectile;
    public float msInterval = 100;
    public float muzzleVelocity = 35;
    public int burstCount;
    public int projectilesPerMag;
    public float reloadTime;

    [Header("Recoil")]
    public Vector2 kickRange = new Vector2(0.05f, 0.2f);
    public Vector2 recoilAngleRange = new Vector2(5, 10);
    public float recoilMoveSettleTime = 0.1f;
    public float recoilAngleSettleTime = 0.1f;

    [Header("Effects")]
    public Transform shell;
    public Transform shellEjection;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    MuzzleFlash muzzleFlash;
    float coolDown = 0;

    bool triggerReleasedSinceLastShot;
    int remainingShotsInBurst;
    int projectilesRemainingInMag;

    Vector3 smoothDampVelocity;
    float rotSmoothDampVelocity;

    float recoilAngle;

    bool isReloading;
    
	// Use this for initialization
	void Start ()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        remainingShotsInBurst = burstCount;

        projectilesRemainingInMag = projectilesPerMag;
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        // Animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref smoothDampVelocity, recoilMoveSettleTime);

        if (!isReloading)
        {
            recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref rotSmoothDampVelocity, recoilAngleSettleTime);
            transform.localEulerAngles = Vector3.forward * recoilAngle;
        }

        if (!isReloading && projectilesRemainingInMag == 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        if (!isReloading && Time.time > coolDown && projectilesRemainingInMag > 0)
        {
            if (fireMode == FireMode.Burst)
            {
                if (remainingShotsInBurst == 0)
                {
                    return;
                }

                remainingShotsInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            for (int i = 0; i < muzzles.Length; i++)
            {
                if (projectilesRemainingInMag == 0)
                {
                    break;
                }

                projectilesRemainingInMag--;

                Quaternion rot = muzzles[i].rotation;

                muzzles[i].Rotate(Vector3.up, 90);
                Projectile newProjectile = Instantiate(projectile, muzzles[i].position, muzzles[i].rotation) as Projectile;
                muzzles[i].rotation = rot;

                newProjectile.SetSpeed(muzzleVelocity);
                coolDown = msInterval / 1000.0f + Time.time;
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();

            transform.localPosition -= Vector3.right * Random.Range(kickRange.x, kickRange.y);
            recoilAngle += Random.Range(recoilAngleRange.x, recoilAngleRange.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootSound, transform.position);
        }
    }

    public void Reload()
    {
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag)
        {
            AudioManager.instance.PlaySound(reloadSound, transform.position);
            StartCoroutine(AnimateReload());
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;

        yield return new WaitForSeconds(0.2f);

        float reloadSpeed = 1.0f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;

        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = ((-percent * percent) + percent) * 4.0f;

            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.forward * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        remainingShotsInBurst = burstCount;
    }

    public void Aim(Vector3 aim)
    {
        transform.LookAt(aim);
    }
}
