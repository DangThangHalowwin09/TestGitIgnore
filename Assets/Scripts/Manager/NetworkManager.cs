using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.UtilityScripts;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int maxPlayers;

    public static NetworkManager instance;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public void CreateRoom(string roomName)
    {
        //Create different rooms
        RoomOptions Roptions = new RoomOptions();
        // intial the number of players
        Roptions.MaxPlayers = (byte)maxPlayers;

        PhotonNetwork.CreateRoom(roomName, Roptions);
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
  
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Thang111");
        StartCoroutine(Leave());
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }
    IEnumerator Leave()
    {
        Debug.Log("222");
        while (PhotonNetwork.InRoom) yield return null;
        PhotonNetwork.LoadLevel("MenuGame");
        Destroy(gameObject);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("111");
    }
}
