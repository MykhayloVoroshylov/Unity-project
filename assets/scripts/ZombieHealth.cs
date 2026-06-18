using Unity.Netcode;
using UnityEngine;

public class ZombieHealth : NetworkBehaviour
{
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private ZombieSpawner assignedSpawner;

    public void AssignSpawner(ZombieSpawner spawner)
    {
        assignedSpawner = spawner;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) currentHealth.Value = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsServer) return;

        currentHealth.Value -= damageAmount;

        if (currentHealth.Value <= 0)
            Die();
    }

    void Die()
    {
        if (!IsServer) return;

        if (assignedSpawner != null)
            assignedSpawner.ReturnZombie(gameObject);

        GetComponent<NetworkObject>().Despawn(true);
    }
}
