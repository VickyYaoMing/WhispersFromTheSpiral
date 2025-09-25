using UnityEngine;

public class GunController : MonoBehaviour
{
    public GunStats stats;
    public Transform muzzle;
    public Camera cam;
    public AudioSource audioSrc;
    public Animator anim;

    [Header("Hit VFX Settings")]
    public GameObject hitEffectPrefab; // Inspector connetted (exp. Bullet_Hit.prefab)

    float nextFire;
    int currentAmmo;

    void Start()
    {
        currentAmmo = stats.maxAmmo;
    }

    void Update()
    {
        // Otomatic and single fire control
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

        // reload
        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Reload());

        
    
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

        // Layer mask: HER ŞEYE çarpabilir
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

    System.Collections.IEnumerator Reload()
    {
        if (anim != null) anim.SetTrigger("Reload");
        yield return new WaitForSeconds(stats.reloadTime);
        currentAmmo = stats.maxAmmo;
    }

    // --- Impact (Bullet Hole + VFX) ---
    void SpawnImpact(RaycastHit hit)
    {
        // Bullet hole instantiate
        if (stats.bulletHolePrefab != null && !hit.collider.CompareTag("Enemy"))
        {
            // Doğru rotasyon (yüzeye yapışsın)
            Quaternion rot = Quaternion.LookRotation(hit.normal);

            // Yüzeye çok hafif mesafeli yerleştir (gömülmesin)
            Vector3 pos = hit.point + hit.normal * 0.01f;

            // Parent YOK! Dünya koordinatına instantiate
            var hole = Instantiate(stats.bulletHolePrefab, pos, rot);

            // Rastgele ölçek ve rotasyon (daha doğal görünüm)
            hole.transform.localScale = Vector3.one * Random.Range(0.08f, 0.12f);
            hole.transform.Rotate(0, 0, Random.Range(0f, 360f));

            // 15 saniye sonra otomatik sil
            Destroy(hole, 15f);
        }

        // Hit VFX (kıvılcım, toz)
        if (hitEffectPrefab != null)
        {
            var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(fx, 2f);
        }
    }
}
