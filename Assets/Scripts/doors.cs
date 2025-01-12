using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doors : MonoBehaviour 
{
	[SerializeField] public string scene;

	private void OnTriggerEnter2D(Collider2D other) {
		if(other.tag == "Player") {
			GameManager.instance.LoadNewScene(scene);
		}
	}
}
