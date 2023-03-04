using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MagicBall : MonoBehaviourPun
{
    public float speed;
    private int attackerId;
    private bool isMine;
    public Vector2 moveDir;
    private Rigidbody2D rb;
    public int damage;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("Attack"))
        {
            damage = PlayerPrefs.GetInt("Attack");
        }

        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(DestroyObject());
    }

    // Update is called once per frame
    void Update()
    {
            rb.velocity = moveDir * speed;
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy" && isMine)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, this.attackerId, damage);
            Destroy(gameObject);
            //PhotonNetwork.Destroy(gameObject);
        }
    }
    [PunRPC]
    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
        //PhotonNetwork.Destroy(gameObject);
    }
    public void Initialized(int attackID, bool isMine)
    {
        this.attackerId = attackID;
        this.isMine = isMine;
    }
}
