using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] private float moveRange = 3.0f;
    [Range(0.01f, 20.0f)] [SerializeField] private float moveSpeed = 3.0f;

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
        transform.localScale = !isFacingRight ? new Vector3(1,1,1) : new Vector3(-1,1,1);

        // if (isDead) { // Obsolete? See OnTriggerEnter2D()
        //     animator.SetBool("isDead", true);
        // }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Eagle-Dead")) {
            Destroy(gameObject);
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        startPositionX = transform.position.x;
    }

    void OnTriggerEnter2D(Collider2D other) { // NOTE: Copy of PlayerController's actions
        if (other.CompareTag("Player")) {
            if (other.transform.position.y > this.transform.position.y) {
                isDead = true;
                animator.SetBool("isDead", true);
                animator.SetTrigger("Hurt");
                Debug.Log("Enemy is dead");
            }
        }
    }
}
