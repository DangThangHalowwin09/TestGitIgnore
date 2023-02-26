using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEditor;
using UnityEngine.XR;
using System;
using Photon.Realtime;

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
    public bool faceRight;
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
    public GameObject FireBallLeft;
    public GameObject FireBallRight;
    public GameObject attackPointLeft;
    public GameObject attackPointRight;
    public int id;
    public int enemyID;
    private bool isMine;
    private float dist = 10000;
    private float distMin;
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
        /*if (!photonView.IsMine)
        {
            return;
        }*/
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        if (targetPlayer != null)
        {
            //float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
            float face = targetPlayer.transform.position.x - transform.position.x;
            initializeAttack(id, photonView.IsMine);
            if (face > 0)
            {
                photonView.RPC("FlipRight", RpcTarget.All);
            }
            else
            {
                photonView.RPC("FlipLeft", RpcTarget.All);
            }

            if(dist < attackRange && Time.time - lastattackTime >= attackrate)
            {
                
                if(type == EnemyType.Death)
                {
                    Attack();
                }
                if(type == EnemyType.Knight && (targetPlayer.transform.position.y < gameObject.transform.position.y + 0.5f && targetPlayer.transform.position.y > gameObject.transform.position.y - 0.5f))
                {
                    StartCoroutine(CastFire());
                }
                else {
                    if (!targetPlayer.dead)
                    {
                        Walk();
                    }
                    else Stand();
                }

            }
            else if(dist > attackRange && type != EnemyType.Boss){
                if (!targetPlayer.dead)
                {
                    Debug.Log("111");
                    Walk();
                }      
            }
            else
            {
                Stand();
            }
        }
        DetectPlayer();
        if (GameUI.instance.TimeLeft == 0 && !GameUI.instance.wasBossDie)
        {
            GameUI.instance.LossNotif.SetActive(true);
        }
    }
    [PunRPC]
    void FlipRight()
    {
        sr.flipX = false;
        faceRight = true;
    }
    [PunRPC]
    void FlipLeft()
    {
        sr.flipX = true;
        faceRight = false;
    }
    void Stand()
    {
        rb.velocity = Vector2.zero;
        anim.SetBool("Walk", false);
    }
    void Walk()
    {
        Vector3 dir = targetPlayer.transform.position - transform.position;
        rb.velocity = dir.normalized * moveSpeed;
        anim.SetBool("Walk", true);
    }
    IEnumerator CastFire()
    {
        anim.SetTrigger("Attack");
        lastattackTime = Time.time;
        yield return new WaitForSeconds(0.5f);
        if (faceRight)
        {
            GameObject bulletObj = PhotonNetwork.Instantiate("FireBallRight", attackPointRight.transform.position, Quaternion.identity);
            FireBall bulletScript = bulletObj.GetComponent<FireBall>();
            bulletScript.Initialized(id, photonView.IsMine);
        }
        else
        {
            GameObject bulletObj = PhotonNetwork.Instantiate("FireBallLeft", attackPointLeft.transform.position, Quaternion.identity);
            FireBall bulletScript = bulletObj.GetComponent<FireBall>();
            bulletScript.Initialized(id, photonView.IsMine);
        }
    }
    void initializeAttack(int attackID, bool isMine)
    {
        this.enemyID = attackID;
        this.isMine = isMine;
    }
    void Attack()
    {
        anim.SetTrigger("Attack");
        lastattackTime = Time.time;
        targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, damage);
    }

    void DetectPlayer()
    {
        
        if (Time.time - lastPlayerDetectTime > playerdetectRate)
        {
            lastPlayerDetectTime = Time.time;
            determineTargetPlayer();
            }
    }

    void determineTargetPlayer()
    {
        foreach (PlayerController player in GameManager.instance.players)
        {
            dist = Vector2.Distance(transform.position, player.transform.position);

            if (player == targetPlayer && !player.dead)
            {
                if (dist > chaseRange)
                {
                    targetPlayer = null;
                    Stand();
                    Debug.Log("222" + player.name);
                }
            }
            else if (dist < chaseRange)
            {
                if (targetPlayer == null && !targetPlayer)
                {
                    targetPlayer = player;
                    Debug.Log("333" + player.name);
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
        if (gameObject.name == "Monster(Clone)")
        {
            GameUI.instance.wasBossDie = true;
            GameUI.instance.WinNotif.SetActive(true);
        }
        PhotonNetwork.Destroy(gameObject);
    }
}
