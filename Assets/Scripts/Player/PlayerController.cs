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
	[SerializeField] private bool grappleAllowed = true;
    [Range(0f, 1000f)] [SerializeField] private float grappleMaxRange = 500f;

	private enum GrappleState {
        Released,
		Preparing,
        Launched,
        Pulled
    } 
    // NOTE: grappleState is not intended to be modified in the inspector
    [SerializeField] GrappleState grappleState = GrappleState.Released;
    [Range(0f, 1.0f)] [SerializeField] private float grappleSlowMotionFactor = 0.5f;
    [Range(0f, 100.0f)] [SerializeField] private float grapplePullForce = 10.0f;
	[SerializeField] private GameObject ropeWrapper;
	[SerializeField] private SpriteRenderer ropeSprite;
	[SerializeField] private GameObject crosshair;


    [Header("Respawning")]
    [SerializeField] private GameObject respawnPoint;
	[SerializeField] private float invincibilityTimer = 0.5f;
    private GameObject checkPoint;
    
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

    public LayerMask groundLayer;

    public Rigidbody2D rigidBody {get; private set;}
    public Animator animator {get; private set;}

    public float lastOnGroundTime {get; private set;}
    public float lastJumpInputTime {get; private set;}
    public float timeSinceLastHurt {get; private set;}
    public bool isInJumpPoint {get; private set;}
    
    private bool isFacingRight = true;
    private bool isInLadder = false;
    private bool isClimbing = false;
    private Vector2 windForce = Vector2.zero;

    private bool isJumping;
    private bool isJumpCut;
    private Rigidbody2D platform;

    private Vector2 moveDirection;

    private SpringJoint2D grapplingSpring;

    private AudioSource audioSource;



    // On component creation
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        grapplingSpring = GetComponent<SpringJoint2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        checkPoint = respawnPoint;
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.position = respawnPoint.transform.position;
        rigidBody.gravityScale = baseGravityFactor;
        grapplingSpring.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
		// draw gizmos and crosshair
		if(grappleState == GrappleState.Preparing) {
			var hitResult = getGrappleConnectionPoint();
			if(hitResult.HasValue) {
				RaycastHit2D hit = hitResult.Value;
				crosshair.transform.position = hit.point;
				crosshair.SetActive(true);
			}
			else {
				crosshair.SetActive(false);
			}
		}
		else {
			crosshair.SetActive(false);
		}

        // Timers
        lastOnGroundTime -= Time.deltaTime;
        lastJumpInputTime -= Time.deltaTime;
        timeSinceLastHurt += Time.deltaTime;

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
		ropeSprite.flipX = !isFacingRight;

		// Update rope sprite
		if(grapplingSpring.enabled) {
			float targetDistance = grapplingSpring.distance;

			Vector2 displacement = grapplingSpring.connectedBody.transform.TransformPoint(grapplingSpring.connectedAnchor) - transform.position;
			Vector2 direction = displacement.normalized;
			float distance = displacement.magnitude;

			ropeSprite.size = new Vector2(0.25f, targetDistance);
			ropeSprite.transform.localScale = new Vector3(1, distance/targetDistance, 1);
			ropeSprite.transform.localPosition = new Vector3(0, distance/2, 0);
			ropeWrapper.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, direction));
		}
		else {
			ropeSprite.size = new Vector2(0.25f, 0);
		}

        // Debug 
        drawDebug();
    }

    public void OnMovement(InputAction.CallbackContext ctx){
        if(GameManager.instance.currGameState != GameManager.GameState.IN_GAME) {return;}
        moveDirection = ctx.ReadValue<Vector2>();

        if(moveDirection.x >= 0.01){
            isFacingRight = true;
        } else if(moveDirection.x <= -0.01){
            isFacingRight = false;
        }
    }

    public void OnJump(InputAction.CallbackContext ctx){
        if(GameManager.instance.currGameState != GameManager.GameState.IN_GAME) {return;}
        if(ctx.started){
            lastJumpInputTime = jumpBuffer;
        }
    }

    public void OnJumpCut(InputAction.CallbackContext ctx){
        if(GameManager.instance.currGameState != GameManager.GameState.IN_GAME) {return;}
        if(ctx.started){
            if(canJumpCut()) {
                isJumpCut = true;
            }
        }
    }

	public static Vector2 rotate(Vector2 v, float delta) {
		return new Vector2(
			v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
			v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
    	);
	}

	private float raycastAngleOfIteration(int i) {
		float side = (i%2)*2-1; // alternate 1/-1
		float magnitude = ((int)((i+1)/2))*0.03f; // (0, 1, 1, 2, 2, 3, 3)*0.03
		float angle = side * magnitude;

		return angle;
	}

	public RaycastHit2D? getGrappleConnectionPoint() {
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		Vector2 playerPosition = transform.position;
		Vector2 direction = (mousePosition - playerPosition).normalized;
		
		bool found = false;
		RaycastHit2D result = Physics2D.Raycast(playerPosition, direction, grappleMaxRange, groundLayer);

		// sweeping pass - search for first hit
		int i = 0;
		while(i < 9) {
			float angle = raycastAngleOfIteration(i);

			Vector2 adjustedDirection = rotate(direction, angle);

			Debug.DrawLine(transform.position, transform.position + (Vector3)adjustedDirection*20);
			result = Physics2D.Raycast(playerPosition, adjustedDirection, grappleMaxRange, groundLayer);
			if(result.collider != null && !result.collider.gameObject.CompareTag("Spikes") && result.collider.attachedRigidbody != null) {
				found = true;
				break;
			}
			i++;
		}

		if(!found) {
			return null;
		}

		if(i != 0) {
			// binary search - search between first hit and previous non-hit
			float high = raycastAngleOfIteration(i);
			float low = raycastAngleOfIteration(i-2);

			for(int j = 0; j < 5; j ++) {
				float mid = (high + low) / 2;
				Vector2 adjustedDirection = rotate(direction, mid);
				RaycastHit2D hit = Physics2D.Raycast(playerPosition, adjustedDirection, grappleMaxRange, groundLayer);
				if(hit.collider != null && !hit.collider.gameObject.CompareTag("Spikes") && hit.collider.attachedRigidbody != null) {
					high = mid;
					result = hit;
				}
				else {
					low = mid;
				}
			}
		}

		return result;
	}

    public void onGrappleLaunch(InputAction.CallbackContext ctx){
        if(GameManager.instance.currGameState != GameManager.GameState.IN_GAME) {return;}
		if(!grappleAllowed) { return; }
        if(grappleState != GrappleState.Released && grappleState != GrappleState.Preparing){ return; }

        if(ctx.started){
            // Debug.Log("Preparing grapple");
			grappleState = GrappleState.Preparing;
            Time.timeScale = grappleSlowMotionFactor;
        }
        if(ctx.canceled){
            // If the ray hits a collider, create a spring joint between the player and the hit rigidbody
            var hitResult = getGrappleConnectionPoint();
			if(!hitResult.HasValue) {
				grappleState = GrappleState.Released;
				Time.timeScale = 1.0f;
				return;
			}
			RaycastHit2D hit = hitResult.Value;
			Rigidbody2D hitRigidbody = hit.collider.attachedRigidbody;
			
			Vector2 playerPosition = transform.position;

			grapplingSpring.connectedBody = hitRigidbody;
			grapplingSpring.distance = Vector2.Distance(playerPosition, hit.point);
			grapplingSpring.connectedAnchor = hitRigidbody.transform.InverseTransformPoint(hit.point);

			Debug.Log(grapplingSpring.attachedRigidbody);
			
			grapplingSpring.enabled = true;
			grappleState = GrappleState.Launched;
			audioSource.PlayOneShot(grappleLaunchSound, AudioListener.volume);

            // Debug.Log("Grapple:"+grappleState + " Connected to:"+hit.collider);
            Time.timeScale = 1.0f;
        }
    }

    public void onGrapplePull(InputAction.CallbackContext ctx){
		if(!grappleAllowed) { return; }
        if(GameManager.instance.currGameState != GameManager.GameState.IN_GAME) {return;}
        // if (grappleState != GrappleState.Launched){ return; }
        if (grapplingSpring.enabled == false){ return; }

        if(ctx.performed){
            // Debug.Log("Pulling grapple");
            grapplingSpring.enabled = true;
            grappleState = GrappleState.Pulled;
            audioSource.PlayOneShot(grapplePullSound, AudioListener.volume);
        }
        if (ctx.canceled){
            releaseGrapple();
        }
    }

    public void pullGrapple(){ 
        if(GameManager.instance.currGameState != GameManager.GameState.IN_GAME) {return;}
        grapplingSpring.distance -= grapplePullForce * Time.deltaTime;
    }

    public void onGrappleRelease(InputAction.CallbackContext ctx){
        if(GameManager.instance.currGameState != GameManager.GameState.IN_GAME) {return;}
		if(!grappleAllowed) { return; }
        // if (grappleState == GrappleState.Released){ return; }

        if(ctx.started){
            releaseGrapple();
        }
    }

    public void releaseGrapple(){
		if(!grappleAllowed) { return; }
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
        if(other.CompareTag("Spikes")){
            TakeDamage(1);
        }
    }

    

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Spikes")){
            TakeDamage(1);
        }
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
        if(other.CompareTag("WindZone")){
            windForce = other.gameObject.GetComponent<Wind>().windForce;
        }
        if(other.CompareTag("Key")){
            GameManager.instance.keyFound(other.gameObject.GetComponent<SpriteRenderer>().color);
            // Debug.Log("Found key. Current key number: " + keysFound);
            other.gameObject.GetComponent<SpriteRenderer>().color = GameManager.disabledKeyColor;
            other.enabled = false;
            audioSource.PlayOneShot(keySound, AudioListener.volume);
            checkPoint = other.gameObject;
        }
        if(other.CompareTag("Heart")){
            if (GameManager.instance.lives < GameManager.instance.maxLives){
                GameManager.instance.lives += 1;
                other.gameObject.SetActive(false);
                audioSource.PlayOneShot(lifeSound, AudioListener.volume);
            }
        }
        if (other.CompareTag("Finish")){
            // NOTE: The rest of the finish interaction is in Finish.cs
            if (GameManager.instance.keysFound == GameManager.instance.keyCount){
                audioSource.PlayOneShot(finishSound, AudioListener.volume);
            }
        }
    }

    public void KilledEnemy(){
        GameManager.instance.enemiesKilled += 1;
    }

    public void TakeDamage(int damage){
        if(timeSinceLastHurt < invincibilityTimer){
            return;
            Debug.Log("Invincible");
        }
        Debug.Log("Took Damage");
        timeSinceLastHurt = 0f;
        animator.SetTrigger("Hurt");
        GameManager.instance.lives -= damage;
        audioSource.PlayOneShot(hurtSound, AudioListener.volume);
        releaseGrapple();
        transform.position = checkPoint.transform.position;
        rigidBody.velocity = new Vector2(0,0);
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
        if(other.CompareTag("WindZone")){
            windForce = Vector2.zero;
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
        if (grappleState == GrappleState.Pulled){
            pullGrapple();
        }
        rigidBody.AddForce(windForce, ForceMode2D.Force);
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
            Vector2 anchorPosition = grapplingSpring.connectedBody.transform.TransformPoint(grapplingSpring.connectedAnchor);
            
            Debug.DrawLine(transform.position, anchorPosition, Color.red);
            
            Vector2 anchorToPlayer = (Vector2)transform.position - anchorPosition;
            Vector2 dist = anchorToPlayer.normalized * grapplingSpring.distance;
            // Perpendicular to anchorToPlayer
            Vector2 bar = new Vector2(dist.y, -dist.x);
            bar *= 0.1f;
            Debug.DrawLine(anchorPosition+dist + bar, anchorPosition+dist - bar, Color.red);
        }
    }
}
