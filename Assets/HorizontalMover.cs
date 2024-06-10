using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalMover : MonoBehaviour
{
    private PlayerController playerController;

    public float baseSpeed = 3f; // Base speed of the object
    public float maxSpeed = 10f; // Maximum speed of the object
    public float timeToMaxSpeed = 60f; // Time over which the speed increases to maxSpeed
    public float specialSpeedMultiplier = 2f; // Multiplier for speed during special ability

    private float speed; // Actual speed of the object
    private float speedIncreaseTimer; // Timer to track the speed increase over time

    public float minY = -3f; // Minimum Y position for respawning
    public float maxY = 3f; // Maximum Y position for respawning

    public float minXOffset = -1f; // Minimum X offset for enemy spawn
    public float maxXOffset = 1f; // Maximum X offset for enemy spawn

    private Camera mainCamera;
    private float halfObjectWidth;
    private float cameraWidth;
    private float cameraHeight;

    private float enemySpawnTimer; // Timer for enemy spawning
    private bool canSpawnEnemies = false; // Control when enemies can start spawning
    private bool enemySpawned = false; // Track if an enemy has been spawned on the current platform

    public GameObject[] enemyPrefabs; // Array of enemy prefabs
    public float enemySpawnFrequency = 0.5f; // Frequency of enemy spawn (per second)
    public float enemySpawnDelay = 3f; // Initial delay before enemy spawn starts

    void Start()
    {
        mainCamera = Camera.main;
        halfObjectWidth = GetComponent<SpriteRenderer>().bounds.extents.x;
        cameraWidth = mainCamera.orthographicSize * mainCamera.aspect;
        cameraHeight = mainCamera.orthographicSize;

        // Initialize enemy spawn timer
        enemySpawnTimer = 1f / enemySpawnFrequency;

        // Start the coroutine to delay enemy spawning
        StartCoroutine(StartEnemySpawningAfterDelay(enemySpawnDelay));

        // Initialize speed and timer
        speed = baseSpeed;
        speedIncreaseTimer = 0f;

        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        MoveObject();
        CheckOffScreen();
        if (canSpawnEnemies && !enemySpawned)
        {
            SpawnEnemy();
        }

        // Adjust speed based on special ability
        if (playerController.isSpecialAbilityActive)
        {
            //speed = Mathf.Min(baseSpeed * specialSpeedMultiplier, maxSpeed * specialSpeedMultiplier);
            speed = maxSpeed * specialSpeedMultiplier;
        }
        else
        {
            // Increase speed over time
            speedIncreaseTimer += Time.deltaTime;
            float t = Mathf.Clamp01(speedIncreaseTimer / timeToMaxSpeed);
            speed = Mathf.Lerp(baseSpeed, maxSpeed, t);
            //Debug.Log("Current Speed" + speed);
        }
    }

    void MoveObject()
    {
        if (Time.timeScale == 0f) return; // Check if the game is paused

        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    void CheckOffScreen()
    {
        if (transform.position.x < mainCamera.transform.position.x - cameraWidth - halfObjectWidth)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        float respawnX = mainCamera.transform.position.x + cameraWidth + halfObjectWidth;
        float respawnY = Random.Range(minY, maxY);
        transform.position = new Vector3(respawnX, respawnY, transform.position.z);
        enemySpawned = false; // Reset the enemy spawn flag
    }

    void SpawnEnemy()
    {
        // Only spawn enemies if the platform is off-screen
        float leftBound = mainCamera.transform.position.x - cameraWidth;
        float rightBound = mainCamera.transform.position.x + cameraWidth;

        if (transform.position.x < leftBound || transform.position.x > rightBound)
        {
            // Decrease the spawn timer
            enemySpawnTimer -= Time.deltaTime;
            if (enemySpawnTimer <= 0)
            {
                // Randomly select an enemy prefab
                GameObject selectedEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

                // Calculate the spawn position on top of the platform
                float enemyX = transform.position.x + Random.Range(minXOffset, maxXOffset); // Add random X offset
                float platformTopY = transform.position.y + GetComponent<SpriteRenderer>().bounds.extents.y;
                float enemyHeight = selectedEnemyPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
                float enemyY = platformTopY + (enemyHeight / 2);

                // Instantiate the selected enemy prefab at the calculated position
                GameObject enemy = Instantiate(selectedEnemyPrefab, new Vector3(enemyX, enemyY, 0f), Quaternion.identity);

                // Make the enemy move with the platform
                enemy.transform.SetParent(transform);

                // Reset spawn timer
                enemySpawnTimer = 1f / enemySpawnFrequency;

                // Mark that an enemy has been spawned on this platform
                enemySpawned = true;
            }
        }
    }

    IEnumerator StartEnemySpawningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canSpawnEnemies = true;
    }
}
