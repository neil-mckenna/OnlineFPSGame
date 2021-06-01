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

    public void Die()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);

        SpawnPlayer();



    }

    

}
