using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public string[] pickupPrefabPath;
    public float maxPickUps;
    public float spawnRadius;
    public float spawnCheckTime;
    private float lastSpawnCheckTime;
    public List<GameObject> currentPickups = new List<GameObject>();
    bool wasSpawnBoss = false;
    int numberPrefabPath;
    public GameObject pickupSpawner;
    private void Start()
    {
        numberPrefabPath = pickupPrefabPath.Count();
    }
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (Time.time - lastSpawnCheckTime > spawnCheckTime)
        {
            lastSpawnCheckTime = Time.time;
            TrySpawn();
        }
    }
    void TrySpawn()
    {
        for (int x = 0; x < currentPickups.Count; x++)
        {
            if (!currentPickups[x])
            {
                currentPickups.RemoveAt(x);
            }
        }
        if (currentPickups.Count >= maxPickUps)
            return;
        Vector3 randomIncircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomIncircle2 = Random.insideUnitCircle * spawnRadius;
        maxPickUps += 2 * Mathf.FloorToInt(GameUI.instance.currentTime / 30);
        GameObject Pickups1 = PhotonNetwork.Instantiate(pickupPrefabPath[Random.Range(0, numberPrefabPath)], transform.position + randomIncircle, Quaternion.identity);
        Pickups1.transform.parent = pickupSpawner.transform;
        GameObject Pickups2 = PhotonNetwork.Instantiate(pickupPrefabPath[Random.Range(0, numberPrefabPath)], transform.position + randomIncircle2, Quaternion.identity);
        Pickups2.transform.parent = pickupSpawner.transform;
        if (GameUI.instance.currentTime > 10 && !wasSpawnBoss)
        {
            Vector3 randomIncircle3 = Random.insideUnitCircle * spawnRadius;
            GameObject Pickups3 = PhotonNetwork.Instantiate(pickupPrefabPath[3], transform.position + randomIncircle3, Quaternion.identity);
            Pickups3.transform.parent = pickupSpawner.transform;
            currentPickups.Add(Pickups3);
            wasSpawnBoss = true;
        }
        currentPickups.Add(Pickups1);
        currentPickups.Add(Pickups2);
    }
}
