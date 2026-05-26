using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField]
    private GameObject staminaBarContainer;
    [SerializeField]
    private Image staminaBar;
    [SerializeField]
    private PlayerMovement playerMovement;

    private void Update()
    {
        UpdateStaminaBar();
    }

    private void UpdateStaminaBar()
    {
        if (playerMovement.CurrentStamina == playerMovement.MaxStamina)
        {
            staminaBarContainer.SetActive(false);
        }
        else if (staminaBarContainer.activeSelf == false)
        {
            staminaBarContainer.SetActive(true);
        }
        staminaBar.fillAmount = playerMovement.CurrentStamina / playerMovement.MaxStamina;
    }
}
