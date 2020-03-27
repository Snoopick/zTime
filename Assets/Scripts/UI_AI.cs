using System;
using System.Collections.Generic;
using Pathfinding.Examples;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UI_AI : MonoBehaviour
{
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject gameMenuUI;
    [SerializeField] private PanelManager menuPanelManager;
    [SerializeField] private GameObject endTurnButton;
    [SerializeField] private GameObject player;
    
    private AudioSource audioSource;
    [SerializeField] private AudioClip hoverAudio;
    [SerializeField] private AudioClip clickAudio;
    [SerializeField] private AudioClip endTurn;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void EndPlayerTurn()
    {
        audioSource.PlayOneShot(endTurn);
        var states = GameObject.Find("GameManager").GetComponent<TurnBasedManager>();
        states.state = TurnBasedManager.State.EnemyMove;
        states.UpdateTurn();
    }

    public void ShowMenu()
    {
        gameUI.SetActive(false);
        gameMenuUI.SetActive(true);
    }

    public void HideMenu()
    {
        menuPanelManager.CloseCurrent();
        StartCoroutine(WaitOnCloseMenu());

    }

    public void StartGame()
    {
        SceneManager.LoadScene("Map1");
    }
    
    public void StartHardGame()
    {
        SceneManager.LoadScene("Map1_hard");
    }
    
    public void GoToUnit()
    {
        Debug.Log("OK");
        GameObject.Find("MainCamera").GetComponent<MainCamera>().target = player.transform;
        GameObject.Find("MainCamera").GetComponent<MainCamera>().target = null;
    }

    private IEnumerator WaitOnCloseMenu()
    {
        yield return new WaitForSeconds(0.5f);
        
        gameUI.SetActive(true);
        gameMenuUI.SetActive(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    public void Restart()
    {
        SceneManager.LoadScene("Map1");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    } 


    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickAudio);
    }
}
