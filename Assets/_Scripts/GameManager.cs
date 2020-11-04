using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Globalization;
using Unity.Collections;
// ReSharper disable InconsistentNaming
// ReSharper disable Unity.PerformanceCriticalCodeNullComparison
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
// ReSharper disable CheckNamespace

/*
 * Author: Garrison Henkle
 * Date Created:  Sept 7, 2020
 * Date Modified: October 28, 2020
 * 
 * Controls game resources such as score, lives, and time and syncs these values with the UI in the game scenes.
 */

public class GameManager : MonoBehaviour
{
    #region GM Singleton
    private static GameManager gm;

    private void Awake()
    {
        if (gm != null)
            print("There is more than one instance of the GameManager!");
        else
        {
            gm = this;
            DontDestroyOnLoad(gm);
        }

    } //end Awake
    #endregion

    #region variables
    //types
    public enum GameState { Playing, Death, GameOver, Beatlevel }

    //game variables
    public GameObject player;                   //the player

    [HideInInspector]
    public GameState state = GameState.Playing; //the current gamestate
    [HideInInspector]
    public bool isStarted;                      //is the game started
    [HideInInspector]
    public bool isReplay;                       //is the game being replayed
    [HideInInspector]
    public bool isOver;                         //is the game over
    [HideInInspector]
    public bool isDead;                         //is the player dead

    [Space(10)]

    [Header("Game Variables")]
    [ReadOnly]
    public int health = 100;                    //current player health
    [HideInInspector]
    public int score;                           //current player score
    [HideInInspector]
    public bool hasWon;                         //has the user won (ie score == winningScore)
    public int winningScore;                    //score required for the user to win

    [HideInInspector]
    public int lives;                           //current number of player lives

    public bool timed;                          //is the level timed
    public static float timer = 3f;             //level timer in minutes

    [Space(10)]

    //defaults
    [Header ("Defaults")]
    // ReSharper disable once RedundantDefaultMemberInitializer
    public int defaultScore = 0;                //default score
    public int defaultLives = 3;                //default lives
    public float defaultTimer = 3f;             //default level timer

    public int defaultWinningScore = 100;       //default winning score

    [Space(10)]

    //levels
    /* Old Level System (OLS from now on)
    [Header("Levels")]
    public string firstLevel;   //first level
    public string nextLevel;    //next level
    [HideInInspector]
    public string storedNextLevel; //stores the next level so that the game can be reset
    [HideInInspector]
    public string levelToLoad;  //level that will be loaded next
    [HideInInspector]
    public string curLevel;     //level that is currently loaded
    */

    /* New System based on Mission Demolition (NLS from now on)*/
    public string[] levels;
    [Space(10)]
    private int levelMax;
    private int levelToLoad;
    private int level;
    
    //audio
    [HideInInspector]
    public AudioSource audioSource;   //plays audio 
    [Header ("Audio")]
    [Space(10)]
    public AudioClip backgroundMusic; //music that plays in the background
    public AudioClip gameOverAudio;   //clip that plays when the player loses
    public AudioClip levelOverAudio;  //clip that plays when the level is completed
    [HideInInspector]
    public bool isMusicOver;          //is the background music over

    [Space(10)]

    //canvases
    [Header ("Canvases")]
    public GameObject menuCanvas;       //contains the main menu
    public GameObject hudCanvas;        //contains the in-game UI
    public GameObject endScreenCanvas;  //contains the game over screen
    public GameObject footerCanvas;     //contains the footer (copyright + credits)

    [Space(10)]

    //main menu
    [Header("Main Menu")]
    public TMP_Text titleTextBox;       //game title
    public string gameTitle = "Game Title";

    public TMP_Text controlsTextBox;    //game controls
    public GameObject controlsOverlay;  //image that covers part of the menu to show controls
    public string controls = "\nMovement - W A S D\nJump - Space\nSprint - Shift\nTry to collect all the coins.\nGet to 100 points to win!";
    private const string defaultControls = "Movement - A, D\nJump - Space\nSprint - Shift\nSwitch Gun - 1, 2, 3 (if unlocked)\nReload - R\n\nMake your way through the village\nenacting revenge on your enemies!";

    [Space(10)]

