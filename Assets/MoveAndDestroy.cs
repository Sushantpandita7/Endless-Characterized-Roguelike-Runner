using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAndDestroy : MonoBehaviour
{
    public float speed = 6f; // Speed at which the object moves to the left
    public float destroyXPosition = -28.7f; // X position at which the object will be destroyed

    void Update()
    {
        // Move the object to the left
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Check if the object's x position is less than the destroyXPosition
        if (transform.position.x < destroyXPosition)
        {
            // Destroy the object
            Destroy(gameObject);
        }
    }
}

