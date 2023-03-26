using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAction : MonoBehaviour
{
    float h;
    float v;
    public float velocity;
    public Rigidbody2D rigid;
    Animator anim;
    Vector3 dirVec;
    GameObject scanObject;
    bool isMoving = false;
    public bool isDead = false;
    SpriteRenderer sprite;

    public float atk;
    //public ParticleSystem dust;
    public int curPlayerHp;
    public int maxPlayerHp;
    public Image[] playerHpImg = new Image[2];
    public Image coolDownImg;
    // public bool isOverHealed;
    public bool randomMode;
    public float curTime;
    WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();
    //public float timer;
    public bool isTimeOver;
    public Image attackIcon;
    public Color pressedColor;
    bool isAttackEnable = true;

    Coroutine dotCo;
    Coroutine timerCo;
    Coroutine attackCo;

    public Text playerHPText;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        //resetPlayer();
        //hpBarRefresh(curPlayerHp);
        //StartCoroutine(dotdamage());
        if(randomMode) StartCoroutine(randomMove());
        //Debug.Log("플레이어 start");
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.GM.isGameOver) return;
        // 플레이어 이동  
        if (!randomMode)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        if (h != 0 || v != 0)
        {
            if (!isMoving)
            {
                isMoving = true;
                anim.SetBool("isMoving", isMoving);
                //dust.Play();
                
            }
            dirVec = new Vector2(h, v).normalized;
            if (h < 0) sprite.flipX = true;
            else sprite.flipX = false;
        }
        else
        {
            isMoving = false;
            anim.SetBool("isMoving", isMoving);
            //dust.Stop();
        }

        // scan object
        if (Input.GetKeyDown(KeyCode.X) && scanObject) Debug.Log(scanObject);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Attack(scanObject);
        }

    }

    IEnumerator randomMove()
    {
        while (true)
        {
            h = Random.Range(-1, 2);
            v = Random.Range(-1, 2);
            yield return new WaitForSeconds(2.0f);
        }
    }

    public void Attack(GameObject scanObject)
    {
        StartCoroutine(IconEffect(attackIcon));
     
        if (isDead || !isAttackEnable) return;

        StartCoroutine(attackCoolTime());
        anim.SetTrigger("doAttack");
        if (!scanObject) return;
        scanObject.GetComponent<BossAction>().getDamage(atk);

    }

    IEnumerator IconEffect(Image icon)
    {
        Color origin = icon.color;
        icon.color *= pressedColor;
        yield return new WaitForSeconds(0.1f);
        icon.color = origin;
    }

    public void getDamage(int damage)
    {
        if (isDead || damage <= 0) return;

        curPlayerHp -= damage;
        anim.SetTrigger("doHit");

        if (curPlayerHp <= 0) {
            curPlayerHp = 0;
            playerDie();
        } 

        hpBarRefresh(curPlayerHp);
    }

    public void playerDie()
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger("doDie");
        StopCoroutine(dotdamage());
       // StopCoroutine(startTimer());
        //GameManager.GM.setGameOver();
        //resetPlayer();
    }

    void hpBarRefresh(int hp)
    {
        playerHPText.text = "HP " + hp ;
        foreach(Image img in playerHpImg)
        {
            img.fillAmount = (float)hp / maxPlayerHp;
        }
        
    }

    void FixedUpdate()
    {
        rigid.velocity = new Vector2(h, v).normalized * Time.deltaTime * velocity; // 플레이어 이동

        // Debug.DrawRay(rigid.position, dirVec * 1.5f, new Color(0, 1, 0)); // 레이캐스트

        // Boss 레이어만 스캔
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, dirVec, 1.5f, LayerMask.GetMask("Boss"));
        if (rayHit.collider)
        {
            scanObject = rayHit.collider.gameObject;
        }
        else scanObject = null;


    }

    public void getHealing(int value)
    {
        if (isDead) return;
        //isOverHealed = false;
        //Debug.Log(curPlayerHp);
        curPlayerHp += value;
        if (curPlayerHp > maxPlayerHp)
        {
            curPlayerHp = maxPlayerHp;
            //isOverHealed = true;
        }
        hpBarRefresh(curPlayerHp);
        //Debug.Log(isOverHealed);
    }

    public IEnumerator dotdamage()
    {
        while (true)
        {
            int damage = Random.Range(0, 5);
            getDamage(damage);
            yield return new WaitForSeconds(1.0f);
        }
    }

    //public IEnumerator startTimer()
    //{
    //    curTime = 0;
    //    //Debug.Log("start timer");

    //    while (true)
    //    {
    //        if (curTime < timer)
    //        {
    //            yield return fixedUpdate;
    //            curTime += Time.deltaTime;
    //        }
    //        else
    //        {
    //            //Debug.Log("timeover");
    //            //GameManager.GM.timeOver = true;
    //            isTimeOver = true;
    //            yield break;
    //        }
    //    }

    //}

    IEnumerator attackCoolTime()
    {
        float t = 0f;
        isAttackEnable = false;
        coolDownImg.fillAmount = 1;

        while (true)
        {
            if (t < 2)
            {
                yield return fixedUpdate;
                t += Time.deltaTime;
                coolDownImg.fillAmount = (2-t) / 2;
            }
            else
            {
                isAttackEnable = true;
                coolDownImg.fillAmount = 0;
                yield break;
            }
        }
    }
    public void resetPlayer()
    {
        if(dotCo != null)
            StopCoroutine(dotCo);
        if(timerCo != null)
            StopCoroutine(timerCo);

        float x = Random.Range(-8.0f, 8.0f);
        float y = Random.Range(-4.0f, 5.0f);
        transform.localPosition = new Vector3(x, y, 0);

        //transform.localPosition = new Vector3(2, 0, 0);
        curPlayerHp = maxPlayerHp;
        //GameManager.GM.isGameOver = false;
        //GameManager.GM.timeOver = false;
        isTimeOver = false;
        isDead = false;
        anim.ResetTrigger("doDie");
        hpBarRefresh(curPlayerHp);

        //dotCo = StartCoroutine(dotdamage());
        //timerCo = StartCoroutine(startTimer());

        //StartCoroutine(dotdamage());
        //StartCoroutine(startTimer());
    }
}
