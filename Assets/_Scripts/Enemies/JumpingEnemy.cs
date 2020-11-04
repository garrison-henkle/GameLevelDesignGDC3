using UnityEngine;

public class JumpingEnemy : StandardEnemy
{
    public override void Move()
    {
        //call StandardEnemy movement (handles horizontal movement)
        base.Move();
        if (canJump)
        {
            var rand = Random.Range(0, 360); //assuming 60fps, 60fps*6sec = 360 to get a random jump every 6 seconds on average; with faster fps, it will happen more often
            if (rand == 0)
                Jump();
        }
    }

    private void Jump()
    {
        rigid.AddForce(rigid.transform.up * jumpForce);
        canJump = false;
    }
}
