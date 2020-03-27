using System.Collections;
using System.Collections.Generic;
using Pathfinding.Examples;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public void PickUp()
    {
        GameObject.Find("GameManager").GetComponent<TurnBasedManager>().findedItems++;
        Destroy(gameObject);
    }
}
