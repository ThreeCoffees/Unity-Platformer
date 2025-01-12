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
    [SerializeField] private GameObject bestTimeText;
    [SerializeField] private GameObject timeText;
    [SerializeField] private GameObject graphicsQualityText;

    [SerializeField] public Canvas inGameCanvas;
    [SerializeField] public TMP_Text inGameScoreText;

    private Image[] keyIcons;
    [Header("Keys")]
    [SerializeField] private GameObject keysIconsSpawner;
    [SerializeField] public int keyCount = 3;
    [SerializeField] private GameObject keyIcon;

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

    private Image[] livesIcons;
    [Header("Lives")]
	[SerializeField] private bool infiniteLives = false;
    [SerializeField] private GameObject livesIconsSpawner;
    [Range(1, 10)] [SerializeField] public int maxLives = 3;
    [SerializeField] private GameObject lifeIcon;
    
    enum LifeUIPolicy {
        GRAY_OUT, DISABLE
    }
    [SerializeField] private LifeUIPolicy lifeUIPolicy;


    public static readonly Color disabledLifeColor = new Color(0.3f,0.3f,0.3f,0.7f);

    private int _lives = 3;
    public int lives {
        get {
            return _lives;
        }
        set {
			if(infiniteLives) return;
			
            _lives = value;
            Debug.Log("Lives: " + _lives);
            
            if(_lives < 0){
                _lives = 0;
            }

            for(int i = 0; i < _lives; i++){
                if (lifeUIPolicy == LifeUIPolicy.GRAY_OUT) {
                    livesIcons[i].color = Color.white;
                } else if (lifeUIPolicy == LifeUIPolicy.DISABLE) {
                    livesIcons[i].enabled = true;
                }    
            }
            for(int i = _lives; i < livesIcons.Length; i++){
                if (lifeUIPolicy == LifeUIPolicy.GRAY_OUT) {
                    livesIcons[i].color = disabledLifeColor;
                } else if (lifeUIPolicy == LifeUIPolicy.DISABLE) {
                    livesIcons[i].enabled = false;
                }
            }

            if(_lives <= 0){
                // FIXME: respawn is implemented in PlayerController
                
                // transform.position = respawnPoint.transform.position; 
                // lives = maxLives;
                
				GameOver();
            }
        }
    }

    private float timer = 0;

    [SerializeField] private TMP_Text timerText;

    [SerializeField] private TMP_Text killedEnemiesText;

    private PlayerInput playerInput;

    Scene currScene;
    int highScore;
    float bestTime = Mathf.Infinity;

    private int _enemiesKilled = 0;
    public int enemiesKilled {
        get {
            return _enemiesKilled;
        }
        set {
            Debug.Log("Enemy killed");
            _enemiesKilled = value;
            killedEnemiesText.text = "" + _enemiesKilled;
        }
    }

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
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame(){
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void RestartScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SetGameState(GameState newGameState) {
        if(inGameCanvas == null) return;
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
        bestTime = PlayerPrefs.GetFloat(currScene.name + "_BestTime");
        if(bestTime > timer){
            bestTime = timer;
            PlayerPrefs.SetFloat(currScene.name + "_BestTime", bestTime);
        }

        highScoreText.GetComponent<TMP_Text>().text = "High Score: " + highScore;
        scoreText.GetComponent<TMP_Text>().text = "Score: " + score;
        bestTimeText.GetComponent<TMP_Text>().text = "Best Time: " + timerToText(bestTime);
        timeText.GetComponent<TMP_Text>().text = "Time: " + timerToText(timer);
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

        if(scoreText != null){
            scoreText.GetComponent<TMP_Text>().text = "" + score;
        }

        playerInput = GetComponent<PlayerInput>();
        SetGameState(GameState.IN_GAME);

        currScene = SceneManager.GetActiveScene();

        if(!PlayerPrefs.HasKey(currScene.name + "_HighScore")){
            PlayerPrefs.SetInt(currScene.name + "_HighScore", 0);
        }
        if(!PlayerPrefs.HasKey(currScene.name + "_BestTime")){
            PlayerPrefs.SetFloat(currScene.name + "_BestTime", Mathf.Infinity);
        }

        if(inGameCanvas != null){
            SetKeyCount();
            SetLivesCount();

            foreach (Image keyIcon in keyIcons){
                keyIcon.color = Color.gray;
            }
        }


        if (graphicsQualityText != null){
            graphicsQualityText.GetComponent<TMP_Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
        }
    }

    protected virtual void Update(){
        if(currGameState == GameState.IN_GAME){
            timer += Time.deltaTime;
            if(timerText != null) {
                timerText.text = timerToText(timer);
            }
        }
    }

    string timerToText(float timer){
        string text = string.Format("{0:00}:{1:00}.{2:00}", 
                Mathf.Floor(timer / 60), Mathf.Floor(timer % 60), Mathf.Floor((timer * 100) % 100));
        return text;
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

    private void SetKeyCount(){
        keyIcons = new Image[keyCount];
        for(int i = 0; i < keyCount; i++){
            GameObject key = Instantiate(keyIcon, keysIconsSpawner.transform);
            key.transform.SetParent(keysIconsSpawner.transform);
            keyIcons[i] = key.GetComponent<Image>();
        }
    }

    private void SetLivesCount(){
		if(infiniteLives) return;
        livesIcons = new Image[maxLives];
        for(int i = 0; i < maxLives; i++){
            GameObject life = Instantiate(lifeIcon, livesIconsSpawner.transform);
            life.transform.SetParent(livesIconsSpawner.transform);
            livesIcons[i] = life.GetComponent<Image>();
        }
        lives = maxLives;
        Debug.Log(lives);
    }
}
