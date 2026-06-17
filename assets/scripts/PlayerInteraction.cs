using UnityEngine;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    public float interactionDistance = 3f;

    private bool HasInputAuthority => !IsSpawned || IsOwner;

    void Update()
    {
        if (!HasInputAuthority) return;

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
                RequestPickupServerRpc(pickup.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }

    [ServerRpc]
    void RequestPickupServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject targetNetObj))
        {
            targetNetObj.Despawn();
        }
    }
}
