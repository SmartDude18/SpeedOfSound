using System;
using System.Collections;
using System.Text;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;




public enum GameState
{
    LOADTITLE,
    TITLE,
    LOADMAINMENU,
    MAINMENU,
    LOADLEVEL,
    STARTGAME,
    PLAY,
    LOADLEVELCOMPLETESCREEN,
    LEVELCOMPLETESCREEN,
    RESTARTLEVEL
}



public class GameManager : MonoBehaviour
{
    [HideInInspector] public GameState gameState = GameState.LOADMAINMENU;
    //public static GameManager Instance { get; private set; }
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }


    private bool sceneLoadingComplete = false;
    private bool levelUnloadingComplete = false;
    private bool levelLoadingComplete = false;

    private bool loadingMenu = false;
    private bool loadingGameLevel = false;
    private bool startingGame = false;
    private bool loadingFinalScreen = false;
    private bool restartingLevel = false;

    private bool finalSceneLoaded = false;
    private bool menuSceneLoaded = false;
    private bool gameSceneLoaded = false;

    private int currentLevel = -1; //This MUST Start At -1!!!!!!!! (for the first scene to load correctly)
    private int maxLevel;

    private float timer = 10.5f;
    private float currentSpeed = 98;
    private float topSpeed = 112;

    private AudioSource currentAudio = null;

    private Coroutine flashingMessageCoroutine = null;

    [SerializeField] private GameState startingState = GameState.LOADTITLE;

    [SerializeField] private string menuScreenSceneName;
    [SerializeField] private string finalScreenSceneName;
    //[SerializeField] private string gameSceneName;

    [Tooltip("please make level names and scene names align!!! (this is case sensitive)")]
    [SerializeField] private string[] LevelNames; //please make level names and scene names align!!! (this is case sensitive)


    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject CountDownPanel;
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private TMPro.TMP_Text timerText;
    [SerializeField] private TMPro.TMP_Text speedText;
    [SerializeField] private TMPro.TMP_Text countDownText;
    [SerializeField] private TMPro.TMP_Text soundBarrierText;

    [SerializeField] private AudioSource MenuMusic;
    [SerializeField] private AudioSource ScoreScreenMusic;

    [Tooltip("Audio Sources must be placed at same index as corresponding level")]
    [SerializeField] private AudioSource[] levelAudio;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        maxLevel = LevelNames.Length;
    }

    private void Start()
    {
        gameState = startingState;
    }


    private void Update()
    {
        switch (gameState)
        {
            case GameState.LOADTITLE:
                break;
            case GameState.TITLE:
                break;
            case GameState.LOADMAINMENU:
                if (!loadingMenu)
                {
                    StartCoroutine(LoadMenuScene());
                    gameState = GameState.MAINMENU;
                    loadingMenu = true;
                }
                break;
            case GameState.MAINMENU:
                break;
            case GameState.LOADLEVEL:
                if(!loadingGameLevel)
                {
                    StartCoroutine(LoadLevel());
                    loadingGameLevel = true;
                }
                break;
            case GameState.STARTGAME:
                if(!startingGame)
                {
                    StartCoroutine(StartGame());
                    startingGame = true;
                }
                break;
            case GameState.PLAY:
                PlayGameUpdate();
                break;
            case GameState.LOADLEVELCOMPLETESCREEN:
                if(!loadingFinalScreen)
                {
                    StartCoroutine(LoadFinalScene());
                    loadingFinalScreen = true;
                }
                break;
            case GameState.LEVELCOMPLETESCREEN:
                break;
            case GameState.RESTARTLEVEL:
                if(!restartingLevel)
                {
                    StartCoroutine(RestartLevel());
                    restartingLevel = true;
                }
                break;
            default:
                break;
        }
    }


    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetMaxLevel()
    {
        return maxLevel;
    }

    public float getTimer()
    {
        return timer;
    }

    public float getTopSpeed()
    {
        return topSpeed;
    }

    public void ReportCurrentSpeed(float speed)
    {
        currentSpeed = speed;
        if(topSpeed < currentSpeed)
        {
            topSpeed = currentSpeed;
        }

        //Breaking the sound barrier
        if(currentSpeed >= 343)
        {
            currentAudio.mute = true;
            //soundBarrierText.gameObject.SetActive(true);
            if(flashingMessageCoroutine == null)
            {
                flashingMessageCoroutine = StartCoroutine(FlashSoundBarrierMessage());
            }
            
        }
        else
        {
            currentAudio.mute = false;
            soundBarrierText.gameObject.SetActive(false);

            if(flashingMessageCoroutine != null)
            {
                StopCoroutine(flashingMessageCoroutine);
                flashingMessageCoroutine = null;
            }
        }
    }



    #region Load and Unload Scenes

    /// <summary>
    /// Loads a scene asynchronously
    /// </summary>
    /// <param name="sceneName">The name of the scene being loaded (MUST be the exact name of the scene (case insensitve) or the file path name if two scenes of the same name exist)</param>
    /// <returns>Yield return for Coroutine</returns>
    private IEnumerator LoadScene(string sceneName)
    {
        //Only loads scene if scene is not already loaded
        if (!SceneManager.GetSceneByName(sceneName).IsValid())
        {
            //Loads specified scene Async (for better performance) and additive so that the base scene remains present
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
    }

    /// <summary>
    /// Unloads a scene asynchronously
    /// </summary>
    /// <param name="sceneName">The name of the scene being loaded (MUST be the exact name of the scene (case insensitve) or the file path name if two scenes of the same name exist)</param>
    /// <returns>Yield return for Coroutine</returns>
    private IEnumerator UnloadScene(string sceneName)
    {
        //Only unloads scene if scene is already loaded
        if (SceneManager.GetSceneByName(sceneName).IsValid())
        {
            //Unloads specified scene Async (for better performance)
            yield return SceneManager.UnloadSceneAsync(sceneName);
        }
    }

    #endregion Load and Unload Scenes



    #region Other Coroutines
    private IEnumerator CountDown()
    {
        for(float i = 3; i > 0; i -= Time.unscaledDeltaTime)
        {
            countDownText.text = (((int)i) + 1).ToString();

            yield return i;

            //sbSpeed.Append(((int)(GameManager.Instance.getTopSpeed())).ToString());

        }
    }

    public IEnumerator FlashSoundBarrierMessage()
    {
        while(true)
        {
            soundBarrierText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            soundBarrierText.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }
    }


    public IEnumerator UnpauseGame()
    {
        PausePanel.SetActive(false);

        CountDownPanel.SetActive(true);
        yield return StartCoroutine(CountDown());
        CountDownPanel.SetActive(false);

        //if(currentAudio != null && !currentAudio.isPlaying)
        if(currentAudio != null)
        {
            currentAudio.Play();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1.0f;
    }

    #endregion Other Coroutines

    #region Other Functions
    public void PauseGameTime()
    {
        Time.timeScale = 0.0f;
    }

    public void PauseGame()
    {
        if(currentAudio != null)
        {
            currentAudio.Pause();
        }

        PauseGameTime();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PausePanel.SetActive(true);
    }


    private void ChangeMusic(AudioSource music, bool startMusic)
    {
        if(currentAudio != null) currentAudio.Stop();
        currentAudio = music;
        if(currentAudio != null && startMusic) currentAudio.Play();
    }

    #endregion Other Functions


    #region GameStateUpdates

    /// <summary>
    /// Loads final screen scene and unloads all other non-base scenes
    /// </summary>
    private IEnumerator LoadFinalScene()
    {
        HUD.SetActive(false);
        PausePanel.SetActive(false);

        if (!sceneLoadingComplete)
        {
            if (menuSceneLoaded)
            {
                yield return StartCoroutine(UnloadScene(menuScreenSceneName));
                menuSceneLoaded = false;
            }

            if (gameSceneLoaded)
            {
                StartCoroutine(UnloadScene(LevelNames[currentLevel]));
                gameSceneLoaded = false;
            }
            
            if (!finalSceneLoaded)
            {
               yield return StartCoroutine(LoadScene(finalScreenSceneName));
                finalSceneLoaded = true;
            }


            sceneLoadingComplete = true;
        }

        ChangeMusic(ScoreScreenMusic, true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        sceneLoadingComplete = false;
        loadingFinalScreen = false;

        gameState = GameState.LEVELCOMPLETESCREEN;
    }

    /// <summary>
    /// Loads main menu screen scene and unloads all other non-base scenes
    /// </summary>
    private IEnumerator LoadMenuScene()
    {
        HUD.SetActive(false);
        PausePanel.SetActive(false);

        if (!sceneLoadingComplete)
        {
           if (finalSceneLoaded)
           {
               yield return StartCoroutine(UnloadScene(finalScreenSceneName));
               finalSceneLoaded = false;
           }

            if (gameSceneLoaded)
            {
                StartCoroutine(UnloadScene(LevelNames[currentLevel]));
                gameSceneLoaded = false;
            }

            currentLevel = -1;


            if (!menuSceneLoaded)
            {
                yield return StartCoroutine(LoadScene(menuScreenSceneName));
                menuSceneLoaded = true;
            }

            ChangeMusic(MenuMusic, true);

            sceneLoadingComplete = true;
        }



        sceneLoadingComplete = false;
        loadingMenu = false;
    }

    /// <summary>
    /// Loads game scene, unloads all other non-base scenes, and starts game
    /// </summary>
    private IEnumerator LoadLevel()
    {
        PausePanel.SetActive(false);

        if (!sceneLoadingComplete)
        {
            if (finalSceneLoaded)
            {
                yield return StartCoroutine(UnloadScene(finalScreenSceneName));
                finalSceneLoaded = false;
            }

            if (menuSceneLoaded)
            {
                yield return StartCoroutine(UnloadScene(menuScreenSceneName));
                menuSceneLoaded = false;
            }

            if(!levelUnloadingComplete)
            {
                if(currentLevel != -1)
                {
                    yield return StartCoroutine(UnloadScene(LevelNames[currentLevel]));

                }
                //print(currentLevel);

                currentLevel++;
                if (currentLevel >= maxLevel)
                {
                    gameState = GameState.LOADMAINMENU;
                    currentLevel = -1;
                    yield break;
                }

                levelUnloadingComplete = true;
                gameSceneLoaded = true;
            }

            if(!levelLoadingComplete)
            {
                //print(currentLevel);
                //print(LevelNames.Length);
                yield return StartCoroutine(LoadScene(LevelNames[currentLevel]));

                levelLoadingComplete = true;
                gameSceneLoaded = true;
            }

            //StartCoroutine(LoadScene(gameSceneName));
            gameSceneLoaded = true;

            sceneLoadingComplete = true;
        }

        //ChangeMusic(null);
        if(levelAudio.Length <= currentLevel)
        {
            ChangeMusic(null, false);
        }
        else
        {
            ChangeMusic(levelAudio[currentLevel], false);
        }

        sceneLoadingComplete = false;
        levelUnloadingComplete = false;
        levelLoadingComplete = false;

        loadingGameLevel = false;
        gameState = GameState.STARTGAME;
    }

    private IEnumerator StartGame()
    {
        //Logic for starting the game (ie a countdown)

        //So that it won't crash during early testing (remove this once we have the countdown)
        //yield return new WaitForSeconds(0.1f);

        //CountDownPanel.SetActive(true);
        //yield return StartCoroutine(CountDown());
        //CountDownPanel.SetActive(false);

        
        timerText.text = "Time: 000";
        speedText.text = "M/S: \n000";

        HUD.SetActive(false);

        PauseGameTime();

        currentSpeed = 0;
        topSpeed = 0;
        timer = 0;

        yield return StartCoroutine(UnpauseGame());


        HUD.SetActive(true);

        gameState = GameState.PLAY;
        startingGame = false;
        
    }

    /// <summary>
    /// Runs all necessary update game manager functions for core gameplay
    /// </summary>
    private void PlayGameUpdate()
    {
        //ANY GAME MANAGER CRITICAL LOGIC GOES HERE!!!

        timer += Time.deltaTime;

        StringBuilder sbTimer = new StringBuilder();
        sbTimer.Append("Time: ");
        sbTimer.Append(timer.ToString());
        //sbTimer.Append(((int)timer).ToString());
        timerText.text = sbTimer.ToString();

        StringBuilder sbSpeed = new StringBuilder();
        sbSpeed.Append("M/S: \n");
        sbSpeed.Append(currentSpeed.ToString());
        speedText.text = sbSpeed.ToString();


        if(Input.GetKeyDown(KeyCode.Escape))
        {
           PauseGame();
        }

        if(Input.GetKeyDown(KeyCode.N))
        {
            gameState = GameState.LOADLEVEL;
        }
    }



    private IEnumerator RestartLevel()
    {
        PausePanel.SetActive(false);

        if (!sceneLoadingComplete)
        {
            if (!levelUnloadingComplete)
            {
                if (currentLevel != -1)
                {
                    yield return StartCoroutine(UnloadScene(LevelNames[currentLevel]));

                }

                //currentLevel++;
                if (currentLevel >= maxLevel)
                {
                    gameState = GameState.LOADMAINMENU;
                    currentLevel = 0;
                    yield break;
                }

                levelUnloadingComplete = true;
                gameSceneLoaded = true;
            }

            if (!levelLoadingComplete)
            {
                yield return StartCoroutine(LoadScene(LevelNames[currentLevel]));

                levelLoadingComplete = true;
                gameSceneLoaded = true;
            }

            gameSceneLoaded = true;

            ChangeMusic(currentAudio, false);

            sceneLoadingComplete = true;
        }

        restartingLevel = false;
        sceneLoadingComplete = false;
        levelLoadingComplete = false;
        levelUnloadingComplete = false;
        gameState = GameState.STARTGAME;
    }

    #endregion GameStateUpdates

}
