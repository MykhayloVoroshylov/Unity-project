using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 100;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("UI Reference")]
    public Image healthBarFill;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer) currentHealth.Value = maxHealth;
        currentHealth.OnValueChanged += OnHealthChanged;
        UpdateHealthBar(currentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
        base.OnNetworkDespawn();
    }

    void OnHealthChanged(int oldVal, int newVal)
    {
        UpdateHealthBar(newVal);
        if (newVal <= 0) Die();
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damage, 0, maxHealth);
    }

    void UpdateHealthBar(int health)
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)health / maxHealth;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
    }
}
