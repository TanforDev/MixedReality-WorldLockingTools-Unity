using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInventory : MonoBehaviour
{
    [SerializeField] private GameObject[] invetoryPool;

    public GameObject SpawnItem(int index)
    {
        var newItem = invetoryPool[index];
        newItem.SetActive(true);
        return newItem;
    }
}
