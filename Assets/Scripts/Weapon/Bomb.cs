using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float speed;
    private int attackerId;
    private bool isMine;
    private Rigidbody2D rb;
    public int damage;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 3);
        DoParticalBomb();
    }

    IEnumerator DoParticalBomb()
    {
        yield return new WaitForSeconds(3);
        PhotonNetwork.Instantiate("Explosion1", transform.position, Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && isMine)
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
