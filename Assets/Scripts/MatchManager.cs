using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat,
        NextMatch,
        TimerSync,

    }

    public enum GameState
    {
        Waiting,
        Playing,
        Ending,


    }

    public static MatchManager instance;
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

    private List<LeaderboardPlayer> leaderboardPlayers = new List<LeaderboardPlayer>();

    [Header("Match Parameters")]
    public int killsToWin = 5;
    public Transform mapCamPoint;
    public GameState state = GameState.Waiting;
    public float waitAfterEnding = 10f;
    public bool perpetual = false;
    public float matchLength = 180f;
    private float currentMatchTime;
    private float sendTimer;

    private void Awake() 
    {
        instance  = this;
        
    }

    private void Start() 
    {
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);

            state = GameState.Playing;

            SetupTimer();

            if(!PhotonNetwork.IsMasterClient)
            {
                UIController.instance.timerText.gameObject.SetActive(false);

            }
        }
        
    }

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Tab) && state != GameState.Ending)
        {
            if(UIController.instance.leaderboard.activeInHierarchy)
            {
                UIController.instance.leaderboard.SetActive(false);
            }
            else
            {
                ShowLeaderboard();

            }
        }

        if(PhotonNetwork.IsMasterClient)
        {
            if(currentMatchTime > 0 && state == MatchManager.GameState.Playing)
            {
                currentMatchTime -= Time.deltaTime;
                if(currentMatchTime <= 0f)
                {
                    currentMatchTime = 0f;
                    state = GameState.Ending;


                    ListPlayersSend();
                    StateCheck();
                }

                UpdateTimerDisplay();

                sendTimer -= Time.deltaTime;
                if(sendTimer <= 0)
                {
                    sendTimer += 1f;

                    TimerSend();
                }
            }

        }

        
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            //Debug.LogWarning("Recevied event " + theEvent);

            switch(theEvent)
            {
                case EventCodes.NewPlayer:
                    
                    NewPlayerRecieve(data);

                break;

                case EventCodes.ListPlayers:
                    
                    ListPlayersRecieve(data);
                
                break;

                case EventCodes.UpdateStat:

                    UpdateStatsRecieve(data);

                break;

                case EventCodes.NextMatch:

                    NextMatchRecieve();

                break;
                case EventCodes.TimerSync:

                    TimerReceive(data);

                break; 

                default:

                break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);

    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);

    }

    public void NewPlayerSend(string userName)
    {
        object[] package = new object[4];
        package[0] = userName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );

    }

    public void NewPlayerRecieve(object[] dataRecieved)
    {
        PlayerInfo player = new PlayerInfo((string)dataRecieved[0], (int)dataRecieved[1], (int)dataRecieved[2], (int)dataRecieved[3]);

        allPlayers.Add(player);

        ListPlayersSend();

    }

    public void ListPlayersSend()
    {
        object[] package = new object[allPlayers.Count + 1];

        package[0] = state;
        
        for(int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i + 1] = piece;

        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void ListPlayersRecieve(object[] dataRecieved)
    {
        allPlayers.Clear();

        state = (GameState)dataRecieved[0];


        for(int i = 1; i < dataRecieved.Length; i++)
        {
            object[] piece = (object[])dataRecieved[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]

            );

            allPlayers.Add(player);

            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i - 1;
            }
        }
        StateCheck();

    }

    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );


    }
    public void UpdateStatsRecieve(object[] dataRecieved)
    {
        int actor = (int)dataRecieved[0];
        int statType = (int)dataRecieved[1];
        int amount = (int)dataRecieved[2];

        for(int i = 0; i < allPlayers.Count; i++)
        {
            if(allPlayers[i].actor == actor)
            {
                switch(statType)
                {
                    // kills
                    case 0:
                        allPlayers[i].kills += amount;
                        Debug.LogWarning("Player" + allPlayers[i].name + " : kills " + allPlayers[i].kills);
                    break;

                    // deaths
                    case 1:
                        allPlayers[i].deaths += amount;
                        Debug.LogWarning("Player" + allPlayers[i].name + " : deaths " + allPlayers[i].deaths);
                    break;

                    default:
                    break;
                }

                if(i == index)
                {
                    UpdateStatDisplay();
                }

                if(UIController.instance.leaderboard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }

                break;
            }
        }  

        ScoreCheck();
    }

    public void UpdateStatDisplay()
    {
        if(allPlayers.Count > index)
        {
            UIController.instance.killsText.text = "Kills: " + allPlayers[index].kills;
            UIController.instance.deathsText.text = "Deaths: " + allPlayers[index].deaths;

        }
        else
        {
            UIController.instance.killsText.text = "Kills: 0";
            UIController.instance.deathsText.text = "Deaths: 0";

        }
    }

    private void ShowLeaderboard()
    {
        UIController.instance.leaderboard.SetActive(true);

        foreach(LeaderboardPlayer lp in leaderboardPlayers)
        {
            Destroy(lp.gameObject);
        }

        leaderboardPlayers.Clear();

        UIController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        foreach(PlayerInfo player in sorted)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderboardPlayerDisplay, UIController.instance.leaderboardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);

            newPlayerDisplay.gameObject.SetActive(true);

            leaderboardPlayers.Add(newPlayerDisplay);

        }

    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while(sorted.Count < players.Count)
        {
            int highest = -1;

            PlayerInfo selectPlayer = players[0];
            
            foreach(PlayerInfo player in players)
            {
                if(!sorted.Contains(player))
                {
                    if(player.kills > highest)
                    {   
                        selectPlayer = player;
                        highest = player.kills;
                    }
                }  
            }

            sorted.Add(selectPlayer);

        }
        return sorted;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        SceneManager.LoadScene(0);
    }

    public void ScoreCheck()
    {
        bool winnerFound = false;

        foreach(PlayerInfo player in allPlayers)
        {
            if(player.kills >= killsToWin && killsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if(winnerFound)
        {
            if(PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayersSend();
            }
        }
    }

    void StateCheck()
    {
        if(state == GameState.Ending)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.LogWarning("Game Ending");
        state = GameState.Ending;
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();

            UIController.instance.endScreen.SetActive(true);

            ShowLeaderboard();
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Camera.main.transform.position = mapCamPoint.position;
            Camera.main.transform.rotation = mapCamPoint.rotation;

            StartCoroutine(EndCo());

        }
    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);

        if(!perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if(PhotonNetwork.IsMasterClient)
            {
                if(!Launcher.instance.changeMapBetweenRounds)
                {
                    NextMatchSend();
                }
                else
                {
                    int newLevel = Random.Range(0, Launcher.instance.allMaps.Length);

                    if(Launcher.instance.allMaps[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(Launcher.instance.allMaps[newLevel]);
                    }

                }
                
            }
        } 
    }

    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextMatch,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );

    }

    public void NextMatchRecieve()
    {
        state = GameState.Playing;

        UIController.instance.endScreen.SetActive(false);
        UIController.instance.leaderboard.SetActive(false);

        foreach(PlayerInfo player in allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
        }

        UpdateStatDisplay();

        PlayerSpawner.instance.SpawnPlayer();

        SetupTimer();

    }

    public void SetupTimer()
    {
        if(matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();

        }
    }

    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);

        UIController.instance.timerText.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");

    }

    public void TimerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, state };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.TimerSync,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void TimerReceive(object[] dataReceived)
    {
        currentMatchTime = (int)dataReceived[0];
        state = (GameState)dataReceived[1];

        UpdateTimerDisplay();

        UIController.instance.timerText.gameObject.SetActive(true);

    }



}




[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        this.name = _name;
        this.actor = _actor;
        this.kills = _kills;
        this.deaths = _deaths;
    }
}


