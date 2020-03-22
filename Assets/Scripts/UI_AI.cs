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

    public void EndPlayerTurn()
    {
//        endTurnButton.SetActive(false);
        var states = GameObject.Find("GameManager").GetComponent<TurnBasedManager>();
        states.state = TurnBasedManager.State.EnemyMove;

        GameObject.Find("GameManager").GetComponent<TurnBasedManager>().UpdateTurn();

//        endTurnButton.SetActive(true);
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
    
}
