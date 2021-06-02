using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

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
    public TMP_Text roomNameText, playerNameLabel;
    public GameObject playerNameContentParent;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();
    
    [Header("Error Settings")]
    public GameObject errorScreen;
    public TMP_Text errorText;

    [Header("Browser Settings")]
    public GameObject roomBrowserScreen;
    public RoomButton theRoomButton;
    public GameObject buttonContentParent;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();

    [Header("Player Name Settings")]
    public GameObject nameInputScreen;
    public TMP_InputField nameInput;
    public static bool hasSetNickName;

    [Header("Level Settings")]
    public string levelToPlay;
    public GameObject startButton;
    public string[] allMaps;
    public bool changeMapBetweenRounds = true;

    [Header("Test Settings")]
    public GameObject roomTestButton;

    


    private void Awake() 
    {
        instance = this;     
    }

    private void Start() 
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Network...";

        if(!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        

#if UNITY_EDITOR
    roomTestButton.SetActive(true);
#endif

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true; 
        
    }

    public void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {

        PhotonNetwork.JoinLobby();

        PhotonNetwork.AutomaticallySyncScene = true;

        loadingText.text = "Joining Lobby...";

    } 

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if(!hasSetNickName)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            if(PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }

        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
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
            // options.IsVisible = true;
            // options.IsOpen = true;

            PhotonNetwork.CreateRoom(roomNameInput.text, options);

            CloseMenus();
            loadingText.text = "Creating Room...";
            loadingScreen.SetActive(true);

        }
        else
        {
            Debug.LogWarning("room name details " + roomNameInput.text);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayers();

        if(PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        } 
    }

    private void ListAllPlayers()
    {
        foreach(TMP_Text player in allPlayerNames)
        {
            if(player == null){ continue; }
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;

        for(int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameContentParent.transform);

            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayerNames.Add(newPlayerLabel); 
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameContentParent.transform);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        allPlayerNames.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        ListAllPlayers();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed to Create Room: " + message;
        CloseMenus();

        errorScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);

    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        
        CloseMenus();
        loadingText.text = "Joining Room " + inputInfo.Name;
        loadingScreen.SetActive(true);

        PhotonNetwork.JoinRoom(inputInfo.Name);

    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();

        loadingText.text = "Leaving Room";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);

    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
       
        
        // destroy all previous buttons
        foreach (RoomButton rb in allRoomButtons)
        {
            if(rb == null) { continue; }
            Destroy(rb.gameObject);
            
        }
        // clear the list
        allRoomButtons.Clear();
        
        // hide the button prefab 
        theRoomButton.gameObject.SetActive(false);

        for(int i = 0; i < roomList.Count; i++)
        {

            Debug.Log(roomList.Count + " Rooms");

            if(roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(theRoomButton, buttonContentParent.transform);
            
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);

            }
            else
            {
                Debug.LogWarning(allRoomButtons[i].name);
            }
        }
    }

    public void SetNickName()
    {
        if(!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;

            PlayerPrefs.SetString("playerName", nameInput.text);

            CloseMenus();
            menuButtons.SetActive(true);

            hasSetNickName = true;

        }
    }

    public void StartGame()
    {
        //PhotonNetwork.LoadLevel(levelToPlay);
        PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);
    }

    public override void OnMasterClientSwitched(Player newMasteClient)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        } 

    }

    public void QuickJoin()
    {

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;

        PhotonNetwork.CreateRoom("Test", options);
        CloseMenus();
        loadingText.text = "Creating Room";
        loadingScreen.SetActive(true);

    }

    public void QuitGame()
    {
        Application.Quit();
    }







}
