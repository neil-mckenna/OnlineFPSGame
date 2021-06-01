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

    }

    public static MatchManager instance;
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

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
        }
        
    }

    private void Update() 
    {
        
        
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
        object[] package = new object[allPlayers.Count];
        
        for(int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i] = piece;

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

        for(int i = 0; i < dataRecieved.Length; i++)
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
                index = i;
            }
        }

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

                break;

            }

        }

        
        
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
