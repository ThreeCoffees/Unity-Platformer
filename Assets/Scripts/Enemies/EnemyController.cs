using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] private float moveRange = 3.0f;
    [Range(0.01f, 20.0f)] [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private int points = 3;
    [SerializeField] private int damage = 1;

    private bool isMovingRight = true;
    private bool isFacingRight = true;

    private bool isDead = false;
    private Animator animator;

    private float startPositionX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move
        if (isMovingRight) {
            // Move right up to moveRange
            if (transform.position.x < startPositionX + moveRange) {
                transform.Translate(moveSpeed * Vector2.right * Time.deltaTime, Space.World);
            } else {
                isMovingRight = false;
                isFacingRight = false;
            }
        } else {
            // Move left up to moveRange
            if (transform.position.x > startPositionX - moveRange) {
                transform.Translate(moveSpeed * Vector2.left * Time.deltaTime, Space.World);
            } else {
                isMovingRight = true;
                isFacingRight = true;
            }
        }

        // Flip
        // isFacingRight is negated - enemy sprites look leftwards
        //transform.localScale = !isFacingRight ? new Vector3(1,1,1) : new Vector3(-1,1,1);
        float newX = Mathf.Abs(transform.localScale.x) * (!isFacingRight ? 1 : -1); 
        transform.localScale = new Vector3(newX, transform.localScale.y, transform.localScale.z);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Eagle-Dead")) {
            Destroy(gameObject);
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        startPositionX = transform.position.x;
    }

    void OnTriggerEnter2D(Collider2D other) { 
        // Prevent a dead enemy from injuring the player while playing the death animation.
        if (isDead == true) {
            return;
        }

        if (other.CompareTag("Player")) {
            if (other.transform.position.y > this.transform.position.y) {
                Die();
                other.gameObject.GetComponentInParent<PlayerController>().KilledEnemy();
            } else {
                other.gameObject.GetComponentInParent<PlayerController>().TakeDamage(damage);
            }
        }
    }

    void Die(){
        isDead = true;
        animator.SetBool("isDead", true);
        animator.SetTrigger("Hurt");
        Debug.Log("Enemy is dead");
    }
}
