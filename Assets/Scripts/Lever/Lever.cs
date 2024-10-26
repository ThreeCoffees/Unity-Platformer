using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
	[SerializeField] private bool isRight = false;
	[SerializeField] [Range(0, 10)] private float pullVelocityThreshold = 3f;
	[SerializeField] [Range(100, 2000)] private float pullAnimationSpeed = 500f;

	[SerializeField] private GameObject HandleAnchor;
	[SerializeField] private UnityEvent<bool> OnLeverPulled = new UnityEvent<bool>();
	
	private Rigidbody2D playerInBounds = null;
	private float currentHandleAngle = 0;

    private void Start()
    {
		if(isRight) currentHandleAngle = -90;
    }

    void Update()
    {
		// check if player pulls the lever
		if (playerInBounds != null) {
			bool prevIsRight = isRight;
			Vector2 playerVelLocal = transform.InverseTransformDirection(playerInBounds.velocity);
			if(Mathf.Abs(playerVelLocal.x) > pullVelocityThreshold) isRight = playerVelLocal.x > 0;
			if(prevIsRight != isRight) OnLeverPulled.Invoke(isRight);
		}

		// animate lever handle
		float targetAngle = isRight ? -90 : 0;
		
		float angleDiff = targetAngle - currentHandleAngle;
		float angleChange = Mathf.Sign(targetAngle - currentHandleAngle) * pullAnimationSpeed * Time.deltaTime;

		if (Mathf.Abs(angleDiff) < Mathf.Abs(angleChange)) currentHandleAngle = targetAngle;
		else currentHandleAngle += angleChange;

		currentHandleAngle = Mathf.Clamp(currentHandleAngle, -90, 0);

        HandleAnchor.transform.localRotation = Quaternion.Euler(0, 0, currentHandleAngle);
    }

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			playerInBounds = 
				other.gameObject
					.transform.parent.gameObject // needed because of colliders nesting in the player prefab
					.transform.parent.gameObject
					.GetComponent<Rigidbody2D>();
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			playerInBounds = null;
		}
	}
}
