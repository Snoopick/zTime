using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Box : MonoBehaviour
{
    [SerializeField] private GameObject[] loots;
    public bool isOpened = false;
    [SerializeField] private int itemCount = 1;
    [SerializeField] private bool isDroped = false;

    
    public void OpenBox()
    {
        for (int i = 0; i < itemCount; i++)
        {
            int randomFromList = Random.Range(0, loots.Length);

            if (!isDroped)
            {
                var obj = Instantiate(loots[randomFromList], transform.position, Quaternion.identity);
                obj.AddComponent<Outline>();
                obj.GetComponent<Outline>().OutlineColor = Color.green;
                isDroped = true;
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
