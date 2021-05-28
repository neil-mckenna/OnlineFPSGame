using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
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
    [Header("Room Settings")]
    public GameObject roomScreen;
    public TMP_Text roomNameText;


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
        roomScreen.SetActive(false);
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

    public void CreateRoom()
    {
        if(!string.IsNullOrEmpty(roomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;


            PhotonNetwork.CreateRoom(roomNameInput.text, options);

            CloseMenus();
            loadingText.text = "Creating Room...";
            loadingScreen.SetActive(true);

        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;



    }







}
