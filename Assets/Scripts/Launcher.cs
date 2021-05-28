using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Singleton
    public static Launcher instance;
    [Header("Starting Menu")]
    public GameObject menuButtons;
    public GameObject loadingScreen;
    public TMP_Text loadingText;
    [Header("Create Room")]
    public GameObject createRoomScreen;
    public TMP_InputField roomNameInput;


    private void Awake() 
    {
        instance = this;    
    
    }

    private void Start() 
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Network...";

        PhotonNetwork.ConnectUsingSettings();
        
    }

    public void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {

        PhotonNetwork.JoinLobby();

        loadingText.text = "Joining Lobby...";

    } 

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void OpenRoomCreate()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);


    }





}
