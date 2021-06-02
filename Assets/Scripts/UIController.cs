using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class UIController : MonoBehaviour
{
    // SIngleton
    public static UIController instance;

    // 
    public TMP_Text overheatedMessage  = null;
    public Slider weaponTempSlider;
    public Slider currentHPSlider;
    public GameObject deathScreen;
    public TMP_Text deathScreenText;

    [Header("Kills & Deaths Scores")]
    public TMP_Text killsText;
    public TMP_Text deathsText;
    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;
    public TMP_Text timerText;

    public GameObject endScreen;
    public GameObject optionsScreen;
    

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        
    }

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();


        }

        if(optionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }

    }

    public void ShowHideOptions()
    {
        if(!optionsScreen.activeInHierarchy)
        {
            optionsScreen.SetActive(true);
        }
        else
        {
            optionsScreen.SetActive(false);

        }

    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();

    }

}
