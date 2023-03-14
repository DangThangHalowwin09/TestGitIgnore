using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun.Demo.Cockpit;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI adText;
    public TextMeshProUGUI dfText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI score;

    public TextMeshProUGUI bestName;
    public TextMeshProUGUI bestScore;
    public TextMeshProUGUI firstRunnerScore;
    public TextMeshProUGUI firstRunnerName;
    public TextMeshProUGUI secondRunnerScore;
    public TextMeshProUGUI secondRunnerName;
    
    public TextMeshProUGUI time;
    public TextMeshProUGUI countDown;
    public static GameUI instance;
    public int countDownToNewGame = 5;
    public int scoreCurrent = 0;
    public float TimeLeft;
    public float currentTime;
    public bool TimerOn;
    public bool oneSecond;
    public float timeASecond = 0;
    
    public GameObject Notif;
    public GameObject win;
    public GameObject defeat;
    public GameObject winnerInfo;
    public GameObject firstRunnerInfo;
    public GameObject secondRunnerInfo;
    public bool CheckOnce;
    
    //public Dictionary<PlayerController[], int[]> = new ToMoney<PlayerController[], int[]>
    const int MAX = 100; 
    public int[] MoneyPlayer = new int[MAX];
    public bool wasBossDie;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {

      if(TimeLeft == 0 && !CheckOnce)
        {
            PrepareStartGame();
            CheckOnce = true;
        }
    }

    void PrepareStartGame()
    {
        //countDownToNewGame = ;
    }
    public void UpdateTime(float Timeleft)
    {
        currentTime += 1;
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        time.text = minutes + ":" + seconds;
    }
  

    public void UpdateGoldText(int goldAmount)
    {
        goldText.text = "" + goldAmount;
    }
    public void UpdateHPText(int curHP, int maxHP)
    {
        hpText.text = "" + curHP + "/" + maxHP;
    }
    public void UpdateAdText(int adAmount)
    {
        adText.text = "" + adAmount;
    }
    public void UpdateDFText(int dfAmount)
    {
        dfText.text = "" + dfAmount;
    }
    public void UpdateLevelText(int curXp, int maxXp)
    {
        string s = (curXp * 100 / maxXp).ToString("F1");
        levelText.text = "" + s + "%";
    }
    public void UpdateScoreText(int amount)
    {
        scoreCurrent += amount;
        score.text = "" + scoreCurrent;
    }

    public void UpdateCountDownToNewGame(int newSecond)
    {
        countDown.text = "Start New Game After:" + newSecond + " Seconds";
    }
    public void PlayAgain()
    {
        StartCoroutine(LeftRoom());
    }
    IEnumerator LeftRoom()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom) yield return null;
    }
}
