using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Examples;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Box : MonoBehaviour
{
    [SerializeField] private GameObject[] loots;
    public bool isOpened = false;
    [SerializeField] private int itemCount = 1;
    [SerializeField] private bool isDroped = false;
    private bool isAddedItem = false;

    
    public void OpenBox()
    {
        for (int i = 0; i < itemCount; i++)
        {
            int randomFromList = Random.Range(0, loots.Length);

            if (!isDroped)
            {
                var obj = Instantiate(loots[randomFromList], transform.position, Quaternion.identity);
//                obj.AddComponent<Outline>();
//                obj.GetComponent<Outline>().OutlineColor = Color.green;
                isDroped = true;
                
                obj.transform.Translate(Time.deltaTime * 2.5f * Vector3.up);
                Destroy(obj.gameObject, 1.2f);
            }

            if (!isAddedItem)
            {
                GameObject.Find("GameManager").GetComponent<TurnBasedManager>().findedItems++;
                isAddedItem = true;
            }

            transform.Translate(Time.deltaTime * 2.5f * Vector3.down);
            Destroy(gameObject, 1f);
        }
    }

    private void Update()
    {
        if (isOpened)
        {
            OpenBox();
        }
    }
}
