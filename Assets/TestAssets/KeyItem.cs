using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [SerializeField] private KeyType keyType = KeyType.Ninguna;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerKeyInventory inventory = other.GetComponent<PlayerKeyInventory>();

            if (inventory != null)
            {
                inventory.AddKey(keyType);
                Destroy(gameObject);
            }
        }
    }
}