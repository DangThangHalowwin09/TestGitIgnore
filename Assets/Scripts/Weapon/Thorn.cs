using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thorn : MonoBehaviour
{
    public Player Owner { get; private set; }
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
        if (other.tag == "Enemy")
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, this.attackerId, damage);
            Destroy(gameObject);
        }
    }
    [PunRPC]
    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
    public void Initialized(int attackID, Player owner)
    {
        this.attackerId = attackID;
        this.Owner = owner;
    }
}
