using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JumpPoint : MonoBehaviour
{
    public static Action OnJumpPointEnter;
    public static Action OnJumpPointExit;

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            OnJumpPointEnter?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            OnJumpPointExit?.Invoke();
        }
    }
}
