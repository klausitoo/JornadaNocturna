using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameEndDoor : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    [SerializeField] Image fadeImage;
    [SerializeField] GameObject endLevelPanel;

    [Header("Settings")]
    [SerializeField] float fadeDuration = 1.5f; 
    [SerializeField] bool debugHasKey = false;

    private bool isLevelEnding = false;

    public void Interact()
    {
        Debug.Log("GAME END DOOR EJECUTADO");
        if (isLevelEnding) return;

        if (PlayerKeyInventory.Instance.HasKey(KeyType.Roja))
        {
            Debug.Log("Key accepted. Ending level...");
            isLevelEnding = true;
            StartCoroutine(EndLevelRoutine());
        }
        else
        {
            Debug.Log("Door is locked. You need the red key.");
        }
     
    }

    private IEnumerator EndLevelRoutine()
    {
        fadeImage.gameObject.SetActive(true);
        Color fadeColor = fadeImage.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }

        fadeColor.a = 1f;
        fadeImage.color = fadeColor;

        endLevelPanel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}