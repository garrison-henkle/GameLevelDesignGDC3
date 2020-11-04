using UnityEngine;

public class StandardEnemy : MovementEnemy
{
    public override void Move()
    {
        //vector that points to the player
        var direction = player.transform.position - transform.position;

        //scale for flipping the sprite
        var scale = transform.localScale;

        //handle horizontal
        float horizontalDirection = 0;
        if (direction.x < 0)
        {
            horizontalDirection = -1;
            if (scale.x > 0) scale.x *= -1; //flip sprite
        }
        else if (direction.x > 0)
        {
            horizontalDirection = 1;
            if (scale.x < 0) scale.x *= -1; //flip sprite
        }
        transform.Translate(horizontalDirection * new Vector3(1, 0, 0) * (speed * Time.deltaTime));

        //handle sprite flipping
        transform.localScale = scale;
    }
}