    //footer
    [Header("Footer")]
    public TMP_Text creditsTextBox;     //game credits                    
    public string credits = "These are some credits:\nMade by Garrison Henkle";
    private const string defaultCredits = "Made by Garrison Henkle\n\nUsed Assets(opengameart.org / unity store) and Audio(freesounds.org):\nCayden Franklin - pixel art 2d weapons - Ak-47 no stock\ngamedevshirious - shotgun\ngamedevshirious - pixel pistol\nbagzie - bat sprite\nGrahhhhh - animated blue ring explosion\nMarco L. - pixel lightning\nChaotic Cody - dollar sign solid\noglsdl - bullet symbol";

    public TMP_Text copyrightTextBox;   //game copyright
    public string copyright = $"Copyright {System.DateTime.Now.Year} Garrison Henkle";

    [Space(10)]

    //HUD
    [Header("HUD")]
    public TMP_Text scoreText;   //score title
    public TMP_Text livesText;   //lives title
    public TMP_Text healthText;  //health title
    public TMP_Text timerText;   //timer title

    public TMP_Text scoreValue;  //score display
    public TMP_Text livesValue;  //lives display
    public TMP_Text healthValue; //health title
    public TMP_Text timerValue;  //timer display

    [Space(10)]

    [Header ("HUD Text")]
    public string defaultScoreText = "Score";           //default score title
    public string defaultLivesText = "Lives";           //default lives title
    public string defaultHealthText = "Health";         //default health title
    public string defaultTimerText = "Time Remaining";  //default timer title

    [Space(10)]

    //game over
    [Header("Game Over")]
    public TMP_Text gameOverText;       //game over
    public string gameOverMessage = "Game Over";

    public TMP_Text messageTextBox;     //game message
    public string gameMessage = "Try not to die next time around.";
    public string gameWinningMessage = "Wow good job!";

    [Space(10)]

    //ammo
    [Header("Ammo Text")]
    public TMP_Text gun1;
    public TMP_Text gun2;
    public TMP_Text gun3;
    public string gun1Text = "Pistol:";
    public string gun2Text = "Automatic:";
    public string gun3Text = "Shotgun";

    public TMP_Text ammo1;
    public TMP_Text ammo2;
    public TMP_Text ammo3;
    public string ammo1Text = "8 / 24";
    public string ammo2Text = "30 / 120";
    public string ammo3Text = "2 / 12";

    [Space(10)]

    //powerups
    [Header("Powerup Text")]
    public TMP_Text reloading;
    public TMP_Text doubleDamage;
    public TMP_Text freeAmmo;
    #endregion

    void Reset() {
        player = null;

        state = GameState.Playing;
        isStarted = false;
        isReplay = false;
        isOver = false;
        isDead = false;

        score = defaultScore;
        hasWon = false;
        winningScore = defaultWinningScore;

        lives = defaultLives;

        timed = false;
        timer = defaultTimer;

        /* OLS
        firstLevel = null;
        nextLevel = null;
        storedNextLevel = null;
        levelToLoad = null;
        curLevel = null;
        */

        backgroundMusic = null;
        gameOverAudio = null;
        levelOverAudio = null;
        isMusicOver = false;

        menuCanvas = null;
        hudCanvas = null;
        endScreenCanvas = null;
        footerCanvas = null;

        titleTextBox = null;
        gameTitle = "Game Title";

        controlsTextBox = null;
        controlsOverlay = null;
        controls = "\nMovement - W A S D\nJump - Space\nSprint - Shift\nDelete Ball - E\n Lift Object - Right Mouse\nGet the ball to the red goal to activate\nthe portal to the next level!";

        creditsTextBox = null;
        credits = "Created by Garrison Henkle";

        copyrightTextBox = null;
        copyright = $"Copyright {System.DateTime.Now.Year} Garrison Henkle";

        scoreText = null;
        livesText = null;
        timerText = null;

        scoreValue = null;
        livesValue = null;
        timerValue = null;

        defaultScoreText = "Score";         
        defaultLivesText = "Lives";         
        defaultTimerText = "Time Remaining";

        gameOverText = null;
        gameOverMessage = "Game Over";

        messageTextBox = null;
        gameMessage = "Try not to die next time around.";
        gameWinningMessage = "Wow good job!";

    } //Reset

    //hide all canvases
    void HideMenu() {
        menuCanvas.SetActive(false);
        hudCanvas.SetActive(false);
        endScreenCanvas.SetActive(false);
        footerCanvas.SetActive(false);
    } //HideMenu

