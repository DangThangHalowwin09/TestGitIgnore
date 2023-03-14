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
using Photon.Pun.UtilityScripts;
using JetBrains.Annotations;
using UnityEngine.SocialPlatforms.Impl;


public class Enemy : MonoBehaviourPun
{
    public Player Owner { get; private set; }
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
    bool PlayOnce = false;
    bool ShowOnce = false;

    private float bestSco = -1000;
    private float secondSco = -1000;
    private float thirdSco = -1000;
    private int bestInd, secondInd, thirdIndex;
    private Player winner, firstRunner, secondRunner;
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
             
        if (GameUI.instance.TimeLeft == 8 && !GameUI.instance.wasBossDie && PlayOnce == false)
            {
                AudioManager.instance.PlaySFX(21);
                PlayOnce = true;
            }
        if (GameUI.instance.TimeLeft == 0 && !GameUI.instance.wasBossDie && ShowOnce == false)
            {
                photonView.RPC("DefeatInfo", RpcTarget.All);
            ShowOnce = true;
            }
    }
    [PunRPC]
    void WinInfo()
    {
        GameUI.instance.Notif.SetActive(true);
        GameUI.instance.win.SetActive(true);
        GameUI.instance.defeat.SetActive(false);
        GetRank2();
    }
    [PunRPC]
    void DefeatInfo()
    {
        GameUI.instance.Notif.SetActive(true);
        GameUI.instance.win.SetActive(false);
        GameUI.instance.defeat.SetActive(true);
        GetRank2();
    }
    void GetRank2()
    {
        
        foreach (Player p in PhotonNetwork.PlayerList)
        {     
            if (bestSco <= p.GetScore())
            {
                bestSco = p.GetScore();
                GameUI.instance.bestScore.text = "" + bestSco;
                GameUI.instance.bestName.text = "" + p.NickName;
                winner = p;
            }
            GameUI.instance.winnerInfo.SetActive(true);
        }
        
        if (GameManager.instance.players.Length >= 2)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (secondSco <= p.GetScore() && p != winner)
                {
                    secondSco = p.GetScore();
                    GameUI.instance.firstRunnerScore.text = "" + secondSco;
                    GameUI.instance.firstRunnerName.text = "" + p.NickName;
                    firstRunner = p;
                }
                GameUI.instance.firstRunnerInfo.SetActive(true);
            }
               
        }

        if (GameManager.instance.players.Length >= 3)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (thirdSco <= p.GetScore() && p != winner && p!= firstRunner)
                {
                    thirdSco = p.GetScore();
                    GameUI.instance.secondRunnerScore.text = "" + thirdSco;
                    GameUI.instance.secondRunnerName.text = "" + p.NickName;
                }
                GameUI.instance.secondRunnerInfo.SetActive(true);
            }
        }
        

    }
    void DestroyPlayer()
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

        if (minDistancePlayer != null && targetPlayer != null && minDistancePlayer == targetPlayer && !minDistancePlayer.dead)
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
        Owner = player.photonPlayer;
        GameManager.instance.GetPlayer(curAttackerID).photonView.RPC("EarnExp", player.photonPlayer, xpToGive);
        AddScore(Owner);
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

    void AddScore(Player p)
    {
        if (type == EnemyType.Death)
        {
           
            p.AddScore(20);
            p.GetScore();
            int score = p.GetScore();
            GameUI.instance.UpdateScoreText(20);
        }

        else if (type == EnemyType.Knight)
        {
            
            p.AddScore(50);
            GameUI.instance.UpdateScoreText(50);
        }

        else
        {
            p.AddScore(100);
            int score = p.GetScore();
            GameUI.instance.UpdateScoreText(100);
        }
    }
    public void Initialized( Player owner)
    {
        this.Owner = owner;
    }
    
}
