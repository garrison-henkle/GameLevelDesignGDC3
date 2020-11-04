using UnityEngine;

public class Health : MonoBehaviour
{
    private GameManager manager;

    [Header("Health")]
    public int startingHealth = 100;
    public int health { private set; get; }

    [Header("Audio")]
    public AudioClip hurt;
    private AudioSource player;

    [Header("Effects")]
    public GameObject deathEffectObject;
    public GameObject damageEffectObject;
    private ParticleSystem deathEffect;
    private ParticleSystem damageEffect;

    [Header("Tags")]
    public string playerTag = "Player";
    public string enemyTag = "Enemy";

    [Header("Score")]
    public int score = 20;

    private bool dead = false;

    private void Start()
    {
        //find the game manager
        var managerObj = GameObject.Find("GameManager");
        if (managerObj != null)
        {
            manager = managerObj.GetComponent<GameManager>();
            manager.player = gameObject;
        }
        else
        {
            manager = null;
            Debug.Log("manager is null");
        }

        //find the audio source
        player = GetComponent<AudioSource>();
        player.loop = false;

        //get particle systems
        deathEffect = deathEffectObject.GetComponent<ParticleSystem>();
        damageEffect = damageEffectObject.GetComponent<ParticleSystem>();

        //set the health
        health = startingHealth;
    }

    private void Update()
    {
        if (gameObject.CompareTag(playerTag) && manager != null)
            manager.health = health;
    }

    public void Damage(int damage)
    {
        var healthAfterDamage = health - damage;
        if (healthAfterDamage <= 0)
            Death();
        else
        {
            health -= damage;
            if (damageEffect != null)
                damageEffect.Play();
        } 
    }

    public void Heal(int heal)
    {
        health += heal;
    }

    private void Death()
    {
        gameObject.layer = 10;
        if (deathEffect != null && hurt != null)
        {
            deathEffect.Play();
            if(dead == false)
            {
                player.clip = hurt;
                player.time = 0;
                player.loop = false;
                player.Play();
                dead = true;
            }
            Invoke(nameof(Delete), hurt.length);
        }
        else
            Delete();
        
    }
    private void Delete()
    {
        if(manager != null)
        {
            if (gameObject.CompareTag(playerTag))
                manager.state = GameManager.GameState.Death;
            if (gameObject.CompareTag(enemyTag))
            {
                manager.Score(score);
                Destroy(gameObject);
            }
        }
    }
}