    //hide all canvases and launch the game
    void Start() {
        HideMenu();

        audioSource = GetComponent<AudioSource>();
        //levelToLoad = firstLevel;    OLS
        
        //NLS
        level = 0;
        levelToLoad = 0;
        levelMax = levels.Length;

        MainMenu();
    } //Start

    //reset player variables and load the menu
    void MainMenu() {
        score = defaultScore;
        lives = defaultLives;

        if (titleTextBox != null) titleTextBox.text = gameTitle ?? "";
        if (creditsTextBox != null) creditsTextBox.text = "Credits";
        if (copyrightTextBox != null) copyrightTextBox.text = copyright ?? "";

        if (menuCanvas != null) menuCanvas.SetActive(true);
        if (footerCanvas != null) footerCanvas.SetActive(true);
    } //MainMenu

    //quit the game
    public void Quit() {
        Application.Quit();
    } //Quit

    //hide canvases, play the game
    public void PlayGame() {
        HideMenu();
        if (hudCanvas != null) hudCanvas.SetActive(true);

        //timing
        if (timed) {
            if (timerText != null) timerText.text = defaultTimerText;
            timer = defaultTimer;
        } else {
            if (timerText != null) timerText.text = "";
            if (timerValue != null) timerValue.text = "";
        }

        //score
        if (scoreText != null) scoreText.text = defaultScoreText;

        //lives
        if (livesText != null) livesText.text = defaultLivesText;

        //game state
        isStarted = true;
        state = GameState.Playing;
        isDead = false;

        //load level (OLS)
        //SceneManager.LoadScene(levelToLoad, LoadSceneMode.Additive);
        //curLevel = levelToLoad;

        //load level (NLS)
        SceneManager.LoadScene(levels[levelToLoad], LoadSceneMode.Additive);
        level = levelToLoad;
    } //PlayGame

