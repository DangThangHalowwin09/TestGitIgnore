using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEditor;
using UnityEngine.XR;
using System;

public class Enemy : MonoBehaviourPun
{
    public enum EnemyType
    {
        Knight,
        Death,
        Boss
    }
    public EnemyType type;
    public string death = "Death";
    public string enemyName;
    public float moveSpeed;
    public int currentHP;
    public int maxHP;
    public float chaseRange;
    public float attackRange;
    private PlayerController targetPlayer;
    public float playerdetectRate;
    private float lastPlayerDetectTime;
    public string[] objectTospawnOnDeath;

    public int damage;
    public float attackrate;
    private float lastattackTime;
    public HeaderInformation healthBar;
    public SpriteRenderer sr;
    public Animator anim;
    public Rigidbody2D rb;
    public int xpToGive;
    public int curAttackerID;
    public string[] objectTospawn = {"Coin", "Coin", "Coin", "Diamond", "Diamond", "Diamond", "Potion" };
    
    
    private void Start()
    {
        healthBar.InitializedEnemy(enemyName, maxHP);
        if(type == EnemyType.Boss)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePosition;
        }
    }
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if(targetPlayer != null)
        {
            float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
            float face = targetPlayer.transform.position.x - transform.position.x;

            if(face > 0)
            {
                photonView.RPC("FlipRight", RpcTarget.All);
            }
            else
            {
                photonView.RPC("FlipLeft", RpcTarget.All);
            }

            if(dist < attackRange && Time.time - lastattackTime >= attackrate)
            {
                Attack();
            }
            else if(dist > attackRange && type != EnemyType.Boss){
                
                    Vector3 dir = targetPlayer.transform.position - transform.position;
                    rb.velocity = dir.normalized * moveSpeed;
                    anim.SetBool("Walk", true); 
                
            }
            else
            {
                rb.velocity = Vector2.zero;
                anim.SetBool("Walk", false);
            }
        }
        DetectPlayer();
        if(GameUI.instance.TimeLeft == 0 && !GameUI.instance.wasBossDie)
        {
            GameUI.instance.LossNotif.SetActive(true);
        }
    }
    [PunRPC]
    void FlipRight()
    {
        sr.flipX = false;
    }
    [PunRPC]
    void FlipLeft()
    {
        sr.flipX = true;
    }

    void Attack()
    {
        anim.SetTrigger("Attack");
        lastattackTime = Time.time;
        targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, damage);
    }

    void DetectPlayer()
    {
        if(Time.time - lastPlayerDetectTime > playerdetectRate)
        {
            lastPlayerDetectTime = Time.time;

            foreach(PlayerController player in GameManager.instance.players)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (player == targetPlayer)
                {
                    if(dist > chaseRange)
                    {
                        targetPlayer = null;
                        anim.SetBool("Walk", false);
                        rb.velocity = Vector2.zero;
                    }
                }else if(dist < chaseRange)
                {
                    if(targetPlayer == null)
                    {
                        targetPlayer = player;
                    }
                }
            }
        }
    }
    [PunRPC]
    public void TakeDamage(int attackerID, int damageamount)
    {
        currentHP -= damageamount;
        curAttackerID = attackerID;
        healthBar.photonView.RPC("UpdateHealthBar", RpcTarget.All, currentHP, maxHP);
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            photonView.RPC("FlashDamage", RpcTarget.All);
        }

    }
    [PunRPC]
    void FlashDamage()
    {
        StartCoroutine(DamageFlash());
        IEnumerator DamageFlash()
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }
    void Die()
    {
        PlayerController player = GameManager.instance.GetPlayer(curAttackerID);
        GameManager.instance.GetPlayer(curAttackerID).photonView.RPC("EarnExp", player.photonPlayer, xpToGive);
        
        PhotonNetwork.Instantiate(death, transform.position, Quaternion.identity);
        System.Random rand = new System.Random();
        if(objectTospawnOnDeath != null)
        PhotonNetwork.Instantiate(objectTospawnOnDeath[(rand.Next(objectTospawnOnDeath.Length))], transform.position, Quaternion.identity);
        AudioManager.instance.PlaySFX(19);
        Debug.Log(gameObject.name);
        if (gameObject.name == "Monster(Clone)")
        {
            
            GameUI.instance.wasBossDie = true;
            GameUI.instance.WinNotif.SetActive(true);
        }
        PhotonNetwork.Destroy(gameObject);
    }
}
