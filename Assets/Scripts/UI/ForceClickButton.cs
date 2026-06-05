using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ForceClickButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    public HealthSystem healthSystem;
    
    void Start()
    {
        // Intentar obtener HealthSystem
        if (healthSystem == null)
            healthSystem = FindFirstObjectByType<HealthSystem>();
            
        Debug.Log("ForceClickButton iniciado - Esperando clicks");
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CLICK DETECTADO POR OnPointerClick ");
        if (healthSystem != null)
            healthSystem.ResetGame();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(" CLICK DETECTADO POR OnPointerDown ");
        // También intentar aquí
        if (healthSystem != null)
            healthSystem.ResetGame();
    }
    
    void Update()
    {
        // Método alternativo: detectar clicks en toda la pantalla
        if (Input.GetMouseButtonDown(0) && gameObject.activeInHierarchy)
        {
            // Verificar si el mouse está sobre este objeto
            RectTransform rect = GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition))
            {
                Debug.Log("!!! CLICK DETECTADO POR Update !!!");
                if (healthSystem != null)
                    healthSystem.ResetGame();
            }
        }
    }
}