    //manage state
    void Update()
    {
        //escape quits the game
        if (Input.GetKeyDown(KeyCode.Escape)) Quit();

        //programmer cheats
        if (Input.GetKeyDown(KeyCode.F10)) isDead = true;
        else if (Input.GetKeyDown(KeyCode.F11)) state = GameState.Beatlevel;
        else if (Input.GetKeyDown(KeyCode.F12)) state = GameState.GameOver;
        else if (Input.GetKeyDown(KeyCode.F9)) {
            print($"state: {state}");
            print($"level: {level}");
            print($"leveltoload: {levelToLoad}");
        }

        //play audio
        if (!audioSource.isPlaying) {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        //update display
        if (scoreValue != null)  scoreValue.text  = score.ToString();
        if (livesValue != null)  livesValue.text  = lives.ToString();
        if (healthValue != null) healthValue.text = health.ToString();
        if (gun1 != null)  gun1.text = gun1Text;
        if (gun2 != null)  gun2.text = gun2Text;
        if (gun3 != null)  gun3.text = gun3Text;
        if (ammo1 != null) ammo1.text = ammo1Text;
        if (ammo2 != null) ammo2.text = ammo2Text;
        if (ammo3 != null) ammo3.text = ammo3Text;
        
        switch (state)
        {
            case GameState.Playing:
                Playing();
                break;
            case GameState.Death:
                Dead();
                break;
            case GameState.GameOver:
                GameOver();
                break;
            case GameState.Beatlevel:
                BeatLevel();
                break;
        }

    } //Update

    //reload the level
    void ResetLevel() {
        //revive the player
        isDead = false; //redundant because it is set in the PlayGame method

        //reload the level
        //SceneManager.UnloadSceneAsync(curLevel);   //OLS
        SceneManager.UnloadSceneAsync(levels[level]);        //NLS
        PlayGame();
    } //ResetLevel

    //load the next level
    void StartNextLevel() {
        //reset game variables
        isMusicOver = false;
        lives = defaultLives;
        
        /* OLS
        levelToLoad = nextLevel;
        storedNextLevel = nextLevel;
        nextLevel = null;
        */
        
        levelToLoad = level + 1;                      //NLS

        //unload current level
        //SceneManager.UnloadSceneAsync(curLevel);    //OLS
        SceneManager.UnloadSceneAsync(levels[level]); //NLS

        //play the next level
        PlayGame();
    }

    //resets all values and plays the first level
    public void RestartGame(){
        //reset game variables
        score = defaultScore;
        lives = defaultLives;

        /* OLS
        levelToLoad = firstLevel;
        nextLevel = storedNextLevel;
        
        //unload current level
        SceneManager.UnloadSceneAsync(curLevel);
        */

        /* NLS */
        SceneManager.UnloadSceneAsync(levels[level]);
        levelToLoad = 0;
        level = 0;
        
        //play the first level
        PlayGame();

    } //RestartGame

    //control lives, level progression, and timer
    void Playing() {
        //respawn if player has lives, otherwise state is death
        if (isDead) {
            if (lives > 0) {
                print($"Lives: {lives}");
                lives -= 1;
                ResetLevel();
            }
            else
                state = GameState.Death;
        }

        //progress if the player has won
        if (hasWon || score >= winningScore)
            state = GameState.Beatlevel;

        //update timer and check timer state
        if (timed){
            if (timer < 0)
                state = GameState.GameOver;
            else{
                timer -= Time.deltaTime;
                if (timerValue != null) timerValue.text = timer.ToString(CultureInfo.CurrentCulture);
            }
        }
    } //Playing

    //fade out background music, then play the game over sfx and end the game
    void Dead() {

        //hide status displays
        ShowDoubleDamage(false);
        ShowFreeAmmo(false);
        ShowReload(false);

        

        if (lives > 0)
        {
            lives -= 1;

            //play game over audio
            if (gameOverAudio != null)
            {
                Debug.Log("playing");
                audioSource.clip = gameOverAudio;
                audioSource.volume = 1f;
                audioSource.loop = false;
                audioSource.Play();
            }

            //restart the level
            ResetLevel();
        }
        else
            state = GameState.GameOver;

    } //Dead

    //disable game objects and show the game over menu
    void GameOver() {
        //hide status displays
        ShowDoubleDamage(false);
        ShowFreeAmmo(false);
        ShowReload(false);

        //disable player and menus
        player = GameObject.Find("Player");
        if(player != null) player.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        HideMenu();

        

        //show game over screen
        if (endScreenCanvas != null) endScreenCanvas.SetActive(true);
    } //GameOver

    //fade out background music, then play the beat level sfx and progress to the next level (or end the game if the game is completed)
    void BeatLevel() {
        //hide status displays
        ShowDoubleDamage(false);
        ShowFreeAmmo(false);
        ShowReload(false);

        if (backgroundMusic != null && isMusicOver == false)
        {
            //play beat level audio
            if (levelOverAudio != null)
            {
                audioSource.clip = levelOverAudio;
                audioSource.volume = 1f;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        //change level
        //if (nextLevel != null)    //OLS
        if (level + 1 < levelMax)  //NLS
            StartNextLevel();
        else
        {
            if (gameOverText != null) gameOverText.text = gameOverMessage;
            if (messageTextBox != null) messageTextBox.text = gameWinningMessage;
            state = GameState.GameOver;
        }

    } //BeatLevel

    private void ToggleOverlay(string field, string defaults)
    {
        if(controlsOverlay != null)
        {
            var active = !controlsOverlay.activeSelf;
            controlsOverlay.SetActive(active);
        }
        
        if (controlsTextBox != null)
        {
            var activeText = !controlsTextBox.gameObject.activeSelf;
            controlsTextBox.text = field == "default" ? defaults : field;
            controlsTextBox.gameObject.SetActive(activeText);
        }
    } //ToggleOverlay
    
    public void ToggleControls() {
        ToggleOverlay(controls, defaultControls);
    } //ToggleControls
    
    public void ToggleCredits()
    {
        ToggleOverlay(credits, defaultCredits);
    } //ToggleCredits

    public void Score(int scored)
    {
        score += scored;
    } //Score

    /*Shooter Game*/

    public void ShowGun1(bool active)
    {
        gun1.gameObject.SetActive(active);
        ammo1.gameObject.SetActive(active);
    } //ShowGun1

    public void ShowGun2(bool active)
    {
        gun2.gameObject.SetActive(active);
        ammo2.gameObject.SetActive(active);
    } //ShowGun2

    public void ShowGun3(bool active)
    {
        gun3.gameObject.SetActive(active);
        ammo3.gameObject.SetActive(active);
    } //ShowGun3

    public void ShowDoubleDamage(bool active)
    {
        doubleDamage.gameObject.SetActive(active);
    } //ShowDoubleDamage

    public void ShowFreeAmmo(bool active)
    {
        freeAmmo.gameObject.SetActive(active);
    } //ShowFreeAmmo

    public void ShowReload(bool state)
    {
        reloading.gameObject.SetActive(state);
    }

} //GameManager
