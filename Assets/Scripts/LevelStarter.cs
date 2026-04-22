using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class LevelStarter : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject lightsContainer;
    [SerializeField] float countdownDuration = 5f;
    [SerializeField] GameObject playerFlashlight;

    private bool _isActivated = false;

    public void Interact()
    {
        if (!_isActivated)
        {
            _isActivated = true;

            if (playerFlashlight != null)
            {
                playerFlashlight.SetActive(true);
            }

            Collider[] allColliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in allColliders)
            {
                col.enabled = false;
            }

            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer ren in allRenderers)
            {
                ren.enabled = false;
            }

            StartCoroutine(LightsOutSequence());
        }
    }

    private IEnumerator LightsOutSequence()
    {
        yield return new WaitForSeconds(countdownDuration);

        if (lightsContainer != null)
        {
            // Turn off all lights
            Light[] allLights = lightsContainer.GetComponentsInChildren<Light>();
            foreach (Light lights in allLights)
            {
                lights.enabled = false;
            }

            // Change light color
            Renderer[] allRenderers = lightsContainer.GetComponentsInChildren<Renderer>();

            foreach (Renderer ren in allRenderers)
            {
                foreach (Material mat in ren.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        mat.SetColor("_Color", Color.black);
                    }
                }
            }
            RenderSettings.ambientLight = Color.black;
        }
    }
}