using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Range(0.1f, 30f)][SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool pingPong = false;
    [SerializeField] Transform[] path;
    private int index = 0;
    private int direction = 1;
    [SerializeField]private const float distanceThreshold = 0.05f;

    private Rigidbody2D rb;
    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.position = path[index].transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        NextPoint();
        MoveTowardsNextPoint();
    }

    void MoveTowardsNextPoint(){
        rb.velocity = (path[index].transform.position - transform.position).normalized * moveSpeed;
        Debug.Log(rb.velocity);
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

    void OnCollisionEnter2D(Collision2D other){
        //other.transform.SetParent(transform);
    }

    void OnCollisionExit2D(Collision2D other){
        //other.transform.SetParent(null);
    }
}
