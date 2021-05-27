using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    public Transform[] spawnPoints;

    private void Awake() 
    {
        instance  = this;
        
    }

    private void Start() 
    {

        foreach(Transform spawn in spawnPoints)
        {
            spawn.gameObject.SetActive(false);
        }
        
    }

    public Transform GetSpawnPoint()
    {
        int rand = Random.Range(0, spawnPoints.Length - 1); 

        for(int i = 0; i < spawnPoints.Length; i++)
        {
            if(i == rand)
            {
                return spawnPoints[i].transform;
            }
            
        }

        return spawnPoints[0].transform;

        //
        //return spawnPoints[Random.Range(0, (int)spawnPoints.Length)];

    }

    




}
