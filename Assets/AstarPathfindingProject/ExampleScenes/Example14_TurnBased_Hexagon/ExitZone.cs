using System.Collections;
using System.Collections.Generic;
using Pathfinding.Examples;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    [SerializeField] private TurnBasedManager gameManager;
    void OnTriggerEnter(Collider coll)
    {
        if(coll.transform.tag == "Player")
        {
            gameManager.state = TurnBasedManager.State.EndGame;
        }
    }
}
