using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class LevelStarter : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject levelObjects;
    [SerializeField] float countdownDuration = 5f;
    [SerializeField] GameObject playerFlashlight;
    [SerializeField] AudioSource lightsOutSound;

    public static bool cosasRecogidas = false;

    private bool _isActivated = false;

    public void Interact()
    {



        if (!_isActivated)
        {

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

        _isActivated = true;
        cosasRecogidas = true;

        if (levelObjects != null)
        {
            // Turn off all lights
            Light[] allLights = levelObjects.GetComponentsInChildren<Light>();
            foreach (Light lights in allLights)
            {
                lights.enabled = false;
            }

            if (lightsOutSound != null)
            {
                lightsOutSound.Play();
            }

            // Change light color
            Renderer[] allRenderers = levelObjects.GetComponentsInChildren<Renderer>();

            foreach (Renderer ren in allRenderers)
            {
                foreach (Material mat in ren.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        mat.SetColor("_Color", new Color32(8, 8, 8, 255));
                    }
                }
            }
            RenderSettings.ambientLight = new Color32(8, 8, 8, 255);
        }
    }
}