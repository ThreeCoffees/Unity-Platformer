using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [SerializeField] Transform OpenPos;
	[SerializeField] private bool activated = false;
	[SerializeField] private float moveSpeed = 1f;

	public void Activate(bool activeted) {
		this.activated = activeted;
	}

    void Start()
    {
        if(activated) transform.position = OpenPos.position;
    }

    void Update()
    {
        Vector3 targetPos = activated ? OpenPos.position : transform.parent.position;

		transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
}
