using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;
using Photon.Realtime;

public enum PickupTypes
{
    Gold,
    Health,
    Food,
    Bomb, 
    AttackTonic,
    DefendTonic
}
public class Pickups : MonoBehaviourPun
{
    public PickupTypes type;
    public int value;
    bool magnetOn = false;
    float speed = 7f;
    PlayerController playerForCoin;
    private void Update()
    {
        if (magnetOn)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerForCoin.transform.position, speed*Time.deltaTime);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (collision.CompareTag("Player") && collision is CircleCollider2D && type != PickupTypes.Bomb)
        {
            magnetOn = true;
            playerForCoin = collision.gameObject.GetComponent<PlayerController>();
        }
        if(collision.CompareTag("Enemy") && type == PickupTypes.Bomb)
        {
            var enemy = collision.gameObject.GetComponent<Enemy>();
            
            if(enemy.type == Enemy.EnemyType.Boss)
            {
                PhotonNetwork.Instantiate("Explosion1", transform.position, Quaternion.identity);
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                AudioManager.instance.PlaySFX(22);
                enemy.photonView.RPC("DieByBomb", RpcTarget.MasterClient);
                if (gameObject != null)
                    gameObject.transform.DOScale(transform.localScale * 1.5f, 0.09f).SetEase(Ease.Linear);
                StartCoroutine(DoBoom());
            }  
        }
        if (collision.CompareTag("Player") && collision is BoxCollider2D)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (type == PickupTypes.Gold)
            {
                player.photonView.RPC("GetGold", player.photonPlayer, value);
                PhotonNetwork.Destroy(gameObject);
            }
                
            else if (type == PickupTypes.Health)
            {
                player.photonView.RPC("Heal", player.photonPlayer, value);
                PhotonNetwork.Destroy(gameObject);
            }
                
            else if (type == PickupTypes.Food)
            {
                player.photonView.RPC("Eat", player.photonPlayer, value);
                PhotonNetwork.Destroy(gameObject);
            }
            else if (type == PickupTypes.Bomb)
            {
                AudioManager.instance.PlaySFX(22);
                player.photonView.RPC("Hurt", player.photonPlayer, -value);
                StartCoroutine(DoBoom());
            }
            else if (type == PickupTypes.AttackTonic)
            {
                player.photonView.RPC("BuyAttack", player.photonPlayer, value);
                PhotonNetwork.Destroy(gameObject);
            }
            else if (type == PickupTypes.DefendTonic)
            {
                player.photonView.RPC("BuyDef", player.photonPlayer, value);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
    IEnumerator DoBoom()
    {
        yield return new WaitForSeconds(0.1f);
        PhotonNetwork.Instantiate("Explosion1", transform.position, Quaternion.identity);
        if(gameObject!=null)
        PhotonNetwork.Destroy(gameObject);
    }
}
