public abstract class MovementEnemy : Enemy
{
    /*Attacking*/

    public override void Attack()
    {
        if (playerHealth != null)
            playerHealth.Damage(damage);
        StartCooldown(attackInterval);
    }

    public override bool CanAttack()
    {
        var distance = (player.transform.position - transform.position).magnitude;
        return distance <= attackRadius;
    }
}
