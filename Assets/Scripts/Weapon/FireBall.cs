using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
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
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 2);
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = moveDir * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && other is BoxCollider2D)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            Destroy(gameObject);
            player.photonView.RPC("Hurt", player.photonPlayer, damage);
        }
    }
    [PunRPC]
    void DestroyObject()
    {
        Destroy(gameObject);
    }
    public void Initialized(int attackID, Player owner)
    {
        this.attackerId = attackID;
        this.Owner = owner;
    }
}
