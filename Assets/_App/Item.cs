using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private PresistentObject presistentObject;
    public PresistentObject PresistentObject
    {
        get
        {
            return presistentObject;
        }
        set
        {
            presistentObject = value;
        }
    }
}
