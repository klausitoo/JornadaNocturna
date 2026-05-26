using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyInventory : MonoBehaviour
{
    public static PlayerKeyInventory Instance;

    private HashSet<KeyType> keys = new HashSet<KeyType>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddKey(KeyType keyType)
    {
        if (keyType == KeyType.Ninguna)
        {
            Debug.LogError("Error: esta llave está configurada como Ninguna.");
            return;
        }

        keys.Add(keyType);
        Debug.Log("Agarraste la llave: " + keyType);
    }

    public bool HasKey(KeyType keyType)
    {
        return keys.Contains(keyType);
    }
}