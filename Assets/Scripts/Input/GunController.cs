using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public GunStats stats;
    public Transform muzzle;
    public Camera cam;
    public AudioSource audioSrc;
    public Animator anim;

    [Header("Hit VFX Settings")]
    public GameObject hitEffectPrefab;

    [Header("Aiming Settings")]
    public float normalFOV = 60f;
    public float aimFOV = 40f;
    public float aimSpeed = 10f;
    public Transform gunTransform;
    public Vector3 hipPosition = new Vector3(0.01f, -0.02f, 0.0f);
    public Vector3 hipRotation = new Vector3(0f, 0f, 0f);
    public Vector3 aimPosition = new Vector3(0f, -0.01f, 0.1f);
    public Vector3 aimRotation = new Vector3(0f, 0f, 0f);

    float nextFire;
    int currentAmmo;

    void Start()
    {
        currentAmmo = stats.maxAmmo;

        // Hide the cursor for immersion
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleAiming();

        if (Input.GetButton("Fire1") && Time.time >= nextFire)
        {
            if (stats.fireMode == FireMode.Single)
            {
                if (Input.GetButtonDown("Fire1")) TryShoot();
            }
            else
            {
                TryShoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Reload());
    }

    void HandleAiming()
    {
        bool isAiming = Input.GetButton("Fire2");

        // Camera FOV zoom
        float targetFOV = isAiming ? aimFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);

        // Move and rotate gun
        if (gunTransform != null)
        {
            Vector3 targetPos = isAiming ? aimPosition : hipPosition;
            Vector3 targetRot = isAiming ? aimRotation : hipRotation;

            gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, targetPos, Time.deltaTime * aimSpeed);
            gunTransform.localRotation = Quaternion.Lerp(gunTransform.localRotation, Quaternion.Euler(targetRot), Time.deltaTime * aimSpeed);
        }
    }

    void TryShoot()
    {
        if (currentAmmo <= 0) return;

        nextFire = Time.time + stats.fireRate;
        currentAmmo--;
        Shoot();
    }

    void Shoot()
    {
        Debug.Log("FIRE!");

        if (anim != null) anim.SetTrigger("Shoot");
        if (audioSrc != null && stats.shootSound != null)
            audioSrc.PlayOneShot(stats.shootSound);

        if (stats.muzzleFlashPrefab != null && muzzle != null)
            Instantiate(stats.muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        int layerMask = Physics.DefaultRaycastLayers;

        if (Physics.Raycast(ray, out RaycastHit hit, stats.range, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("HIT!");
            SpawnImpact(hit);
        }
        else
        {
            Debug.Log("NO HIT!");
        }
    }

    IEnumerator Reload()
    {
        if (anim != null) anim.SetTrigger("Reload");
        yield return new WaitForSeconds(stats.reloadTime);
        currentAmmo = stats.maxAmmo;
    }

    void SpawnImpact(RaycastHit hit)
    {
        if (stats.bulletHolePrefab != null && !hit.collider.CompareTag("Enemy"))
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal);
            Vector3 pos = hit.point + hit.normal * 0.01f;
            var hole = Instantiate(stats.bulletHolePrefab, pos, rot);

            hole.transform.localScale = Vector3.one * Random.Range(0.08f, 0.12f);
            hole.transform.Rotate(0, 0, Random.Range(0f, 360f));
            Destroy(hole, 15f);
        }

        if (hitEffectPrefab != null)
        {
            var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(fx, 2f);
        }
    }
}
