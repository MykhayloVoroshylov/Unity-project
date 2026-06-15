using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour 
{
    public int maxHealth = 100;
    
    // Automatically syncs health values from Host/Server -> all connected Clients
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn() 
    {
        if (IsServer) 
        {
            currentHealth.Value = maxHealth;
        }
        // Listen dynamically for updates to safely execute local UI changes
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int oldVal, int newVal) 
    {
        Debug.Log("Player HP updated: " + newVal + "/" + maxHealth);
        if (newVal <= 0) 
        {
            Die();
        }
    }

    public void TakeDamage(int damage) 
    {
        // Only the Server is authorized to compute damage calculations
        if (!IsServer) return;

        currentHealth.Value -= damage;
        if (currentHealth.Value < 0) currentHealth.Value = 0;
    }

    void Die() 
    {
        Debug.Log("Player has died.");
        // Add your respawn or game over overlay logic here
    }
}
