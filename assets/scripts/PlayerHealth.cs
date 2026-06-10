using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Player took damage!");

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log("Player HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died");
        // Later:
        // Respawn player
        // Load game over screen
    }
}