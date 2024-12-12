using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Range(0.01f, 20.0f)] [SerializeField] private float moveSpeed = 10f;
    [Range(0.01f, 30.0f)] [SerializeField] private float groundAcceleration = 9f;
    [Range(0.01f, 30.0f)] [SerializeField] private float groundDecceleration = 25f;
    [Range(0.01f, 20.0f)] [SerializeField] private float airAcceleration = 6f;
    [Range(0.01f, 20.0f)] [SerializeField] private float airDecceleration = 6f;
    [Range(0.01f, 20.0f)] [SerializeField] private float groundFriction = 6f;

    [Header("Jump")]
    [Range(0.01f, 40.0f)] [SerializeField] private float jumpForce = 15f;
    [Range(0.001f, 10.0f)] [SerializeField] private float coyoteTime = 0.5f;
    [Range(0.001f, 10.0f)] [SerializeField] private float jumpBuffer = 0.5f;
    [Space(5)]
    [Range(1f, 4f)] [SerializeField] private float baseGravityFactor = 1f;
    [Range(1f, 4f)] [SerializeField] private float jumpCutGravityFactor = 2f;
    [Range(1f, 4f)] [SerializeField] private float fallGravityFactor = 1.5f;
    [Range(0f, 4f)] [SerializeField] private float hangGravityFactor = 0.5f;
    [Range(0f, 4f)] [SerializeField] private float hangThreshold = 1.0f;

    [Header("Grapple")]
    [Range(0f, 1000f)] [SerializeField] private float grappleMaxRange = 500f;

        private enum GrappleState {
        Released,
        Launched,
        Pulled
    } 
    // NOTE: grappleState is not intended to be modified in the inspector
    [SerializeField] GrappleState grappleState = GrappleState.Released;
    [Range(0f, 1.0f)] [SerializeField] private float grappleSlowMotionFactor = 0.5f;
    [Range(0f, 100.0f)] [SerializeField] private float grapplePullForce = 10.0f;


    [Header("Features")]
    [Range(1, 10)] [SerializeField] private int maxLives = 3;
    [SerializeField] private GameObject respawnPoint;
    
    [Header("Audio")]
    [SerializeField] private AudioClip bonusSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip keySound;
    [SerializeField] private AudioClip killSound;
    [SerializeField] private AudioClip finishSound;
    [SerializeField] private AudioClip lifeSound;
    [SerializeField] private AudioClip grappleLaunchSound; // TODO
    [SerializeField] private AudioClip grapplePullSound; // TODO

    // public int keysFound = 0;
    // public int keysNumber = 3;
    public LayerMask groundLayer;

    /*private int _score = 0;
    public int score {
        get {
            return _score;
        }
        set {
            _score = value;
            Debug.Log("Score: " + _score);
        }
    }*/


    public Rigidbody2D rigidBody {get; private set;}
    public Animator animator {get; private set;}

    public float lastOnGroundTime {get; private set;}
    public float lastJumpInputTime {get; private set;}
    public bool isInJumpPoint {get; private set;}
    
    private bool isFacingRight = true;
    private bool isInLadder = false;
    private bool isClimbing = false;

    private bool isJumping;
    private bool isJumpCut;
    private Rigidbody2D platform;

    private Vector2 moveDirection;

    private SpringJoint2D grapplingSpring;

    private AudioSource audioSource;



    // On component creation
    private void Awake()
    {
        // GameManager.instance.lives = maxLives; // FIXME: GameManager is hardcoded to support 3 lives max.
        transform.position = respawnPoint.transform.position;
        rigidBody = GetComponent<Rigidbody2D>();
        grapplingSpring = GetComponent<SpringJoint2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody.gravityScale = baseGravityFactor;
        grapplingSpring.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Timers
        lastOnGroundTime -= Time.deltaTime;
        lastJumpInputTime -= Time.deltaTime;

        // Jump
        if (isJumping && rigidBody.velocity.y < 0) {
			isJumping = false;
		}
        if (lastOnGroundTime > 0 && !isJumping){
            isJumpCut = false;
        }


        if (canJump() && lastJumpInputTime > 0)
        {
            isJumping = true;
            isJumpCut = false;
            Jump();
        }

        if (isInLadder && moveDirection.y != 0){
            isClimbing = true;
            animator.SetBool("isClimbing", true);
        }

        // Gravity
        if (rigidBody.velocity.y < 0) {
            // if falling
            rigidBody.gravityScale = baseGravityFactor * fallGravityFactor;
        } else if (isJumpCut) {
            // if player stopped the jump early
            rigidBody.gravityScale = baseGravityFactor * jumpCutGravityFactor;
        } else if (isJumping && Mathf.Abs(rigidBody.velocity.y) < hangThreshold) {
            // if we are in the highest point during the jump
            rigidBody.gravityScale = baseGravityFactor * hangGravityFactor;
        } else {
            // if we are just doing normal stuff
            rigidBody.gravityScale = baseGravityFactor;
        }

        //animator.SetBool("isGrounded", lastOnGroundTime > 0);
        animator.SetBool("isWalking", Mathf.Abs(moveDirection.x) > 0.1);
        animator.SetBool("isFalling", rigidBody.velocity.y < 0.1);

        // Flip sprite
        transform.localScale = isFacingRight ? new Vector3(1,1,1) : new Vector3(-1,1,1);
        // Debug 
        drawDebug();
    }

    public void OnMovement(InputAction.CallbackContext ctx){
        moveDirection = ctx.ReadValue<Vector2>();

        if(moveDirection.x >= 0.01){
            isFacingRight = true;
        } else if(moveDirection.x <= -0.01){
            isFacingRight = false;
        }
    }

    public void OnJump(InputAction.CallbackContext ctx){
        if(ctx.started){
            lastJumpInputTime = jumpBuffer;
        }
    }

    public void OnJumpCut(InputAction.CallbackContext ctx){
        if(ctx.started){
            if(canJumpCut()) {
                isJumpCut = true;
            }
        }
    }

    public void onGrappleLaunch(InputAction.CallbackContext ctx){
        if(grappleState != GrappleState.Released){ return; }

        if(ctx.started){
            Debug.Log("Preparing grapple");
            Time.timeScale = grappleSlowMotionFactor;
        }
        if(ctx.canceled){
            Debug.Log("Launching grapple");
            // Project a ray through mouse position up to a nearby collider
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 playerPosition = transform.position;
            Vector2 direction = (mousePosition - playerPosition).normalized;
            
            // If the ray hits a collider, create a spring joint between the player and the hit point
            RaycastHit2D hit = Physics2D.Raycast(playerPosition, direction, grappleMaxRange, groundLayer);
            if(hit.collider != null){
                grapplingSpring.enabled = true;
                grapplingSpring.connectedBody = hit.collider.attachedRigidbody;
                grapplingSpring.connectedAnchor = hit.point;
                grapplingSpring.distance = Vector2.Distance(playerPosition, hit.point);
                grappleState = GrappleState.Launched;
            }

            Debug.Log("Grapple:"+grappleState + " Connected to:"+hit.collider);
            Time.timeScale = 1.0f;
        }
    }

    public void onGrapplePull(InputAction.CallbackContext ctx){
        // if (grappleState != GrappleState.Launched){ return; }
        if (grapplingSpring.enabled == false){ return; }

        if(ctx.performed){ // FIXME: gets called only once instead of every frame
            Debug.Log("Pulling grapple");
            // Pull the player towards the grapple point
            grapplingSpring.distance -= grapplePullForce * Time.deltaTime;
            grapplingSpring.enabled = true;

            grappleState = GrappleState.Pulled;
        }
        if (ctx.canceled){
            releaseGrapple();
        }
    }

    public void onGrappleRelease(InputAction.CallbackContext ctx){
        // if (grappleState == GrappleState.Released){ return; }

        if(ctx.started){
            releaseGrapple();
        }
    }

    public void releaseGrapple(){
        Debug.Log("Releasing grapple");
        grapplingSpring.enabled = false;
        grappleState = GrappleState.Released;
    }


    private void OnTriggerStay2D(Collider2D other) {
        // checks for collision and if it's happening starts the timer in which we can still jump after it stops.
        if((groundLayer.value & (1 << other.transform.gameObject.layer)) > 0) {
            animator.SetBool("isGrounded", true);
            lastOnGroundTime = coyoteTime; 
        }
    }

    

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("JumpPoint")){
            isInJumpPoint = true;
        } else if(other.CompareTag("MovingPlatform")){
            platform = other.gameObject.GetComponent<Rigidbody2D>();
        } 
        if(other.CompareTag("Bonus")){
            GameManager.instance.score += 1;
            other.gameObject.SetActive(false);
            audioSource.PlayOneShot(bonusSound, AudioListener.volume);
        }
        if(other.CompareTag("Ladder")){
            isInLadder = true;
        }
        if(other.CompareTag("Key")){
            GameManager.instance.keyFound(other.gameObject.GetComponent<SpriteRenderer>().color);
            // Debug.Log("Found key. Current key number: " + keysFound);
            other.gameObject.GetComponent<SpriteRenderer>().color = GameManager.disabledKeyColor;
            other.enabled = false;
            audioSource.PlayOneShot(keySound, AudioListener.volume);
        }
        if(other.CompareTag("Heart")){
            if (GameManager.instance.lives < maxLives){
                GameManager.instance.lives += 1;
                other.gameObject.SetActive(false);
                audioSource.PlayOneShot(lifeSound, AudioListener.volume);
            }
        }
        if (other.CompareTag("Finish")){
            // NOTE: The rest of the finish interaction is in Finish.cs
            if (GameManager.instance.keysFound == GameManager.instance.keyIcons.Length){
                audioSource.PlayOneShot(finishSound, AudioListener.volume);
            }
        }
    }

    public void KilledEnemy(int points){
        GameManager.instance.score += points;
        GameManager.instance.enemiesKilled += 1;
    }

    public void TakeDamage(int damage){
        animator.SetTrigger("Hurt");
        GameManager.instance.lives -= damage;
        audioSource.PlayOneShot(hurtSound, AudioListener.volume);
    }

    private void OnTriggerExit2D(Collider2D other) {
        if((groundLayer.value & (1 << other.transform.gameObject.layer)) > 0) {
            animator.SetBool("isGrounded", false);
        }   
        if(other.CompareTag("JumpPoint")){
            isInJumpPoint = false;
        } else if(other.CompareTag("MovingPlatform")){
            platform = null;
        }
        if(other.CompareTag("Ladder")){
            isInLadder = false;
            isClimbing = false;
            animator.SetBool("isClimbing", false);
        }
    }

    void FixedUpdate(){
        Run();
        if(isClimbing == true){
            rigidBody.gravityScale = 0;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, moveDirection.y * moveSpeed);
        } else {
            rigidBody.gravityScale = baseGravityFactor;
        }
        if(platform != null){
            rigidBody.velocity = new Vector2(rigidBody.velocity.x + platform.velocity.x/2, rigidBody.velocity.y);
        }
    }

    void Run() {
        float targetSpeed = moveDirection.x * moveSpeed;
        float speedDiff = targetSpeed - rigidBody.velocity.x;

        float acceleration;
        // calculate acceleration
        if(lastOnGroundTime > 0){
            acceleration = (Mathf.Abs(targetSpeed) > 0.01f) ? groundAcceleration : groundDecceleration;
        } else {
            acceleration = (Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDecceleration;
        }

        // don't change acceleration if moving faster than moveSpeed
        if(Mathf.Abs(targetSpeed) > Mathf.Abs(rigidBody.velocity.x) && Mathf.Sign(targetSpeed) == Mathf.Abs(rigidBody.velocity.x)) {
            acceleration = 0;
        }

        // additional friction
        // skipped if on a moving platform
        if(platform == null){
            if(Mathf.Abs(moveDirection.x) == 0 && lastOnGroundTime > 0){
                float frictionAmmount = Mathf.Min(Math.Abs(rigidBody.velocity.x), Mathf.Abs(groundFriction));
                frictionAmmount *= -Mathf.Sign(rigidBody.velocity.x);

                rigidBody.AddForce(Vector2.right * frictionAmmount, ForceMode2D.Impulse);
            }
        } 

        float movement = speedDiff * acceleration;

        rigidBody.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    void Jump()
    {
        // prevent jumping multiple times on one input
        lastJumpInputTime = 0;
        lastOnGroundTime = 0;
        // prevent the 2x jump when mashing space
        if(isInJumpPoint){
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
        }
        isInJumpPoint = false;

        float force = jumpForce;

        // compensate for jumping while falling
        if (rigidBody.velocity.y < 0)
            force -= rigidBody.velocity.y;

        // apply jump impulse
        rigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    bool canJump()
    {
        return (lastOnGroundTime > 0 && !isJumping) || isInJumpPoint;
    }

    bool canJumpCut() {
        return isJumping && rigidBody.velocity.y > 0;
    }

    void drawDebug() {
        if (grapplingSpring.enabled){
            Debug.DrawLine(transform.position, grapplingSpring.connectedAnchor, Color.red);
        }
    }
}
