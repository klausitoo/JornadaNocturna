using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("HealthConfig")]
    public int maxLives = 3;
    private int currentLives;

    [Header("UI References")]
    public Image[] lifeBars;
    public Color activeBarColor = Color.green;
    public Color depletedBarColor = Color.gray;

    [Header("Visual Effects")]
    public GameObject damageOverlay;
    public float damageOverlayDuration = 0.2f;
    public Image vignetteEffect;

    [Header("Audio")]
    public AudioClip damageSound;
    public AudioClip gameOverSound;
    public AudioSource audioSource;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

    private bool isGameOver = false;
    private PlayerMovement playerMovement;

    void Start()
    {
        currentLives = maxLives;
        playerMovement = GetComponent<PlayerMovement>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateLifeBarsUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (damageOverlay != null)
            damageOverlay.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("=== SISTEMA DE VIDA INICIADO ===");
        Debug.Log($"Vidas iniciales: {currentLives}/{maxLives}");
    }

    public void TakeDamage(string attackerName = "Unknown")
    {
        Debug.Log($"=== [ATAQUE RECIBIDO] ===");
        Debug.Log($"Atacante: {attackerName}");
        Debug.Log($"¿Juego terminado? {isGameOver}");
        Debug.Log($"¿Invulnerable? {isInvincible}");

        if (isGameOver) return;
        if (isInvincible) return;

        int previousLives = currentLives;
        currentLives--;

        Debug.Log($" Vidas: {previousLives} → {currentLives} (Perdió 1 vida)");

        if (currentLives <= 0)
        {
            Debug.Log(" ¡EL JUGADOR HA MUERTO! ");
            Debug.Log("GAME OVER - No quedan vidas");
        }
        else
        {
            Debug.Log($" Daño aplicado correctamente. Vidas restantes: {currentLives}/{maxLives}");
            Debug.Log($" Activando invencibilidad por {invincibilityDuration} segundos...");
        }

        PlayDamageEffects();
        UpdateLifeBarsUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }

    public void TakeDamage()
    {
        TakeDamage("Desconocido");
    }

    public void OnRestartButtonPressed()
    {
        Debug.Log("Restart button pressed - Reloading Scene");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void OnMenuButtonPressed()
    {
        Debug.Log("Menu button pressed - Loading Menu");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    void PlayDamageEffects()
    {
        Debug.Log(" Reproduciendo efectos de daño...");

        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
            Debug.Log("🔊 Reproduciendo sonido de daño");
        }

        if (damageOverlay != null)
        {
            StartCoroutine(ShowDamageOverlay());
            Debug.Log(" Mostrando overlay de daño");
        }

        StartCoroutine(CameraShake());
        Debug.Log(" Temblor de cámara activado");

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        Debug.Log(" Vibración activada");
#endif
    }

    IEnumerator ShowDamageOverlay()
    {
        damageOverlay.SetActive(true);
        yield return new WaitForSeconds(damageOverlayDuration);
        damageOverlay.SetActive(false);
    }

    IEnumerator CameraShake()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) yield break;

        Vector3 originalPos = mainCamera.transform.localPosition;
        float shakeDuration = 0.15f;
        float shakeMagnitude = 0.05f;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    IEnumerator InvincibilityFrames()
    {
        Debug.Log(" INVENCIBILIDAD ACTIVADA");
        isInvincible = true;

        if (vignetteEffect != null)
        {
            float elapsed = 0f;
            while (elapsed < invincibilityDuration)
            {
                vignetteEffect.color = new Color(1, 0, 0, Mathf.PingPong(elapsed * 4, 0.5f));
                elapsed += Time.deltaTime;
                yield return null;
            }
            vignetteEffect.color = Color.white;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }

        isInvincible = false;
        Debug.Log(" INVENCIBILIDAD DESACTIVADA");
    }

    void UpdateLifeBarsUI()
    {
        if (lifeBars == null || lifeBars.Length == 0) return;

        for (int i = 0; i < lifeBars.Length; i++)
        {
            if (i < currentLives)
            {
                lifeBars[i].color = activeBarColor;
                if (lifeBars[i].type == Image.Type.Filled)
                    lifeBars[i].fillAmount = 1f;
            }
            else
            {
                lifeBars[i].color = depletedBarColor;
                if (lifeBars[i].type == Image.Type.Filled)
                    lifeBars[i].fillAmount = 0f;
            }
        }
    }

    void GameOver()
    {
        isGameOver = true;

        if (gameOverSound != null && audioSource != null)
            audioSource.PlayOneShot(gameOverSound);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        DisableControls();
    }

    void DisableControls()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            MonoBehaviour cameraScript = mainCamera.GetComponent<MonoBehaviour>();
            if (cameraScript != null && cameraScript != this)
            {
                cameraScript.enabled = false;
            }
        }
    }

    void EnableControls()
    {
        if (playerMovement != null)
            playerMovement.enabled = true;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            MonoBehaviour cameraScript = mainCamera.GetComponent<MonoBehaviour>();
            if (cameraScript != null && cameraScript != this)
            {
                cameraScript.enabled = true;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    IEnumerator HealEffect()
    {
        if (damageOverlay != null)
        {
            damageOverlay.SetActive(true);
            Image overlayImage = damageOverlay.GetComponent<Image>();
            if (overlayImage != null)
            {
                overlayImage.color = new Color(0, 1, 0, 0.5f);
                yield return new WaitForSeconds(0.2f);
                overlayImage.color = new Color(1, 0, 0, 0.5f);
            }
            damageOverlay.SetActive(false);
        }
    }
}