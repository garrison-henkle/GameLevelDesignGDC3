using UnityEngine;

public class Bullet : MonoBehaviour
{
    //bullet properties
    private float speed = 15f;
    private int damage = 10;

    //travel direction
    private Vector3 direction;

    //targets
    private string curTag;

    //audio
    private AudioSource player;

    public void Activate(Vector3 position, Vector3 direction, float speed, int damage, float effectiveRange, string tag)
    {
        //set the bullet properties
        transform.position = position;
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.curTag = tag;

        //calculate the time it will be alive (the time it takes to travel its effective range)
        var timer = effectiveRange / speed;
        Invoke(nameof(Delete), timer);

        //play audio
        player = GetComponent<AudioSource>();
        player.time = 0;
        player.Play();

        //activate the object
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.Translate(direction * Time.deltaTime * speed);
    }

    private void Delete()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(curTag))
        {
            var health = collision.gameObject.GetComponent<Health>();
            if (health != null)
                health.Damage(damage);
            Delete();
        }
        if (collision.gameObject.CompareTag("Floor"))
            Delete();
    }
}
