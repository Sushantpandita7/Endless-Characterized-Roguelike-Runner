using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 10f;
    public float midAirJumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool hasPerformedMidAirJump;

    public int maxHealth = 100;
    private int currentHealth;

    public Slider healthBar;
    public Slider bulletSlider;
    public Slider enemySlider;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public int maxBullets = 5;
    public float fireRate = 1f;
    public float replenishRate = 1f;
    private int currentBullets;
    private float lastFireTime;

    private int enemiesDestroyed;
    public int maxEnemiesDestroyed = 3;

    public int enemyDamage = 40;

    public bool isSpecialAbilityActive = false;
    public float specialAbilityDuration = 5f;
    public float sizeMultiplier = 1.5f;
    private Vector3 originalScale;

    public float fallThresholdY = -10f; // Define the y position threshold for falling
    public float fallThresholdX = -12.76f; // Define the x position threshold for falling

    public GameObject gameOverScreen;

    private bool isGamePaused = false; // Track whether the game is paused

    public TextMeshProUGUI scoreText; // Reference to the score text UI
    private float elapsedTime; // Track the time elapsed since the start
    private int totalEnemiesKilled; // Track the total number of enemies killed

    AudioSource jumpSound;
    AudioSource shootSound;
    AudioSource killSound;

    private Animator animator;

    public float healthBar_Yoffset = 1f;
    public float healthBar_Xoffset = 1f;

    public GameObject specialImagePrefab; // Reference to the prefab of the special image
    private GameObject specialImageInstance; // Instance of the special image displayed in the game
    private bool hasDisplayedSpecialImage = false; // Flag to track if the special image has been displayed

    private bool isPausedFromScore = false; // Flag to track if the game is paused from reaching a certain score
    private float pausedElapsedTime; // Time elapsed at the point of pausing
    private int pausedTotalEnemiesKilled; // Total enemies killed at the point of pausing

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        isGrounded = false;
        hasPerformedMidAirJump = false;

        currentHealth = maxHealth;
        healthBar.value = CalculateHealth();

        currentBullets = maxBullets;
        bulletSlider.maxValue = maxBullets;
        bulletSlider.value = currentBullets;

        enemiesDestroyed = 0;
        enemySlider.maxValue = maxEnemiesDestroyed;
        enemySlider.value = enemiesDestroyed;

        originalScale = transform.localScale;

        StartCoroutine(ReplenishBullets());

        UpdateBulletCounter();
        UpdateEnemyCounter();
        UpdateScore();

        // Assign the audio sources
        AudioSource[] audioSources = GetComponents<AudioSource>();

        jumpSound = audioSources[0];
        shootSound = audioSources[1];
        killSound = audioSources[2];
    }

    void Update()
    {
        if (isGamePaused)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                RestartGame();
            }
            return;
        }

        // Update the health bar's position to follow the player
        Vector3 healthBarOffset = new Vector3(healthBar_Xoffset, healthBar_Yoffset, 0); // Adjust the offset as needed
        healthBar.transform.position = transform.position + healthBarOffset;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump(jumpForce);
            }
            else if (!hasPerformedMidAirJump)
            {
                Jump(midAirJumpForce);
                hasPerformedMidAirJump = true;
            }
        }

        if (Input.GetButtonDown("Fire1"))
        {
            FireBullet();
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * Time.deltaTime;
        }

        if (transform.position.y < fallThresholdY || transform.position.x < fallThresholdX)
        {
            currentHealth = 0;
            Die();
        }

        elapsedTime += Time.deltaTime;
        UpdateScore();
    }

    void Jump(float force)
    {
        if (jumpSound != null)
        {
            jumpSound.Play();
        }
        rb.velocity = new Vector2(rb.velocity.x, force);
        animator.SetBool("Jump", true);
    }

    void FireBullet()
    {
        if (currentBullets > 0 && Time.time >= lastFireTime + 1f / fireRate)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            currentBullets--;
            lastFireTime = Time.time;
            UpdateBulletCounter();

            if (shootSound != null)
            {
                shootSound.Play();
            }

            if (isGrounded == false && rb.velocity.y < 0)
            {
                StartCoroutine(HoverInAir(0.2f));
            }
        }
    }

    IEnumerator HoverInAir(float duration)
    {
        float originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(rb.velocity.x, 0); // Stop vertical movement
        yield return new WaitForSeconds(duration);
        rb.gravityScale = originalGravityScale; // Restore original gravity scale
    }

    IEnumerator ReplenishBullets()
    {
        while (true)
        {
            yield return new WaitForSeconds(replenishRate);
            if (currentBullets < maxBullets)
            {
                currentBullets++;
                UpdateBulletCounter();
            }
        }
    }

    void UpdateBulletCounter()
    {
        if (bulletSlider != null)
        {
            bulletSlider.value = currentBullets;
        }
    }

    void UpdateEnemyCounter()
    {
        if (enemySlider != null)
        {
            enemySlider.value = enemiesDestroyed;
        }

        if (enemiesDestroyed >= maxEnemiesDestroyed)
        {
            ActivateSpecialAbility();
        }
    }

    void ActivateSpecialAbility()
    {
        if (!isSpecialAbilityActive)
        {
            isSpecialAbilityActive = true;
            enemiesDestroyed = 0;

            UpdateEnemyCounter();

            transform.localScale = originalScale * sizeMultiplier;

            StartCoroutine(SpecialAbilityDuration());
        }
    }

    IEnumerator SpecialAbilityDuration()
    {
        yield return new WaitForSeconds(specialAbilityDuration);

        DeactivateSpecialAbility();
    }

    void DeactivateSpecialAbility()
    {
        transform.localScale = originalScale;
        isSpecialAbilityActive = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        bool isEnemyCollision = false;
        Enemy enemy = null;
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enemy2"))
        {
            isEnemyCollision = true;
            enemy = collision.gameObject.GetComponent<Enemy>();
        }
        else
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.collider.CompareTag("Enemy") || contact.collider.CompareTag("Enemy2"))
                {
                    isEnemyCollision = true;
                    enemy = contact.collider.GetComponent<Enemy>();
                    break;
                }
            }
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
            hasPerformedMidAirJump = false;
            animator.SetBool("Jump", false);
        }

        if (isEnemyCollision)
        {
            TakeDamage(enemyDamage);
            if (enemy != null)
            {
                enemy.Die();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;
        }
    }

    public void EnemyDestroyed()
    {
        if (killSound != null)
        {
            killSound.Play();
        }

        if (isSpecialAbilityActive)
        {
            totalEnemiesKilled++;
            return;
        }

        enemiesDestroyed++;
        totalEnemiesKilled++;
        UpdateEnemyCounter();
    }

    public void TakeDamage(int damage)
    {
        if (isSpecialAbilityActive)
        {
            return;
        }

        currentHealth -= damage;
        healthBar.value = CalculateHealth();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    float CalculateHealth()
    {
        return (float)currentHealth / maxHealth;
    }

    void Die()
    {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    public void RestartGame()
    {
        isGamePaused = false;
        gameOverScreen.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateScore()
    {
        float score = (elapsedTime * 2) + (totalEnemiesKilled * 50);
        scoreText.text = "Score: " + score.ToString("F0");

        // Check if the score has crossed 250 points and the special image hasn't been displayed yet
        if (score >= 100 && !hasDisplayedSpecialImage)
        {
            PauseGameAndDisplayImage();
        }
    }

    void PauseGameAndDisplayImage()
    {
        Debug.Log("PauseGameAndDisplayImage method called.");

        // Pause the game
        Time.timeScale = 0f;
        isGamePaused = true;
        isPausedFromScore = true;

        // Save the game state
        pausedElapsedTime = elapsedTime;
        pausedTotalEnemiesKilled = totalEnemiesKilled;

        if (specialImagePrefab != null)
        {
            // Instantiate the special image prefab and assign it to specialImageInstance
            specialImageInstance = Instantiate(specialImagePrefab);

            // Ensure the special image instance is positioned correctly (adjust as needed)
            specialImageInstance.transform.position = Vector3.zero;

            // Start a coroutine to resume the game after 3 seconds
            StartCoroutine(ResumeGameAfterDelay(3f));

            hasDisplayedSpecialImage = true; // Set the flag to true to indicate the special image has been displayed
            Debug.Log("Special image activated.");
        }
        else
        {
            Debug.LogError("specialImagePrefab is not assigned!");
        }
    }

    IEnumerator ResumeGameAfterDelay(float delay)
    {
        Debug.Log("ResumeGameAfterDelay coroutine started with delay: " + delay);

        // Wait for the specified delay
        yield return new WaitForSecondsRealtime(delay);

        // Deactivate the special image
        specialImageInstance.SetActive(false);

        Debug.Log("Special image deactivated.");

        // If paused from reaching 250 points, restore game state
        if (isPausedFromScore)
        {
            elapsedTime = pausedElapsedTime;
            totalEnemiesKilled = pausedTotalEnemiesKilled;
            UpdateScore();

            // Reset the flag
            isPausedFromScore = false;

            Debug.Log("Game state restored. elapsedTime: " + elapsedTime + ", totalEnemiesKilled: " + totalEnemiesKilled);
        }

        // Resume the game
        Time.timeScale = 1f;
        isGamePaused = false;

        Debug.Log("Game resumed.");
    }
}
 