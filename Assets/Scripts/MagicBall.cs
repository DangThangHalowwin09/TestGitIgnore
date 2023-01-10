using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MagicBall : MonoBehaviourPun
{
    public float speed;
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
        Invoke("DestroyObject", 1);
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = moveDir * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Enemy")
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
            photonView.RPC("DestroyObject", RpcTarget.MasterClient);
        }
    }


    [PunRPC]
    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
