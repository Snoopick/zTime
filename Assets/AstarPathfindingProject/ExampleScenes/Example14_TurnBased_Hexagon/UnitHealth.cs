using System.Collections;
using System.Collections.Generic;
using Pathfinding.Examples;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public TurnBasedManager gameManager;
    public int health = 3;
    public Animator animator;
    private bool isDamaged = false;
    private Coroutine hitCoroutine;

    public void Hit(int damage)
    {
        if (hitCoroutine == null)
        {
            hitCoroutine = StartCoroutine(HitCoroutine(damage));
        }
    }

    IEnumerator HitCoroutine (int damage) 
    {
        int fHealth = health - damage;
        isDamaged = true;

        if (fHealth < 1)
        {
            animator.SetBool("Die", true);
            StartCoroutine(HideBody());
        }
        else
        {
            animator.SetTrigger("Hit");
        }
        
        yield return new WaitForSeconds(1f);

        health = fHealth;
        isDamaged = false;
        hitCoroutine = null;
        
    }
    
    IEnumerator HideBody () 
    {
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
        gameManager.GetEnemies();
    }
}

