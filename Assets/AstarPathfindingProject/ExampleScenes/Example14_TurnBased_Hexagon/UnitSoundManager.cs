using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSoundManager : MonoBehaviour
{
    // логично вынести в все звуки в контроллер, но на это нет времени :(
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip steps;
    [SerializeField] private AudioClip bite;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayShootSound()
    {
        audioSource.PlayOneShot(shoot);
    }

    public void StopPlayStepSound()
    {
        audioSource.loop = false;
        audioSource.clip = null;
        audioSource.Stop();
    }
    
    public void PlayStepSound()
    {
        audioSource.loop = true;
        audioSource.clip = steps;
        audioSource.Play();
//        audioSource.PlayOneShot(steps);
    }

    public void EnemyBite()
    {
        audioSource.PlayOneShot(bite);
    }
}
