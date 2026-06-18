using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MeleeAttack : NetworkBehaviour
{
    [Header("Knife Animation Settings")]
    public Transform knifeTransform;
    public Vector3 attackOffset = new Vector3(0, 0, 0.5f);
    public float attackSpeed = 0.1f;

    [Header("Combat Settings")]
    public float attackRange = 2f;
    public int damageAmount = 25;
    public Camera playerCamera;

    private Vector3 originalPosition;
    private bool isAttacking = false;

    void Start()
    {
        if (knifeTransform != null)
            originalPosition = knifeTransform.localPosition;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(ThrustKnife());
            PerformMeleeRaycast();
        }
    }

    IEnumerator ThrustKnife()
    {
        if (knifeTransform == null) yield break;

        isAttacking = true;
        Vector3 targetPosition = originalPosition + attackOffset;

        float targetTime = 0f;
        while (targetTime < attackSpeed)
        {
            knifeTransform.localPosition = Vector3.Lerp(originalPosition, targetPosition, targetTime / attackSpeed);
            targetTime += Time.deltaTime;
            yield return null;
        }

        targetTime = 0f;
        while (targetTime < attackSpeed)
        {
            knifeTransform.localPosition = Vector3.Lerp(targetPosition, originalPosition, targetTime / attackSpeed);
            targetTime += Time.deltaTime;
            yield return null;
        }

        knifeTransform.localPosition = originalPosition;
        isAttacking = false;
    }

    void PerformMeleeRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, attackRange)) return;

        if (hit.collider.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            DamageServerRpc(netObj.NetworkObjectId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void DamageServerRpc(ulong targetNetworkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetNetObj))
            return;

        if (targetNetObj.TryGetComponent<ZombieHealth>(out ZombieHealth zombieHealth))
            zombieHealth.TakeDamage(damageAmount);
    }
}
