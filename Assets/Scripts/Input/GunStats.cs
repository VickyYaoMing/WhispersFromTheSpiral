using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode { Single, Automatic }

[CreateAssetMenu(menuName = "Data/Gun Stats")]
public class GunStats : ScriptableObject
{
    [Header("General")]
    public FireMode fireMode = FireMode.Single;     // Fire mode: Single or Automatic
    public readonly int maxAmmo = 10;                        // Magazine capacity
    public float fireRate = 0.25f;                  // Time between shots
    public float reloadTime = 1.5f;                 // Time to reload
    public float damage = 10f;                      // Damage per shot
    public float range = 50f;                       // Max raycast distance
    public int pickedUpAmmo = 0;
    private int ammoCrateAmount = 2; //How much each ammo crate is worth

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;            // Muzzle flash effect
    public GameObject bulletHolePrefab;             // Bullet hole decal prefab
    public GameObject hitEffectPrefab;              // Optional hit effect (e.g. sparks, dust)

    [Header("Audio")]
    public AudioClip shootSound;                    // Sound played when shooting
    public AudioClip reloadSound;                   // Optional reload sound
    private void OnEnable()
    {
        AmmoInteraction.AmmoPickedUp += UpdateAmmoAmount;
    }

    private void OnDisable()
    {
        AmmoInteraction.AmmoPickedUp -= UpdateAmmoAmount;
    }

    private void UpdateAmmoAmount()
    {
        Debug.Log("ammo has been added");
        pickedUpAmmo += ammoCrateAmount;
    }
}
