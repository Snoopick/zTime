using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Examples;
using UnityEngine;

public class EndTurn : MonoBehaviour
{
    [SerializeField] private TurnBasedManager gameManager;

    public void EndPlayerTurn()
    {
        Debug.Log("STATE 1" + gameManager.state);
        gameManager.state = TurnBasedManager.State.EnemyMove;
        Debug.Log("STATE 2" + gameManager.state);
    }
}
