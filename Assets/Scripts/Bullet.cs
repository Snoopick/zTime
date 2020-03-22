using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public string[] targetTags = {"Enemy"};

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
//                Destroy(gameObject);
            }
        }
    }
}
