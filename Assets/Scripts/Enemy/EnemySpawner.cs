using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemySpawner : MonoBehaviourPun
{
    public string[] enemyprefabPath;
    public float maxEnemies;
    public float spawnRadius;
    public float spawnCheckTime;
    private float lastSpawnCheckTime;
    public List<GameObject> currentEnemies = new List<GameObject>();
    bool wasSpawnBoss = false;
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if(Time.time - lastSpawnCheckTime > spawnCheckTime)
        {
            lastSpawnCheckTime = Time.time;
            TrySpawn();
        }
    }
    void TrySpawn()
    {
        for(int x = 0; x < currentEnemies.Count; x++)
        {
            if (!currentEnemies[x])
            {
                currentEnemies.RemoveAt(x);
            }
        }
        if (currentEnemies.Count >= maxEnemies)
            return;
        Vector3 randomIncircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomIncircle2 = Random.insideUnitCircle * spawnRadius;
        maxEnemies += 1.2f * Mathf.FloorToInt(GameUI.instance.currentTime / 50); 

        GameObject enemy1 = PhotonNetwork.Instantiate(enemyprefabPath[0], transform.position + randomIncircle, Quaternion.identity);
        GameObject enemy2 = PhotonNetwork.Instantiate(enemyprefabPath[1], transform.position + randomIncircle2, Quaternion.identity);
        
        if(GameUI.instance.currentTime > 10 && !wasSpawnBoss)
        {
            GameObject enemy3 = PhotonNetwork.Instantiate(enemyprefabPath[2], new Vector3(0, 0, 0), Quaternion.identity);
            currentEnemies.Add(enemy3);
            wasSpawnBoss = true;
        }
        currentEnemies.Add(enemy1);
        currentEnemies.Add(enemy2);
    }
}
