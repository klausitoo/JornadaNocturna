using UnityEngine;
using System.Collections;

public class LevelTransition : MonoBehaviour
{
   
    [SerializeField] DoorScript.Door specificDoor;
    [SerializeField] GameObject level1;
    [SerializeField] GameObject level2;

    [SerializeField] float waitTime = 1.5f;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            StartCoroutine(SequenceTransition());
        }
    }

    IEnumerator SequenceTransition()
    {
        // Close the door
        if (specificDoor != null)
        {
            specificDoor.ForceClose();
        }

        // Wait until the door is closed
        yield return new WaitForSeconds(waitTime);

        // Level change
        if (level1 != null) level1.SetActive(false);
        if (level2 != null) level2.SetActive(true);

    }
}