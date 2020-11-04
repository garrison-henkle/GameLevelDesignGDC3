using UnityEngine;

public class TurretEnemy : Enemy
{
    //enemy
    [Header("Objects")]
    public GameObject enemyHands;
    public GameObject weaponObject = null;
    public GameObject bulletObject = null;

    //weapons
    [Header("Weapons")]
    public float projectileSpeed = 10f;

    //aiming
    private RaycastHit hit;
    private Vector3 direction;

    private new void Update()
    {
        //aim the gun at the target
        direction = player.transform.position - enemyHands.transform.position;
        var rotation = Quaternion.LookRotation(direction);
        rotation.y = rotation.x = 0;
        weaponObject.transform.rotation = rotation;

        base.Update();
    }

    public override void Attack()
    {
        var direction = player.transform.position - transform.position;
        var scale = transform.localScale;

        //ensure the enemy is turned in the correct direction - don't let the enemy fire backwards (through themselves)
        if (direction.x >= 0 && scale.x > 0 ||
            direction.x <= 0 && scale.x < 0)
        {
            CreateBullet(enemyHands.transform.position, direction);
            StartCooldown(attackInterval);
        }
    }

    public override bool CanAttack()
    {
        bool canAttack = false;
        var distance = direction.magnitude;

        if (distance <= attackRadius)
        {
            Physics.Raycast(enemyHands.transform.position, direction, out hit, attackRadius);
            if (hit.collider.gameObject.CompareTag(playerTag))
                canAttack = true;
        }

        return canAttack;
    }

    private void CreateBullet(Vector3 position, Vector3 direction)
    {
        //create the bullet
        var bullet = Instantiate(bulletObject);

        //activate the new bullet
        var attr = bullet.GetComponent<Bullet>();
        attr.Activate(position, direction, projectileSpeed, damage, attackRadius, playerTag);
    }

    public override void Move(){
        var scale = transform.localScale;
        if (direction.x < 0 && scale.x > 0 ||
            direction.x > 0 && scale.x < 0)
                scale.x *= -1;
        transform.localScale = scale;
    }
}
