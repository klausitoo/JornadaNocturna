using UnityEngine;

public class KeyItem : MonoBehaviour ,IInteractable
{
    [SerializeField] private KeyType keyType = KeyType.Roja;

    

    public void Interact()
    {
        PlayerKeyInventory.Instance.AddKey(keyType);
        Destroy(gameObject);
    }
}