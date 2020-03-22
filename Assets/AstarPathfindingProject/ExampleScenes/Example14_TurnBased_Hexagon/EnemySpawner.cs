using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Examples;
using UnityEngine;
using Pathfinding.Examples;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private bool isEnable = true;
    [SerializeField] private List<GameObject> enemyList;
    [SerializeField] private int enemyCount = 1;
    [SerializeField] private BlockManager blockManager;
    [SerializeField] private TurnBasedManager gameManager;
    public bool isSettedInTurn = false;

    private GameObject nearestUnit;
    
    
    // Start is called before the first frame update
    void Start()
    {
        SetEnemy();
    }

    public void SetEnemy()
    {
        if (!isEnable || enemyCount == 0 || enemyList.Count <= 0)
        {
            return;
        }


        int randomFromList = Random.Range(0, enemyList.Count);
        
        for (int i = 0; i < enemyCount; i++)
        {
            enemyList[randomFromList].GetComponent<SingleNodeBlocker>().manager = blockManager;
            enemyList[randomFromList].GetComponent<TurnBasedAI>().blockManager = blockManager;
            enemyList[randomFromList].GetComponent<UnitHealth>().gameManager = gameManager;
            enemyList[randomFromList].GetComponent<UnitHealth>().gameManager = gameManager;
            
            var obj = Instantiate(enemyList[randomFromList], new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);

            FindNearestUnit();
            
            Vector3 ea = transform.eulerAngles;
            obj.transform.LookAt(nearestUnit.transform);
            obj.transform.eulerAngles = new Vector3(ea.x, obj.transform.eulerAngles.y, ea.z);

            isSettedInTurn = true;
        }
    }
    
    void FindNearestUnit()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (nearestUnit == null)
                {
                    nearestUnit = players[i];
                }
            }
        }
    }
    
}
