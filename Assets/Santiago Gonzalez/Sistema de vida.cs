using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Sistemadevida : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxLives = 3;
    private int currentLives;
    
    [Header("Referencias UI")]
    public Image[] lifeBars; // Las 3 barras de vida en la UI
    public Color activeBarColor = Color.green;
    public Color depletedBarColor = Color.gray;
    
    [Header("Efectos Visuales")]
    public GameObject damageOverlay; // Pantalla roja al recibir daño
    public float damageOverlayDuration = 0.2f;
    public Image vignetteEffect; // Efecto de viñeta opcional
    
    [Header("Audio")]
    public AudioClip damageSound;
    public AudioClip gameOverSound;
    public AudioSource audioSource;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    
    [Header("Invulnerabilidad")]
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;
    
    private bool isGameOver = false;
    private PlayerMovement playerMovement; 
    
    // Para debugging
    private string lastAttacker = "";
    
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
    
    /// <summary>
    /// Aplica daño al jugador
    /// </summary>
    /// <param name="attackerName">Nombre del atacante (opcional)</param>
    public void TakeDamage(string attackerName = "Desconocido")
    {
        Debug.Log($"=== [ATAQUE RECIBIDO] ===");
        Debug.Log($"Atacante: {attackerName}");
        Debug.Log($"¿Juego terminado? {isGameOver}");
        Debug.Log($"¿Invulnerable? {isInvincible}");
        
        // Verificar si el juego ya terminó
        if (isGameOver)
        {
            Debug.Log(" DAÑO RECHAZADO: El juego ya terminó (Game Over)");
            return;
        }
        
        // Verificar si es invencible
        if (isInvincible)
        {
            Debug.Log($" DAÑO RECHAZADO: El jugador es invencible (dura {invincibilityDuration} segundos después de recibir daño)");
            return;
        }
        
        // Guardar vidas antes del daño
        int previousLives = currentLives;
        
        // Reducir vida
        currentLives--;
        
        Debug.Log($" Vidas: {previousLives} → {currentLives} (Perdió 1 vida)");
        
        // Verificar si el jugador murió
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
        
        // Efectos
        PlayDamageEffects();
        UpdateLifeBarsUI();
        
        // Verificar muerte
        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }
    
    // Sobrecarga del método para mantener compatibilidad
    public void TakeDamage()
    {
        TakeDamage("Desconocido");
    }
    
    void PlayDamageEffects()
    {
        Debug.Log(" Reproduciendo efectos de daño...");
        
        // Sonido de daño
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
            Debug.Log("🔊 Reproduciendo sonido de daño");
        }
        
        // Overlay de pantalla roja
        if (damageOverlay != null)
        {
            StartCoroutine(ShowDamageOverlay());
            Debug.Log(" Mostrando overlay de daño");
        }
        
        // Animación de la cámara
        StartCoroutine(CameraShake());
        Debug.Log(" Temblor de cámara activado");
        
        // Feedback háptico
        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        Debug.Log(" Vibración activada");
        #endif
    }
    
    /// Muestra overlay rojo al recibir daño
    IEnumerator ShowDamageOverlay()
    {
        damageOverlay.SetActive(true);
        yield return new WaitForSeconds(damageOverlayDuration);
        damageOverlay.SetActive(false);
    }
    
    /// Pequeño temblor de cámara al recibir daño
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
    
    /// Frames de invencibilidad después del daño
    IEnumerator InvincibilityFrames()
    {
        Debug.Log(" INVENCIBILIDAD ACTIVADA");
        isInvincible = true;
        
        // Efecto de parpadeo en pantalla
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
        Debug.Log(" INVENCIBILIDAD DESACTIVADA - El jugador puede recibir daño nuevamente");
    }
    
    /// Actualiza las 3 barras de vida visualmente
    void UpdateLifeBarsUI()
    {
        if (lifeBars == null || lifeBars.Length == 0)
        {
            Debug.LogWarning(" No hay barras de vida asignadas en el Inspector");
            return;
        }
        
        Debug.Log($"🎨 Actualizando UI de vidas: {currentLives}/{maxLives} barras activas");
        
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
        
        Debug.Log("========================================");
        Debug.Log("  GAME OVER - EL JUGADOR HA PERDIDO ");
        Debug.Log($" Vidas finales: {currentLives}/{maxLives}");
        Debug.Log("========================================");
        
        // Sonido de game over
        if (gameOverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gameOverSound);
            Debug.Log(" Reproduciendo sonido de Game Over");
        }
        
        // Mostrar panel de game over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log(" Mostrando panel de Game Over");
            
            // Liberar cursor para el menú
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log(" Cursor liberado");
        }
        
        DisableControls();
    }
    
    /// Desactiva todos los controles 
    void DisableControls()
    {
        Debug.Log(" Desactivando controles del jugador...");
        
        // Desactivar PlayerMovement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log(" PlayerMovement desactivado");
        }
        
        // Opcional: También puedes desactivar componentes de la cámara
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            MonoBehaviour cameraScript = mainCamera.GetComponent<MonoBehaviour>();
            if (cameraScript != null && cameraScript != this)
            {
                cameraScript.enabled = false;
                Debug.Log(" Script de cámara desactivado");
            }
        }
    }
    
    /// Reactiva los controles 
    void EnableControls()
    {
        Debug.Log(" Reactivando controles del jugador...");
        
        // Reactivar PlayerMovement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log(" PlayerMovement reactivado");
        }
        
        // Reactivar scripts de cámara
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            MonoBehaviour cameraScript = mainCamera.GetComponent<MonoBehaviour>();
            if (cameraScript != null && cameraScript != this)
            {
                cameraScript.enabled = true;
                Debug.Log(" Script de cámara reactivado");
            }
        }
        
        // Bloquear cursor 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log(" Cursor bloqueado para FPS");
    }
    
    /// Reinicia el juego completamente
    public void ResetGame()
    {
        Debug.Log("========================================");
        Debug.Log(" REINICIANDO JUEGO");
        Debug.Log("========================================");
        
        // Reanudar tiempo si estaba pausado
        Time.timeScale = 1f;
        
        // Resetear vidas
        currentLives = maxLives;
        isGameOver = false;
        isInvincible = false;
        
        Debug.Log($" Vidas restablecidas: {currentLives}/{maxLives}");
        
        // Resetear UI
        UpdateLifeBarsUI();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Reactivar controles 
        EnableControls();
        
        // Resetear posición del jugador
        ResetPlayerPosition();
        
        Debug.Log("Juego reiniciado correctamente");
        Debug.Log("========================================");
    }
    
    /// Reinicia la posición del jugador al spawn point
    void ResetPlayerPosition()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;
            Debug.Log($"📍 Jugador reposicionado en spawn point: {spawnPoint.name}");
        }
        else
        {
            Debug.LogWarning(" No se encontró un spawn point con tag 'Respawn'");
        }
        
        // Resetear velocidad si tiene CharacterController
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            Debug.Log(" CharacterController reseteado");
        }
        
        // Resetear Rigidbody si usa
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log(" Rigidbody reseteado (velocidad = 0)");
        }
    }
    
    /// Métodos públicos para otros scripts
    public int GetCurrentLives() 
    { 
        Debug.Log($"🔍 Consultando vidas actuales: {currentLives}/{maxLives}");
        return currentLives;
    }
    
    public bool IsGameOver() 
    {
        Debug.Log($"🔍 Consultando estado Game Over: {isGameOver}");
        return isGameOver;
    }
    
    public bool IsInvincible() 
    {
        Debug.Log($"🔍 Consultando estado invencible: {isInvincible}");
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