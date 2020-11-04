using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    //game manager
    private GameManager manager;

    //movement
    [Header("Movement")]
    public float movementSpeed = 3f;
    public float boostSpeed = 6f;
    private float speed;
    private Rigidbody rigid;

    //jumping
    [Header("Jumping")]
    public float jumpForce = 300f;
    public string jumpResetTag = "Floor";
    private bool canJump = true;
    public bool canInitiallyJump;

    //animation
    private bool isIdle = true;
    private Animator animator;
    private bool lastIdle; //store the previous idle to detect changes

    void Start()
    {
        //find the game manager
        var managerObj = GameObject.Find("GameManager");
        if (managerObj != null)
        {
            manager = managerObj.GetComponent<GameManager>();
            manager.player = gameObject;
        }
        else
            manager = null;

        //player rigidbody
        var rigidObj = GetComponent<Rigidbody>();
        rigid = rigidObj == null ? null : rigidObj;

        //player animator
        animator = GetComponent<Animator>();
        lastIdle = isIdle;

        //set jump state
        canJump = canInitiallyJump;
    } //Start

    private bool isFirst = true;
    private void Update()
    {
        //lock cursor to the screen
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        //movement
        var hdir = Input.GetAxis("Horizontal");
        var forward = new Vector3(1, 0, 0);
        var direction = forward * hdir;
        speed = Input.GetKey(KeyCode.LeftShift) ? boostSpeed : movementSpeed;
        transform.Translate(direction * (speed * Time.deltaTime));
        var scale = transform.localScale;
        if ((direction.x < 0 && scale.x > 0) ||
            (direction.x > 0 && scale.x < 0))
                scale.x *= -1;
        transform.localScale = scale;

        //fix bug where the player flies up with no input
        if (isFirst)
        {
            transform.Translate(forward * (speed * Time.deltaTime));
            isFirst = false;
            direction = forward;
        }
            
        //jumping
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            rigid.AddForce(rigid.transform.up * jumpForce);
            canJump = false;

            //jump animation
            animator.SetFloat("speed", 0f);
            animator.SetTrigger("jumpStart");
        }

        //animation
        //maintain a variable with the current and previous isIdle value
        //if the value changes, how it changed can be determined to make
        //the correct transition. Transitions are rising edge.

        //state
        if (direction == Vector3.zero)
            isIdle = true;
        else
            isIdle = false;

        //walk to idle
        if (isIdle && !lastIdle)
            animator.SetTrigger("toIdle");
        //idle to walk
        if (!isIdle && lastIdle)
            animator.SetTrigger("toWalk");

        //update state
        lastIdle = isIdle;
        
    } //Update

    private void OnCollisionEnter(Collision collision)
    {
        //jumping
        if (collision.gameObject.CompareTag(jumpResetTag))
        {
            canJump = true;

            //cancel jump animation
            animator.SetFloat("speed", 1f);
            animator.SetTrigger("toWalk");
        }
    } //OnCollisionEnter
}
