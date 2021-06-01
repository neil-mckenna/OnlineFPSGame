using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;

    public GameObject playerPrefab;
    private GameObject player;

    public GameObject deathEffect;
    [Header("Respawn Times")]
    [Range(1, 10)] public float respawnTime = 5f;
 
    private void Awake() 
    {
        instance = this;
    }

    private void Start() 
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();

        }

 


        
    }



    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

    }

    public void Die(string damager)
    {
        
        UIController.instance.deathScreenText.text = "You were killed by " + damager;

        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if(player != null)
        {
            StartCoroutine(DieCo());
        }

    }

    public IEnumerator DieCo()
    {

        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        player = null;
        UIController.instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTime); 

        UIController.instance.deathScreen.SetActive(false);

        if(MatchManager.instance.state == MatchManager.GameState.Playing && player == null)
        {
            SpawnPlayer();
        }
        else
        {
            

        }
        


    }

    

}
