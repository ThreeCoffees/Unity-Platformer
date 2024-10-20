using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject PlayerTarget;
    public Vector3 offset;
    public float positionSmooth = 0.25f;
    public float minZoom = 5.0f;
    public float maxZoom = 10.0f;
    public float maxVelocity = 15.0f;
    public float zoomSmooth = 0.25f;

    private Rigidbody2D player_rb;
    private Camera cam;
    private Vector3 currentVelocity;
    private float currentSize;

    void Start(){
        player_rb = PlayerTarget.GetComponent<Rigidbody2D>();
        cam = GetComponent<Camera>();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(
                transform.position,
                PlayerTarget.transform.position + offset,
                ref currentVelocity,
                positionSmooth
                );

        float vel = Mathf.Clamp(player_rb.velocity.magnitude / maxVelocity, 0, 1);
        float desiredSize = Mathf.Lerp(minZoom, maxZoom, vel);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, desiredSize, ref currentSize, zoomSmooth);
        
    }
}
