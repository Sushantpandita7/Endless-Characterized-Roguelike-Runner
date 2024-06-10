using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // Speed of the bullet
    public float lifetime = 2f; // Lifetime of the bullet before it gets destroyed

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        Destroy(gameObject, lifetime); // Destroy the bullet after a certain time to prevent memory leaks
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enemy2"))
        {
            // Apply damage to the enemy
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1); // Assuming 1 damage per bullet hit
            }

            // Destroy the bullet on collision
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Platform"))
        {
            // Destroy the bullet if it hits something else that's not a platform
            Destroy(gameObject);
        }
    }

}
