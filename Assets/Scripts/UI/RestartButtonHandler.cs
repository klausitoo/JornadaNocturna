using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class RestartButtonHandler : MonoBehaviour
{
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(RestartGame);
            Debug.Log("Botón configurado - Reiniciará la escena");
        }
    }
    
    void RestartGame()
    {
        Debug.Log("=== REINICIANDO ESCENA ===");
        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}