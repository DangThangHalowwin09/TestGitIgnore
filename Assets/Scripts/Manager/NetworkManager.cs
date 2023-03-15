using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.UtilityScripts;
using Photon.Voice.Unity;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int maxPlayers;

    public static NetworkManager instance;
    public GameObject VoiceManager;
    public GameObject VoiceLogger;
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
        VoiceManager = GameObject.FindGameObjectWithTag("VoiceManager");
        VoiceLogger = GameObject.FindGameObjectWithTag("VoiceLogger");
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
        StartCoroutine(Leave());
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }
    IEnumerator Leave()
    {
        while (PhotonNetwork.InRoom) yield return null;
        Destroy(VoiceManager);
        Destroy(VoiceLogger);
        PhotonNetwork.LoadLevel("MenuGame");
        Destroy(gameObject);
    }
    
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
    }
}
