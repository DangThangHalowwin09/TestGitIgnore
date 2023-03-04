using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bomb : MonoBehaviour
{
    public float speed;
    private int attackerId;
    private bool isMine;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRender;
    public int damage;
    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer spriteRender = gameObject.GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        DoParticalBomb();
        Destroy(gameObject, 1f);
        
    }

    IEnumerator DoParticalBomb()
    {
        yield return new WaitForSeconds(1f);
        spriteRender.color = Color.Lerp(Color.white, Color.red, 1f);
        PhotonNetwork.Instantiate("Explosion1", transform.position, Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && isMine && other is BoxCollider2D)
        {
            PhotonNetwork.Instantiate("Explosion1", transform.position, Quaternion.identity);
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if (isMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }

            player.photonView.RPC("Hurt", player.photonPlayer, damage);
        }
    }
    [PunRPC]
    void DestroyObject()
    {
        Destroy(gameObject);
    }
    public void Initialized(int attackID, bool isMine)
    {
        this.attackerId = attackID;
        this.isMine = isMine;
    }
}
