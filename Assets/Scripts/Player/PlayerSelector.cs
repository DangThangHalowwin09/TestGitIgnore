using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    private int gold;
    public GameObject nextButton;
    public GameObject backButton;
    public GameObject selectButton;
    public GameObject mainScreen;
    public GameObject changeButton;
    public static PlayerSelector instance;
    public string playerPrefabName;
    public GameObject[] playerModel;
    public int selectedCharacter = 0;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        if (PlayerPrefs.HasKey("Gold"))
        {
            gold = PlayerPrefs.GetInt("Gold");
        }
        else
        {
            gold = 10000;
            PlayerPrefs.SetInt("Gold", 10000);
            PlayerPrefs.SetInt("Attack", 50);
            PlayerPrefs.SetInt("Def", 50);
            PlayerPrefs.SetInt("MaxHP", 200);
        }
        if (PlayerPrefs.HasKey("SelectedCharacter")){
            selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter");
            playerPrefabName = playerModel[selectedCharacter].GetComponent<PlayerName>().playerName;
            nextButton.SetActive(false);
            backButton.SetActive(false);
            selectButton.SetActive(false);
            mainScreen.SetActive(true);
            changeButton.SetActive(true);
        }
        else
        {
            selectedCharacter = 0;
            playerPrefabName = playerModel[selectedCharacter].GetComponent<PlayerName>().playerName;
            nextButton.SetActive(true);
            backButton.SetActive(true);
            selectButton.SetActive(true);
            mainScreen.SetActive(false);
            changeButton.SetActive(false);
        }


        foreach(GameObject player in playerModel)
        {
            player.SetActive(false);
        }
        playerModel[selectedCharacter].SetActive(true);
    }
    public void ChangeNext()
    {
        AudioManager.instance.PlaySFX(0);
        playerModel[selectedCharacter].SetActive(false);
        selectedCharacter++;
        if (selectedCharacter == playerModel.Length)
            selectedCharacter = 0;
        playerModel[selectedCharacter].SetActive(true);
        
        playerPrefabName = playerModel[selectedCharacter].GetComponent<PlayerName>().playerName;
    }
    public void ChangeBack()
    {
        AudioManager.instance.PlaySFX(0);
        playerModel[selectedCharacter].SetActive(false);
        selectedCharacter--;
        if (selectedCharacter == - 1)
            selectedCharacter = playerModel.Length -1;
        playerModel[selectedCharacter].SetActive(true);
        playerPrefabName = playerModel[selectedCharacter].GetComponent<PlayerName>().playerName;
    }
    public void SelectHero()
    {
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacter);
        nextButton.SetActive(false);
        backButton.SetActive(false);
        selectButton.SetActive(false);
        mainScreen.SetActive(true);
        changeButton.SetActive(true);
        AudioManager.instance.PlaySFX(3);
    }
    public void ChangeHero(int amount)
    {
        if(gold >= amount)
        {
            gold -= amount;
            PlayerPrefs.SetInt("Hold", gold);
            PlayerPrefs.DeleteKey("SelectedCharacter");
            nextButton.SetActive(true);
            backButton.SetActive(true);
            selectButton.SetActive(true);
            mainScreen.SetActive(false);
            changeButton.SetActive(false);
            AudioManager.instance.PlaySFX(0);
        }
    }
}
