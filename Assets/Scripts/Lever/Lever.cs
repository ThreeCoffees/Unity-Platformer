using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
	[SerializeField] private bool isEnabled = false;
	[SerializeField] private bool invert = false;
	[SerializeField] [Range(0, 10)] private float pullVelocityThreshold = 3f;
	[SerializeField] [Range(100, 2000)] private float pullAnimationSpeed = 500f;

	[SerializeField] private GameObject HandleAnchor;
	[SerializeField] private GameObject Bulb;
	[SerializeField] private UnityEvent<bool> OnLeverPulled = new UnityEvent<bool>();
	
	private Rigidbody2D playerInBounds = null;
	private Rigidbody2D parentRigidBody = null;
	private float currentHandleAngle = 0;

    private void Start()
    {
		parentRigidBody = GetComponentInParent<Rigidbody2D>();
		if(isEnabled ^ invert) currentHandleAngle = -90;
    }

    void Update()
    {
		// check if player pulls the lever
		if (playerInBounds != null) {
			bool prevIsEnabled = isEnabled;

			Vector2 leverVelocity = Vector2.zero;
			if(parentRigidBody != null) leverVelocity = parentRigidBody.velocity;
			Vector2 playerVelLocal = transform.InverseTransformDirection(playerInBounds.velocity);
			playerVelLocal -= leverVelocity;
			if(Mathf.Abs(playerVelLocal.x) > pullVelocityThreshold) isEnabled = (playerVelLocal.x > 0) ^ invert;
			
			if(prevIsEnabled != isEnabled) OnLeverPulled.Invoke(isEnabled);
		}

		// animate lever handle
		float targetAngle = isEnabled ^ invert ? -90 : 0;
		
		float angleDiff = targetAngle - currentHandleAngle;
		float angleChange = Mathf.Sign(targetAngle - currentHandleAngle) * pullAnimationSpeed * Time.deltaTime;

		if (Mathf.Abs(angleDiff) < Mathf.Abs(angleChange)) currentHandleAngle = targetAngle;
		else currentHandleAngle += angleChange;

		currentHandleAngle = Mathf.Clamp(currentHandleAngle, -90, 0);

        HandleAnchor.transform.localRotation = Quaternion.Euler(0, 0, currentHandleAngle);

		// show/hide bulb
		Bulb.SetActive(isEnabled);
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
			Vector2 playerLocal = transform.InverseTransformPoint(playerInBounds.transform.position);
			bool prevIsEnabled = isEnabled;
			isEnabled = playerLocal.x > -0.5 ^ invert;
			if(prevIsEnabled != isEnabled) OnLeverPulled.Invoke(isEnabled);

			playerInBounds = null;
		}
	}
}
