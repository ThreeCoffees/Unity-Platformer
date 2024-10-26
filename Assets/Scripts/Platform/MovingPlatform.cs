using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Range(0.1f, 30f)][SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool pingPong = false;
    [SerializeField] private bool paused = false;
    [SerializeField] Transform[] path;
    private int index = 0;
    private int direction = 1;
    [SerializeField]private const float distanceThreshold = 0.05f;

    private Rigidbody2D rb;
    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

	public void Activate(bool activeted) {
		paused = !activeted;
		Debug.Log("Platform activated: " + activeted);
	}

    void Start()
    {
        transform.position = path[index].transform.position;
    }

    void FixedUpdate()
    {
		NextPoint();
		MoveTowardsNextPoint();
    }

    void MoveTowardsNextPoint(){
		if(paused) rb.velocity = Vector2.zero;
		else       rb.velocity = (path[index].transform.position - transform.position).normalized * moveSpeed;
    }

    void NextPoint() {
        if(Vector2.Distance(transform.position, path[index].transform.position) >= distanceThreshold) return;

        index += direction;
        if(index >= path.Length || index < 0) {
            if(pingPong){
                direction *= -1;
                index += direction;
            } else {
                index = 0;
            }
        }
    }
}
