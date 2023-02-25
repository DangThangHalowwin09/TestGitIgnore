using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectShop : MonoBehaviour
{
    public bool DetectedShop;
    public GameObject ShopUI;
    public GameObject questIcon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (DetectedShop)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ShopUI.SetActive(true);
            }
        }
        else
        {
            ShopUI.SetActive(false);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "Shop")
        {
            DetectedShop = true;
            questIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "Shop")
        {
            DetectedShop = false;
            questIcon.SetActive(false);
        }
    }
}
