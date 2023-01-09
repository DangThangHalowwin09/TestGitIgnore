using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI adText;
    public TextMeshProUGUI dfText;

    public static GameUI instance;

    private void Awake()
    {
        instance = this;
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
}
