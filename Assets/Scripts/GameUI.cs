using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun.Demo.Cockpit;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI adText;
    public TextMeshProUGUI dfText;
    public TextMeshProUGUI levelText;
   
    public TextMeshProUGUI time;
    public static GameUI instance;
    public float TimeLeft;
    public float currentTime;
    public bool TimerOn;
    public bool oneSecond;
    public float timeASecond = 0;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
      
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
}
