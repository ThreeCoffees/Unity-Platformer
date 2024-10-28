using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] private float moveRange = 3.0f;
    [Range(0.01f, 20.0f)] [SerializeField] private float moveSpeed = 3.0f;

    private bool isMovingRight = true;
    private bool isFacingRight = true;
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
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        startPositionX = transform.position.x;
    }
}
