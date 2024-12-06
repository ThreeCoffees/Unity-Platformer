using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")){
            GameManager gm = GameManager.instance;
            if(gm.keysFound == gm.keyIcons.Length){
                Debug.Log("Game Over");
                GameManager.instance.LevelCompleted();
            }
            else {
                Debug.Log("Not enough keys");
            }
            // NOTE: audio playback is in PlayerController.cs
        }
    }
}
