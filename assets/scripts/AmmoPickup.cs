using Unity.Netcode;
using UnityEngine;

public class AmmoPickup : NetworkBehaviour
{
    public string targetWeaponName = "AK-47";
    public int ammoAmount = 30;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject networkObject = other.GetComponent<NetworkObject>();
        if (networkObject == null || !networkObject.IsOwner) return;

        Firearm[] playerGuns = other.GetComponentsInChildren<Firearm>(true);
        foreach (Firearm gun in playerGuns)
        {
            if (gun.weaponName != targetWeaponName) continue;

            gun.reservedAmmo += ammoAmount;
            DestroySelfServerRpc();
            break;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void DestroySelfServerRpc()
    {
        if (!IsServer) return;
        GetComponent<NetworkObject>().Despawn();
    }
}
