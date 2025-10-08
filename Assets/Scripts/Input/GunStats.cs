using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode { Single, Automatic }

[CreateAssetMenu(menuName = "Data/Gun Stats")]
public class GunStats : ScriptableObject
{
    [Header("General")]
    public FireMode fireMode = FireMode.Single;   // Tekli / otomatik ate?
    public int maxAmmo = 12;                      // ?arj�r kapasitesi
    public float fireRate = 0.25f;                // At?? h?z? (saniye ba??na)
    public float reloadTime = 1.5f;               // ?arj�r doldurma s�resi
    public float damage = 10f;                    // Hasar (ileride enemy olursa laz?m)
    public float range = 50f;                     // Raycast menzili

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;          // Namlu �?k?? efekti
    public GameObject bulletHolePrefab;           // Duvar izi prefab�?
    public GameObject hitEffectPrefab;            // K?v?lc?m/toz efekti (opsiyonel)

    [Header("Audio")]
    public AudioClip shootSound;                  // Ate? sesi
    public AudioClip reloadSound;                 // ?arj�r sesi (opsiyonel)
}
