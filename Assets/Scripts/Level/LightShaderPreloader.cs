using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShaderPrewarmer : MonoBehaviour
{
    public Light flashlight;
    public GameObject levelObjects;

    private Light[] allSceneLights;
    private Dictionary<Light, bool> originalLightStates = new Dictionary<Light, bool>();

    void Start()
    {
        StartCoroutine(PrewarmShaders());
    }

    private IEnumerator PrewarmShaders()
    {
        if (levelObjects == null)
        {
            Debug.LogError("Falta asignar el objeto LevelObjects en el Prewarmer");
            yield break;
        }

        allSceneLights = levelObjects.GetComponentsInChildren<Light>();

        foreach (Light sceneLight in allSceneLights)
        {
            if (sceneLight == flashlight) continue;

            originalLightStates[sceneLight] = sceneLight.enabled;
            sceneLight.enabled = false;
        }

        bool originalFlashlightState = flashlight.enabled;
        flashlight.enabled = true;

        yield return null;

        foreach (Light sceneLight in allSceneLights)
        {
            if (sceneLight == flashlight) continue;

            sceneLight.enabled = originalLightStates[sceneLight];
        }

        flashlight.enabled = originalFlashlightState;
    }
}