using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject PlayerTarget;
    public Vector3 offset;
    public float smoothTime = 0.25f;

    private Rigidbody2D player_rb;
    private Vector3 currentVelocity;

    void Start(){
        player_rb = PlayerTarget.GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(
                transform.position,
                PlayerTarget.transform.position + offset,
                ref currentVelocity,
                smoothTime
                );
    }
}
