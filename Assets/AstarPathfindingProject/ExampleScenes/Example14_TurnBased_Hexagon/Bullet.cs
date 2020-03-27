using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Examples;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public string[] targetTags = {"Enemy"};
    [SerializeField] private GameObject trail;

    private Vector3 shootDir;
    private Vector3 lastPos;

    private void Start()
    {
        lastPos = transform.position;

        bool isHardMode = GameObject.Find("GameManager").GetComponent<TurnBasedManager>().hardMode;
        if (isHardMode)
        {
            damage = 2;
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        foreach(string currentTag in targetTags)
        {
            if(currentTag == coll.transform.tag)
            {
                coll.transform.GetComponent<UnitHealth>().Hit(damage);
                Destroy(gameObject);
            }
            else
            {
                if (coll.transform.tag != "Player" && coll.transform.tag != null) 
                {
                    Debug.Log("Collider " + coll.transform.name);
                    Destroy(gameObject);                    
                }
            }
        }
    }

    public void Setup(Vector3 shootDir)
    {
        this.shootDir = shootDir;
        
        Vector3 ea = transform.eulerAngles;
	    transform.LookAt(shootDir);
	    transform.eulerAngles = new Vector3(ea.x, transform.eulerAngles.y, ea.z);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * 50f * Time.deltaTime);
    }
}
