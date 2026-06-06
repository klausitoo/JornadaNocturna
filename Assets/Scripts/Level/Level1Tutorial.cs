using UnityEngine;
using TMPro;
using System.Collections;

public class TriggerTextDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI textElement;

    [Header("Configuration")]
    [TextArea(3, 5)]
    [SerializeField] string messageToShow;
    [SerializeField] float displayDuration = 7f;
    [SerializeField] bool disableTriggerAfterUse = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(ShowAndHideTextRoutine());

            if (disableTriggerAfterUse)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }

    private IEnumerator ShowAndHideTextRoutine()
    {
        textElement.gameObject.SetActive(true);
        textElement.text = messageToShow;

        yield return new WaitForSeconds(displayDuration);

        textElement.gameObject.SetActive(false);
    }
}