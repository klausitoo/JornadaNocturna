using UnityEngine;
using TMPro;
using System.Collections;

public class IntroDialogue : MonoBehaviour
{
    public GameObject textObject;
    public float duration = 4f;

    IEnumerator Start()
    {
        textObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        textObject.SetActive(false);
    }
}