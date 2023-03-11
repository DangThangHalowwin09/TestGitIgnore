using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEditor;
using UnityEngine.XR;
using System;
using Photon.Realtime;
using DG.Tweening;
using System.Linq;
using Newtonsoft.Json.Linq;

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
    public GameObject FireBallLeft;
    public GameObject FireBallRight;
    public GameObject attackPointLeft;
    public GameObject attackPointRight;
    public int id;
    public int enemyID;
    private bool isMine;
    private float dist = 10000;
    private float distMin;
    public GameObject bomb;
    public PlayerController[] AllPlayers;
    private Sequence sequence;
    private int indexBomb = 0;
    PlayerController minDistancePlayer;
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

        if (targetPlayer != null)
        {
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

            if (dist < attackRange && Time.time - lastattackTime >= attackrate)
            {

                if (type == EnemyType.Death)
                {
                    Attack();
                }
                if (type == EnemyType.Knight && (targetPlayer.transform.position.y < gameObject.transform.position.y + 0.5f && targetPlayer.transform.position.y > gameObject.transform.position.y - 0.5f))
                {
                    StartCoroutine(CastFire());
                }
                else
                {
                    if (!targetPlayer.dead && type == EnemyType.Knight)
                    {
                        Walk();
                    }
                    else Stand();
                }

            }
            else if (dist >= attackRange && type != EnemyType.Boss)
            {
                if (!targetPlayer.dead)
                {
                    Walk();
                }
            }
            else
            {
                Stand();
            }
        }
            DetectPlayer();
            if (GameUI.instance.TimeLeft == 8 && !GameUI.instance.wasBossDie)
            {
                AudioManager.instance.PlaySFX(21);

            }
            if (GameUI.instance.TimeLeft == 0 && !GameUI.instance.wasBossDie)
            {
                photonView.RPC("DefeatInfo", RpcTarget.All);

            }
        
    }
    [PunRPC]
    void WinInfo()
    {
        GameUI.instance.Notif.SetActive(true);
        GameUI.instance.win.SetActive(true);
        GameUI.instance.defeat.SetActive(false);
        //GetRank();
    }
    [PunRPC]
    void DefeatInfo()
    {
        GameUI.instance.Notif.SetActive(true);
        GameUI.instance.win.SetActive(false);
        GameUI.instance.defeat.SetActive(true);
        //GetRank();
    }

    void GetRank()
    {
        for (int i = 0; i < GameManager.instance.players.Length; i++)
        {
            GameUI.instance.MoneyPlayer[i] = GameManager.instance.players[i].gold; 
        }
            for (int i = 0; i< GameManager.instance.players.Length; i++)
        {
            if(GameManager.instance.players.Length >= 1)
            {
                GameUI.instance.winnerInfo.SetActive(true);
                GameUI.instance.SecondRunnerInfo.SetActive(false);
                GameUI.instance.firstRunnerInfo.SetActive(false);
                GameUI.instance.win.SetActive(true);
                int max = GameUI.instance.MoneyPlayer.Max();
                GameUI.instance.bestScore.text = "" + max.ToString();
                int maxIndex = Array.IndexOf(GameUI.instance.MoneyPlayer, max);
                GameUI.instance.bestName.text = "" + GameManager.instance.players[maxIndex].name;

            }   
        }
    }
    void ReturnRoom()
    {
        
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
        AnimatorStateInfo animEnemy = anim.GetCurrentAnimatorStateInfo(0);
        if (animEnemy.IsName("Any State"))
        {
            //playerAnim.SetTrigger("Attack");
        }
        anim.SetTrigger("Attack");
        lastattackTime = Time.time;
        yield return new WaitForSeconds(0f);
       
    }
    void InstantiateFireBall()
    {
        
        if (faceRight)
        {
            GameObject bulletObj = Instantiate(FireBallRight, attackPointRight.transform.position, Quaternion.identity);
            FireBall bulletScript = bulletObj.GetComponent<FireBall>();
            bulletScript.Initialized(id, photonView.Owner);
        }
        else
        {
            GameObject bulletObj = Instantiate(FireBallLeft, attackPointLeft.transform.position, Quaternion.identity);
            FireBall bulletScript = bulletObj.GetComponent<FireBall>();
            bulletScript.Initialized(id, photonView.Owner);
        }
    }
    void SpawnBomb(int index)
    {
        indexBomb++;
        foreach (PlayerController player in GameManager.instance.players)
        {
            if (player != null)
            {
                GameObject bulletObj = Instantiate(bomb, transform.position, Quaternion.identity);
                bulletObj.transform.DOJump(player.transform.position, 5, 1, 1).SetEase(Ease.Linear).SetId(index);
                Bomb bulletScript = bulletObj.GetComponent<Bomb>();
                bulletScript.index = index;
                bulletScript.Initialized(id, photonView.Owner);
            }     
        }
    }
    void BombTrigger()
    {
        StartCoroutine(SpawnBombIE());
       
    }
    IEnumerator SpawnBombIE()
    {

        SpawnBomb(indexBomb++);
        yield return new WaitForSeconds(0f);
    }
    void initializeAttack(int attackID, bool isMine)
    {
        this.enemyID = attackID;
        this.isMine = isMine;
    }
    void Attack()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Any State"))
        {
             //anim.SetTrigger("Attack");
        }
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

            dist = 10000000;
            foreach (PlayerController player in GameManager.instance.players)
            {
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                
                    if (dist > distance)
                    {
                        dist = distance;
                        minDistancePlayer = player;
                    }
                }
            }
            
            if (minDistancePlayer == targetPlayer && !minDistancePlayer.dead)
            {
                if (dist > chaseRange)
                {
                    targetPlayer = null;
                    Stand();
                }
            
            }
            else if (dist <= chaseRange)
            {
            if (targetPlayer == null && !minDistancePlayer.dead)
            {
                targetPlayer = minDistancePlayer;
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

        if(type == EnemyType.Boss)
        {
            anim.SetTrigger("Attack");
        }
    }
    [PunRPC]
    public void EnemyHurt(int damageamount)
    {
        currentHP -= damageamount;
        healthBar.photonView.RPC("UpdateHealthBar", RpcTarget.All, currentHP, maxHP);
        if (currentHP <= 0)
        {
            DieByBomb();
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
            photonView.RPC("WinInfo", RpcTarget.All);

        }
        PhotonNetwork.Destroy(gameObject);
    }
    [PunRPC]
    void DieByBomb()
    {
        PhotonNetwork.Instantiate(death, transform.position, Quaternion.identity);
        System.Random rand = new System.Random();
        if (objectTospawnOnDeath != null)
            PhotonNetwork.Instantiate(objectTospawnOnDeath[(rand.Next(objectTospawnOnDeath.Length))], transform.position, Quaternion.identity);
        AudioManager.instance.PlaySFX(19);
        PhotonNetwork.Destroy(gameObject);
    }
}
