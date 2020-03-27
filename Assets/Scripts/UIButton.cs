using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    [SerializeField] private AudioClip hoverAudio;
    [SerializeField] private AudioClip clickAudio;
    private AudioSource audioSource;
    public LayerMask layerMask;

    private void Start()
    {
        Debug.Log("DONE");
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var button = GetByRay<UIButton>(ray);
        
        if (button != null && Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("Click");
            audioSource.PlayOneShot(hoverAudio);
        }
    }
    
    T GetByRay<T>(Ray ray) where T : class {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask)) {
            return hit.transform.GetComponentInParent<T>();
        }
        return null;
    }
}
