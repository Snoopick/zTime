using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    public GameObject BulletPrefab;
    public GameObject shootEffect;
    public float reloadTimer = 0f;
    public const float reloadCooldown = 0.1f;
    
    public void OnClick()
    {
        shootEffect.SetActive(true);
        if (reloadTimer > 0) reloadTimer -= Time.deltaTime; 
        if (reloadTimer <= 0) {
            reloadTimer = reloadCooldown;
            var instance = Instantiate(BulletPrefab, transform.position, Quaternion.identity);        
        }
        
        shootEffect.SetActive(false);
    }
}
