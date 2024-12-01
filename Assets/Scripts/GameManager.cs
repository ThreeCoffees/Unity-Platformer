using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { 
        [InspectorName("In Game")] IN_GAME, 
        [InspectorName("Paused")] PAUSED, 
        [InspectorName("Level Completed")] LEVEL_COMPLETED, 
        [InspectorName("Game Over")] GAME_OVER, 
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
    [SerializeField] private GameObject highScoreText;
    [SerializeField] private GameObject scoreText;

    private PlayerInput playerInput;

    Scene currScene;
    int highScore;

    private int _score = 0;
    public int score {
        get {
            return _score;
        }
        set {
            _score = value;
            Debug.Log("Score: " + _score);
        }
    }

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
        gameOverScreen.SetActive(false);
        levelFinishedScreen.SetActive(false);
    }

    public void InGame(){
        SetGameState(GameState.IN_GAME);
        pauseScreen.SetActive(false);
        playerInput.SwitchCurrentActionMap("InGame");
        gameOverScreen.SetActive(false);
        levelFinishedScreen.SetActive(false);
    }

    public void LevelCompleted(){
        SetGameState(GameState.LEVEL_COMPLETED);
        playerInput.SwitchCurrentActionMap("UI");
        pauseScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        levelFinishedScreen.SetActive(true);

        highScore = PlayerPrefs.GetInt(currScene.name + "_HighScore");
        if(highScore < score){
            highScore = score;
            PlayerPrefs.SetInt(currScene.name + "_HighScore", highScore);
        }

        highScoreText.GetComponent<TMP_Text>().text = "High Score: " + highScore;
        scoreText.GetComponent<TMP_Text>().text = "Score: " + score;
    }

    public void GameOver(){
        SetGameState(GameState.GAME_OVER);
        playerInput.SwitchCurrentActionMap("UI");
        pauseScreen.SetActive(false);
        gameOverScreen.SetActive(true);
        levelFinishedScreen.SetActive(false);
    }

    public void Settings(){
        SetGameState(GameState.SETTINGS);
        playerInput.SwitchCurrentActionMap("UI");
        pauseScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        levelFinishedScreen.SetActive(false);
    }

    void Awake(){
        if(instance == null){
            instance = this;
        } else {
            Debug.Log("Duplicate Game Manager", gameObject);
        }

        playerInput = GetComponent<PlayerInput>();
        SetGameState(GameState.IN_GAME);

        currScene = SceneManager.GetActiveScene();

        if(!PlayerPrefs.HasKey(currScene.name + "_HighScore")){
            PlayerPrefs.SetInt(currScene.name + "_HighScore", 0);
        }
    }
}
