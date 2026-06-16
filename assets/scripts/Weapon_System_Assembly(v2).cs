using UnityEngine;
using System.Collections;
using Unity.Netcode;

// ==========================================
// 1. WEAPON MANAGER & INTERACTION
// ==========================================
public class PlayerInteraction : NetworkBehaviour
{
    public float interactionDistance = 3f;

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformInteractionRaycast();
        }
    }

    void PerformInteractionRaycast()
    {
        Ray centerRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(centerRay, out hit, interactionDistance))
        {
            WeaponGroundPickup pickup = hit.collider.GetComponent<WeaponGroundPickup>();
            if (pickup != null)
            {
                // Tell the server we want to pick this up across the network
                RequestPickupServerRpc(pickup.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }

    [ServerRpc]
    void RequestPickupServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject targetNetObj))
        {
            // Server side pickup execution/despawn logic
            targetNetObj.Despawn(); 
        }
    }
}

// ==========================================
// 2. FIREARM (GUN WEAPON SCRIPT)
// ==========================================
public class Firearm : NetworkBehaviour
{
    public string weaponName = "AK-47";
    public int damage = 25;
    public int currentAmmo = 30;
    public int reservedAmmo = 90;
    public float fireRate = 0.1f;
    private float nextFireTime = 0f;

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentAmmo > 0)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        currentAmmo--;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ZombieHealth zombie = hit.collider.GetComponent<ZombieHealth>();
            if (zombie != null)
            {
                // Request the server to deduct health from the hit Zombie ID
                RequestDamageEnemyServerRpc(zombie.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage);
            }
        }
    }

    [ServerRpc]
    void RequestDamageEnemyServerRpc(ulong zombieNetId, int damageValue)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(zombieNetId, out NetworkObject netObj))
        {
            ZombieHealth healthScript = netObj.GetComponent<ZombieHealth>();
            if (healthScript != null) healthScript.TakeDamage(damageValue);
        }
    }
}

// ==========================================
// 3. WEAPON GROUND PICKUP
// ==========================================
public class WeaponGroundPickup : MonoBehaviour
{
    public string weaponName;
}

// ==========================================
// 4. AMMO PICKUP BOX
// ==========================================
public class AmmoPickup : MonoBehaviour
{
    public string targetWeaponName = "AK-47";
    public int ammoAmount = 30;

    void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is a valid network player instance
        if (other.CompareTag("Player"))
        {
            var networkObject = other.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                Firearm[] playerGuns = other.GetComponentsInChildren<Firearm>(true);
                foreach (Firearm gun in playerGuns)
                {
                    if (gun.weaponName == targetWeaponName)
                    {
                        gun.reservedAmmo += ammoAmount;
                        DestroySelfServerRpc();
                        break;
                    }
                }
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void DestroySelfServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}