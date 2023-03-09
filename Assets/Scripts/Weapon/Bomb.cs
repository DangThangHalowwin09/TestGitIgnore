using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Realtime;

public class Bomb : MonoBehaviour
{
    public Player Owner { get; private set; }
    public float speed;
    private int attackerId;
    private bool isMine;
    private Rigidbody2D rb;
    public int damage;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(DoParticalBomb()); 
    }

    IEnumerator DoParticalBomb()
    {
        yield return new WaitForSeconds(1.1f);
        PhotonNetwork.Instantiate("Explosion1", transform.position, Quaternion.identity);
        DestroyObject();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && other is BoxCollider2D)
        {
            PhotonNetwork.Instantiate("Explosion1", transform.position, Quaternion.identity);
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            DestroyObject();
            DOTween.Kill(transform);
            player.photonView.RPC("Hurt", player.photonPlayer, damage);
        }
    }
    void DestroyObject()
    {
        AudioManager.instance.PlaySFX(22);
        Debug.Log("333");
        if(gameObject != null)
        Destroy(gameObject);
    }
    public void Initialized(int attackID, Player owner)
    {
        this.attackerId = attackID;
        this.Owner = owner;
    }
}
