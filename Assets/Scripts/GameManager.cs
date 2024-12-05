using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
    [SerializeField] private GameObject graphicsQualityText;

    [SerializeField] public Canvas inGameCanvas;
    [SerializeField] public TMP_Text inGameScoreText;

    [SerializeField] public Image[] keyIcons;

    private int _keysFound = 0;
    public static readonly Color disabledKeyColor = new Color(0.3f,0.3f,0.3f,0.7f);

    public void keyFound(Color keyColor){
        keyIcons[_keysFound].color = keyColor;
        _keysFound++;
    }

    public int keysFound {
        get {
            return _keysFound;
        }
    }

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
            inGameScoreText.GetComponent<TMP_Text>().text = "" + _score;
        }
    }

    public void LoadNewScene(string sceneName){
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void ExitGame(){
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void RestartScene(){
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    void SetGameState(GameState newGameState) {
        currGameState = newGameState;
        if (currGameState == GameState.IN_GAME) {
            inGameCanvas.enabled = true;
        } else {
            inGameCanvas.enabled = false;
        }
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

        scoreText.GetComponent<TMP_Text>().text = "" + score;

        playerInput = GetComponent<PlayerInput>();
        SetGameState(GameState.IN_GAME);

        currScene = SceneManager.GetActiveScene();

        if(!PlayerPrefs.HasKey(currScene.name + "_HighScore")){
            PlayerPrefs.SetInt(currScene.name + "_HighScore", 0);
        }

        foreach (Image keyIcon in keyIcons){
            keyIcon.color = Color.gray;
        }

        graphicsQualityText.GetComponent<TMP_Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }

    public void SetVolume(float vol){
        AudioListener.volume = vol;
        Debug.Log(AudioListener.volume);
    }

    public void DecreaseGraphics(){
        QualitySettings.DecreaseLevel();
        graphicsQualityText.GetComponent<TMP_Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }

    public void IncreaseGraphics(){
        QualitySettings.IncreaseLevel();
        graphicsQualityText.GetComponent<TMP_Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }
}
