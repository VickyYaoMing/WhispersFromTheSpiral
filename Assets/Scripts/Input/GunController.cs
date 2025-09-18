using UnityEngine;

public class GunController : MonoBehaviour
{
    public GunStats stats;
    public Transform muzzle;
    public Camera cam;
    public AudioSource audioSrc;
    public Animator anim;

    [Header("Hit VFX Settings")]
    public GameObject hitEffectPrefab; // Inspector'dan bağla (örn. Bullet_Hit.prefab)

    float nextFire;
    int currentAmmo;

    void Start()
    {
        currentAmmo = stats.maxAmmo;
    }

    void Update()
    {
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

    void TryShoot()
    {
        if (currentAmmo <= 0) return;

        nextFire = Time.time + stats.fireRate;
        currentAmmo--;
        Shoot();
    }

    void Shoot()
    {
        if (anim != null) anim.SetTrigger("Shoot");
        if (audioSrc != null && stats.shootSound != null) audioSrc.PlayOneShot(stats.shootSound);
        if (stats.muzzleFlashPrefab != null && muzzle != null)
            Instantiate(stats.muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, stats.range, ~0, QueryTriggerInteraction.Ignore))
        {
            SpawnImpact(hit); // ← tek yerden yönet
        }
    }

    System.Collections.IEnumerator Reload()
    {
        if (anim != null) anim.SetTrigger("Reload");
        yield return new WaitForSeconds(stats.reloadTime);
        currentAmmo = stats.maxAmmo;
    }

    // --- Impact (decal + hit effect) ---
    void SpawnImpact(RaycastHit hit)
    {
        // Duvarlarda siyah iz (Enemy'de bırakma)
        if (stats.bulletHolePrefab != null && !hit.collider.CompareTag("Enemy"))
        {
            Quaternion rot = Quaternion.LookRotation(-hit.normal);
            Vector3 pos = hit.point + hit.normal * 0.002f;
            var hole = Instantiate(stats.bulletHolePrefab, pos, rot, hit.collider.transform);
            hole.transform.localScale = Vector3.one * Random.Range(0.08f, 0.12f);
            hole.transform.Rotate(0, 0, Random.Range(0f, 360f));
            Destroy(hole, 15f);
        }

        // Anlık kıvılcım/toz
        if (hitEffectPrefab != null)
        {
            var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(fx, 2f);
        }
    }
}
