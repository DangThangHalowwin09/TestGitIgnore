using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Runtime.CompilerServices;

public enum AttackType
{
    Warrior,
    Magicer
}
public class PlayerController : MonoBehaviourPun
{
    public AttackType type;
    public GameObject magicRight;
    public GameObject magicLeft;
    public int warriorID;
    private bool isMine;
    public bool faceRight;
    public Transform attackPointRight;
    public Transform attackPointLeft;
    public int damage;
    public int def;
    public float attackRange;
    public float attackDelay;
    public float lastAttackTime;
    [HideInInspector]
    public int id;
    public Animator playerAnim;
    public Rigidbody2D rig;
    public Player photonPlayer;
    public SpriteRenderer sr;
    //public HeaderInfo headerInfo
    public float moveSpeed;
    public int gold;
    public int currentHP;
    public int maxHP;
    public bool dead;
    public static PlayerController me;
    public HeaderInformation headerInfo;

    public int playerLevel = 1;
    public int currentExp;
    public int maxExp = 500;
    public string levelUpEffect = "levelEffect";
    [PunRPC]
    public void Initialized(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;
        headerInfo.InitializedPlayer(playerLevel, player.NickName, maxHP);

        if (PlayerPrefs.HasKey("PlayerLevel"))
        {
            playerLevel = PlayerPrefs.GetInt("PlayerLevel");
        }
        if (PlayerPrefs.HasKey("CurrentEXP"))
        {
            currentExp = PlayerPrefs.GetInt("currentEXP");
        }
        if (PlayerPrefs.HasKey("MaxEXP"))
        {
            maxExp = PlayerPrefs.GetInt("MaxEXP");
        }
        
        headerInfo.InitializedPlayer(playerLevel, player.NickName, maxHP);

        if (PlayerPrefs.HasKey("Attack"))
        {
            damage = PlayerPrefs.GetInt("Attack");
        }
        GameUI.instance.UpdateAdText(damage);

        if (PlayerPrefs.HasKey("Def"))
        {
            def = PlayerPrefs.GetInt("Def");
        }
        GameUI.instance.UpdateDFText(def);

        if (PlayerPrefs.HasKey("Gold"))
        {
            gold = PlayerPrefs.GetInt("Gold");
        }
        GameUI.instance.UpdateGoldText(gold);

        if (PlayerPrefs.HasKey("MaxHP"))
        {
            maxHP = PlayerPrefs.GetInt("MaxHP");
        }
        currentHP = maxHP;
        GameUI.instance.UpdateHPText(currentHP, maxHP);
        GameUI.instance.UpdateLevelText(currentExp, maxExp);

        if (player.IsLocal)
            me = this;
        else
            rig.isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        Move();
        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackDelay) 
        {
            if (type == AttackType.Warrior)
                Attack();
            else if (type == AttackType.Magicer)
                CastSpell();
        }

        
    }
    private void FixedUpdate()
    {
        if (GameUI.instance.timeASecond >= 1 && !GameUI.instance.oneSecond)
        {
            GameUI.instance.timeASecond = 0;
            GameUI.instance.oneSecond = true;
        }
        else
        {
            GameUI.instance.timeASecond += Time.deltaTime;
            GameUI.instance.oneSecond = false;
        }

        if (GameUI.instance.TimerOn)
        {
            if (GameUI.instance.TimeLeft > 0 && GameUI.instance.oneSecond)
            {
                GameUI.instance.TimeLeft -= 1;
                GameUI.instance.UpdateTime(GameUI.instance.TimeLeft);
                
            }
            else if(GameUI.instance.oneSecond && GameUI.instance.TimeLeft <= 0)
            {
                Debug.Log("Time is up");
                GameUI.instance.TimeLeft = 0;
                GameUI.instance.TimerOn = false;
            }
        }
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        rig.velocity = new Vector2(x, y) * moveSpeed;
        if (x != 0 || y != 0)
        {
            playerAnim.SetBool("Move", true);
            
            if (x > 0)
            {
                photonView.RPC("FlipRight", RpcTarget.All);
            }
            else
            {
                photonView.RPC("FlipLeft", RpcTarget.All);
            }
        }
        else
        {
            playerAnim.SetBool("Move", false);
        }
    }
    void CastSpell()
    {
        lastAttackTime = Time.time;
        
        playerAnim.SetTrigger("Attack");
    }
    public void CastBall()
    {
        if (faceRight)
        {
            GameObject bulletObj = Instantiate(magicRight, attackPointRight.transform.position, Quaternion.identity);
            MagicBall bulletScript = bulletObj.GetComponent<MagicBall>();
            bulletScript.Initialized(id, photonView.IsMine);
        }
        else
        {
            GameObject bulletObj = Instantiate(magicLeft, attackPointLeft.transform.position, Quaternion.identity);
            MagicBall bulletScript = bulletObj.GetComponent<MagicBall>();
            bulletScript.Initialized(id, photonView.IsMine);
        }
    }
    void initializeAttack(int attackID, bool isMine)
    {
        this.warriorID = attackID;
        this.isMine = isMine;
    }
    void Attack()
    {
        lastAttackTime = Time.time;
        if (faceRight)
        {
            RaycastHit2D hit = Physics2D.Raycast(attackPointRight.position, transform.forward, attackRange);
            initializeAttack(id, photonView.IsMine);
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy") && isMine)
            {
                Debug.Log("1");
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, this.warriorID, damage);
            }   
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(attackPointLeft.position, transform.forward, attackRange);
            initializeAttack(id, photonView.IsMine);
            if (hit.collider != null  && hit.collider.gameObject.CompareTag("Enemy") && isMine)
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, this.warriorID, damage);
            }
        }

        playerAnim.SetTrigger("Attack");
        AudioManager.instance.PlaySFX(10);
                
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

    [PunRPC]
    public void TakeDamage(int damageAmount)
    {
        int damageValue = damageAmount - def;
        if (damageValue < 1) damageValue = 1;
        currentHP = currentHP - damageValue;
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, currentHP, maxHP);
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            photonView.RPC("FlashDamage", RpcTarget.All);
        }
        GameUI.instance.UpdateHPText(currentHP, maxHP);
    }
    [PunRPC]
    void FlashDamage()
    {
        StartCoroutine(DamageFlash());
        IEnumerator DamageFlash()
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            sr.color = Color.white;
        }
    }

    void Die()
    {
        AudioManager.instance.PlaySFX(6);
        dead = true;
        rig.isKinematic = true;
        transform.position = new Vector3(0, 90, 0);
        
        Vector3 spawnPos = GameManager.instance.spawnPoint[Random.Range(0, GameManager.instance.spawnPoint.Length)].position;
        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }

    IEnumerator Spawn(Vector3 SpawnPos, float timeToSpawn)
    {
        yield return new WaitForSeconds(timeToSpawn);
        dead = false;
        transform.position = SpawnPos;
        currentHP = maxHP;
        rig.isKinematic = false;
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, currentHP, maxHP);
        GameUI.instance.UpdateHPText(currentHP, maxHP);
    }
    [PunRPC]
    void Heal(int amountToHeal)
    {
        AudioManager.instance.PlaySFX(3);
        currentHP = Mathf.Clamp(currentHP + amountToHeal, 0, maxHP);
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, currentHP, maxHP);
        GameUI.instance.UpdateHPText(currentHP, maxHP);

    }
    [PunRPC]
    public void AddHealth(int amoutToAdd)
    {
        maxHP += amoutToAdd;
        PlayerPrefs.SetInt("MaxHP", maxHP);

        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, currentHP, maxHP);
        GameUI.instance.UpdateHPText(currentHP, maxHP);
    }
    [PunRPC]
    public void BuyHealth(int itemPrice)
    {
        if(gold >= itemPrice)
        {
            AudioManager.instance.PlaySFX(9);
            AddHealth(10);
            gold -= itemPrice;
            PlayerPrefs.SetInt("Gold", gold);
            GameUI.instance.UpdateGoldText(gold);
        }
    }

    public void BuyAttack(int itemPrice)
    {
        if (gold >= itemPrice)
        {
            AudioManager.instance.PlaySFX(9);
            damage++;
            PlayerPrefs.SetInt("Attack", damage);
            gold -= itemPrice;
            PlayerPrefs.SetInt("Gold", gold);
            GameUI.instance.UpdateGoldText(gold);
            GameUI.instance.UpdateAdText(damage);
        }
    }

    public void BuyDef(int itemPrice)
    {
        if (gold >= itemPrice)
        {
            def++;
            PlayerPrefs.SetInt("Def", def);
            gold -= itemPrice;
            PlayerPrefs.SetInt("Gold", gold);
            GameUI.instance.UpdateGoldText(gold);
            GameUI.instance.UpdateDFText(def);
        }
    }

    [PunRPC]
    void GetGold(int goldToGive)
    {
        AudioManager.instance.PlaySFX(7);
        gold += goldToGive;
        PlayerPrefs.SetInt("Gold", gold);
        GameUI.instance.UpdateGoldText(gold);
    }
    [PunRPC]
    public void EarnExp(int xpAmount)
    {
        currentExp += xpAmount;
        PlayerPrefs.SetInt("CurrentEXP", currentExp);
        LevelUp();
        GameUI.instance.UpdateLevelText(currentExp, maxExp);
    }
    public void LevelUp()
    {
        while(currentExp >= maxExp)
        {
            AudioManager.instance.PlaySFX(3);
            PhotonNetwork.Instantiate(levelUpEffect, transform.position, Quaternion.identity);
            currentExp -= maxExp;
            maxExp = (int)(maxExp * 1.5f);
            playerLevel++;
            headerInfo.photonView.RPC("UpdatePlayerLevel", RpcTarget.All, playerLevel);
            GameUI.instance.UpdateLevelText(currentExp, maxExp);
            PlayerPrefs.SetInt("PlayerLevel", playerLevel);
            PlayerPrefs.SetInt("CurrentEXP", currentExp);
            PlayerPrefs.SetInt("MaxEXP", maxExp);
            damage++;
            PlayerPrefs.SetInt("Attack", damage);
            GameUI.instance.UpdateAdText(damage);
            AddHealth(5);
        }
    }
}
