using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardPlayer : MonoBehaviour
{
    public TMP_Text playerNameText, playerKillsText, playerDeathText;

    public void SetDetails(string name, int kills, int deaths)
    {
        playerNameText.text = name;
        playerKillsText.text = kills.ToString();
        playerDeathText.text = deaths.ToString();

    }


}
