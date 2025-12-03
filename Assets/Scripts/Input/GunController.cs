using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class GunController : InteractableBase
{
    [Header("Recoil Settings")]
    public float recoilAmount = 0.1f;
    public float recoilSpeed = 5f;
    private Vector3 originalGunPosition;
    private Vector3 recoilOffset;

    public TextMeshProUGUI ammoText;


    public GunStats stats;
    public Transform muzzle;
    [SerializeField] private Camera cam;
    [SerializeField] private Camera pixelFOVCam;

    public AudioSource audioSrc;
    public Animator anim;

    [Header("Hit VFX Settings")]
    public GameObject hitEffectPrefab;

    [Header("Aiming Settings")]
    public float normalFOV = 60f;
    public float aimFOV = 40f;
    public float aimSpeed = 10f;
    public Transform GunSlideTransform;
    public Vector3 hipPosition = new Vector3(0.01f, -0.02f, 0.0f);
    public Vector3 hipRotation = new Vector3(0f, 0f, 0f);
    public Vector3 aimPosition = new Vector3(0f, -0.01f, 0.1f);
    public Vector3 aimRotation = new Vector3(0f, 0f, 0f);
    private bool isAiming = false;
    public static Action<bool> disableBaseInteraction;

    float nextFire;
    //How much ammo is currently in gun
    int currentAmmoInGun;

    void Start()
    {
        currentAmmoInGun = stats.maxAmmo;

        // Hide the cursor for immersion
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        originalGunPosition = GunSlideTransform.localPosition;
        ammoText.gameObject.SetActive(false);
    }

    //private void OnEnable()
    //{
    //    itemShouldBeRotatedWhenHeld = Quaternion.Euler(0, 90, -40);
    //}

    void Update()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmoInGun} / {stats.pickedUpAmmo}";

        HandleRecoil();
        HandleAiming();
        if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(Reload());
        if (!isAiming)
        {
            ammoText.gameObject.SetActive(false);
            return;
        }
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
        ammoText.gameObject.SetActive(true);
    }
    void HandleRecoil()
    {
        if (GunSlideTransform != null)
        {
            // Lerp recoil effect back to original position
            recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilSpeed);
            GunSlideTransform.localPosition = Vector3.Lerp(GunSlideTransform.localPosition, originalGunPosition + recoilOffset, Time.deltaTime * recoilSpeed * 6f);
        }
    }

    void HandleAiming()
    {
        isAiming = Input.GetMouseButton(1);

        if(Input.GetMouseButtonDown(1)) disableBaseInteraction?.Invoke(isAiming);
        if(Input.GetMouseButtonUp(1)) disableBaseInteraction?.Invoke(isAiming);

        float targetFOV = isAiming ? aimFOV : normalFOV;
        pixelFOVCam.fieldOfView = Mathf.Lerp(pixelFOVCam.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);

        // Move and rotate gun
        if (GunSlideTransform != null)
        {
            Vector3 targetPos = isAiming ? aimPosition : hipPosition;
            Vector3 targetRot = isAiming ? aimRotation : hipRotation; 
            Vector3 recoilTargetPos = targetPos + recoilOffset;
            //gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, recoilTargetPos, Time.deltaTime * aimSpeed);
            GunSlideTransform.localRotation = Quaternion.Lerp(GunSlideTransform.localRotation, Quaternion.Euler(targetRot), Time.deltaTime * aimSpeed);
        }
    }

    void TryShoot()
    {
        if (currentAmmoInGun <= 0) return;

        nextFire = Time.time + stats.fireRate;
        currentAmmoInGun--;
        Debug.Log(currentAmmoInGun);
        Shoot();
    }

    void Shoot()
    {
        if (anim != null) anim.SetTrigger("Shoot");
        if (audioSrc != null && stats.shootSound != null)
            audioSrc.PlayOneShot(stats.shootSound);

        if (stats.muzzleFlashPrefab != null && muzzle != null)
            Instantiate(stats.muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        int layerMask = Physics.DefaultRaycastLayers;

        if (Physics.Raycast(ray, out RaycastHit hit, stats.range, layerMask, QueryTriggerInteraction.Ignore))
        {
            SpawnImpact(hit);
            TryStunEnemy(hit);
        }
        //recoilOffset = new Vector3(0f, 0f, -recoilAmount); // the gun gets pushed back
        //        recoilOffset = new Vector3(UnityEngine.Random.Range(-0.05f, 0.05f),UnityEngine.Random.Range(0.05f, 0.1f),-0.3f
        //);
        //recoilOffset = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), -recoilAmount);

        //recoilOffset = -gunTransform.forward * recoilAmount;
        //recoilOffset = -cam.transform.forward * recoilAmount;
        
        Vector3 camBack = -GunSlideTransform.InverseTransformDirection(cam.transform.forward);
        Vector3 camUp = GunSlideTransform.InverseTransformDirection(cam.transform.up);

        
       recoilOffset = camBack * recoilAmount;




        // the gun gets pushed back z axis
        Debug.DrawRay(GunSlideTransform.position, GunSlideTransform.forward * 0.5f, Color.red, 2f);


        Debug.Log("Recoil Offset: " + recoilOffset);

    }

    private void TryStunEnemy(RaycastHit hit)
    {
        // Check if we hit an enemy with ElkDemonAI
        ElkDemonAI elkDemon = hit.collider.GetComponent<ElkDemonAI>();

        if (elkDemon == null)
        {
            // Try parent or root if not on direct collider
            elkDemon = hit.collider.GetComponentInParent<ElkDemonAI>();
        }

        if (elkDemon != null)
        {
            Debug.Log($"Hit Elk Demon at: {hit.point}, on collider: {hit.collider.name}");

            // Optional: Check if it's a valid stun (not already stunned, not grabbing)
            if (!elkDemon.IsGrabbingPlayer)
            {
                elkDemon.GetStunned();

                // Visual feedback
                if (hitEffectPrefab != null)
                {
                    var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(fx, 2f);
                }
            }
            else
            {
                Debug.Log("Cannot stun while demon is grabbing player");
            }
        }
    }

    IEnumerator Reload()
    {
        Debug.Log("Reloading");
        if (anim != null) anim.SetTrigger("Reload");
        yield return new WaitForSeconds(stats.reloadTime);
        if (!(currentAmmoInGun >= stats.maxAmmo) || !(stats.pickedUpAmmo <= 0))
        {
            int needed = stats.maxAmmo - currentAmmoInGun;
            int bulletsToLoad = Mathf.Min(needed, stats.pickedUpAmmo);
            currentAmmoInGun += bulletsToLoad;
            stats.pickedUpAmmo -= bulletsToLoad;
        }
        Debug.Log("Reloaded " + " " + currentAmmoInGun);
    }

    void SpawnImpact(RaycastHit hit)
    {
        if (stats.bulletHolePrefab != null && !hit.collider.CompareTag("Enemy"))
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal);
            Vector3 pos = hit.point + hit.normal * 0.01f;
            var hole = Instantiate(stats.bulletHolePrefab, pos, rot);

            hole.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.08f, 0.12f);
            hole.transform.Rotate(0, 0, UnityEngine.Random.Range(0f, 360f));
            Destroy(hole, 1f);
        }

        if (hitEffectPrefab != null)
        {
            var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(fx, 2f);
        }
    }
}
