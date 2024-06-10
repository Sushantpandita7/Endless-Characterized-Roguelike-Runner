using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 2; // Maximum health points
    private int currentHealth; // Current health points

    void Start()
    {
        currentHealth = maxHealth; // Initialize health
    }

    public void TakeDamage(int damage) // Ensure this method is public
    {
        //Debug.Log("Enemy took damage: " + damage); // Add this line to track damage taken by the enemy

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die() // Ensure this method is public
    {
        //Debug.Log("Enemy died"); // Add this line to track when the enemy dies

        // Notify the PlayerController
        FindObjectOfType<PlayerController>().EnemyDestroyed();

        // Handle enemy death (e.g., respawn outside of the scene or destroy)
        // For now, just destroy the enemy
        Destroy(gameObject);
    }
}
