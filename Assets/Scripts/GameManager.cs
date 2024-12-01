using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum GameState { 
        [InspectorName("In Game")] IN_GAME, 
        [InspectorName("Paused")] PAUSED, 
        [InspectorName("Level Completed")] LEVEL_COMPLETED, 
        [InspectorName("Settings")] SETTINGS, 
    };

    private GameState _currGameState;
    public GameState currGameState {
        get{
            return _currGameState;
        } 
        private set{
            _currGameState = value;
            updateTimeScale();
        }
    }

    public static GameManager instance;

    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject levelFinishedScreen;

    private PlayerInput playerInput;

    void SetGameState(GameState newGameState) {
        currGameState = newGameState;
    }

    void updateTimeScale(){
        if(currGameState == GameState.IN_GAME){
            Time.timeScale = 1;
        } else {
            Time.timeScale = 0;
        }
    }

    public void Pause(){
        SetGameState(GameState.PAUSED);
        pauseScreen.SetActive(true);
        playerInput.SwitchCurrentActionMap("UI");
        //gameOverScreen.SetActive(false);
        //levelFinishedScreen.SetActive(false);
    }

    public void InGame(){
        SetGameState(GameState.IN_GAME);
        pauseScreen.SetActive(false);
        playerInput.SwitchCurrentActionMap("InGame");
        //gameOverScreen.SetActive(false);
        //levelFinishedScreen.SetActive(false);
    }

    public void LevelCompleted(){
        SetGameState(GameState.LEVEL_COMPLETED);
    }

    public void GameOver(){
        SetGameState(GameState.LEVEL_COMPLETED);
    }

    public void Settings(){
        SetGameState(GameState.SETTINGS);
    }

    void Awake(){
        if(instance == null){
            instance = this;
        } else {
            Debug.Log("Duplicate Game Manager", gameObject);
        }

        playerInput = GetComponent<PlayerInput>();
        SetGameState(GameState.IN_GAME);
    }
}
