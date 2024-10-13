using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement parameters")]

    [Range(0.01f, 20.0f)] [SerializeField] private float moveSpeed = 0.1f;
    [Range(0.01f, 1000.0f)] [SerializeField] private float jumpForce = 6f;
    // [Space(10)];

    [Range(0.5f, 2.0f)] [SerializeField]  private float rayLength = 2.0f;

    private Rigidbody2D rigidbody;
    public LayerMask groundLayer;


    // On component creation
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // Controls
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.Translate(moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.Translate(-moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
            Debug.Log("jumping");
        }

        
        Debug.DrawRay(transform.position, Vector3.down * rayLength, Color.white);

    }

    void Jump()
    {
        if(isGrounded())
        {
            rigidbody.AddForce(Vector2.up * jumpForce);
        }
    }

    bool isGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer.value);
    }
}
