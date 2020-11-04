using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    //target
    [Header("Target")]
    public GameObject player;
    public string playerTag = "Player";
    [HideInInspector]
    public Health playerHealth;

    //movement
    [Header("Movement")]
    public float speed;
    public float aggroRadius;

    //jumping
    [Header("Jumping")]
    public float jumpForce = 300f;
    public string jumpResetTag = "Floor";
    [HideInInspector]
    public bool canJump = true;
    public bool canInitiallyJump = true;
    [HideInInspector]
    public Rigidbody rigid;

    //damage
    [Header("Damage")]
    public int damage;
    public float attackInterval;
    public float attackRadius;
    [HideInInspector]
    public bool attackCooldown = false;

    public void Start()
    {
        canJump = canInitiallyJump;
        playerHealth = player.GetComponent<Health>();
        rigid = GetComponent<Rigidbody>();
    }

    //core update loop
    public void Update()
    {
        if (!attackCooldown && CanAttack() && gameObject.layer != 10)
            Attack();
        if (CanMove())
            Move();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //reset jumping once the floor is hit
        if (collision.gameObject.CompareTag(jumpResetTag))
            canJump = true;
    } //OnCollisionEnter

    /*Movement*/

    public bool CanMove()
    {
        var distance = (player.transform.position - transform.position).magnitude;
        return distance <= aggroRadius;
    }

    public abstract void Move();
    
    /*Attacking*/

    public abstract void Attack();
    
    public abstract bool CanAttack();

    /*Cooldowns*/

    public void StartCooldown(float cooldown)
    {
        attackCooldown = true;
        Invoke(nameof(EndCooldown), cooldown);
    }

    public void EndCooldown()
    {
        attackCooldown = false;
    }
}
