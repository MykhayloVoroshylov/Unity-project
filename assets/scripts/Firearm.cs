using Unity.Netcode;
using UnityEngine;

public class Firearm : NetworkBehaviour
{
    public string weaponName = "AK-47";
    public int damage = 25;
    public int magazineSize = 30;
    public int currentAmmo = 30;
    public int reservedAmmo = 90;
    public float fireRate = 0.1f;

    private float nextFireTime;

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && currentAmmo > 0)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
            Reload();
    }

    void Shoot()
    {
        currentAmmo--;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        ZombieHealth zombie = hit.collider.GetComponent<ZombieHealth>();
        if (zombie == null) return;

        NetworkObject zombieNetObj = zombie.GetComponent<NetworkObject>();
        if (zombieNetObj == null) return;

        RequestDamageEnemyServerRpc(zombieNetObj.NetworkObjectId, damage);
    }

    void Reload()
    {
        if (currentAmmo >= magazineSize || reservedAmmo <= 0) return;

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, reservedAmmo);

        reservedAmmo -= ammoToLoad;
        currentAmmo += ammoToLoad;
    }

    [ServerRpc]
    void RequestDamageEnemyServerRpc(ulong zombieNetId, int damageValue)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(zombieNetId, out NetworkObject netObj))
            return;

        ZombieHealth healthScript = netObj.GetComponent<ZombieHealth>();
        if (healthScript != null)
            healthScript.TakeDamage(damageValue);
    }
}
