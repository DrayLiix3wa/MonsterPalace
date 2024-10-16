using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    public enum GameState
    {
        MAINMENU,
        STARTRUN,
        PAUSE,
        RUNEND
    }

    [Header("Game State")]
    public GameState currentState;

    public bool isMainMenu = false;
    public bool isPlay = false;
    public bool isPause = false;
    public bool isRunEnd = false;

    [Space(10)]

    [Header("Events")]
    public UnityEvent onStart = new UnityEvent();
    public UnityEvent onPlay = new UnityEvent();
    public UnityEvent onPauseEnter = new UnityEvent();
    public UnityEvent onPauseExit = new UnityEvent();
    public UnityEvent onRunEnd = new UnityEvent();


    [Space(10)]
    [Header("References")]
    [SerializeField] private SaveManager _saveManager;

    private void Awake()
    {
        if ( _instance == null )
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            TransitionToState(GameState.MAINMENU);
        }
        else if (scene.name == "Run")
        {

            if (currentState != GameState.STARTRUN)
            {
                TransitionToState(GameState.STARTRUN);
            }
        }
    }

    private void Start()
    {
        _saveManager = FindObjectOfType<SaveManager>();
    }

    private void Update()
    {
        OnStateUpdate();
    }

    private void OnStateEnter()
    {
        switch (currentState)
        {
            case GameState.MAINMENU:
                onStart.Invoke();
                isMainMenu = true;
                break;
            case GameState.STARTRUN:
                Time.timeScale = 1;
                onPlay.Invoke();
                isPlay = true;
                break;
            case GameState.PAUSE:
                Time.timeScale = 0;
                onPauseEnter.Invoke();
                isPause = true;
                break;
            case GameState.RUNEND:
                onRunEnd.Invoke();
                isRunEnd = true;
                break;
        }
    }

    private void OnStateUpdate()
    {
        switch (currentState)
        {
            case GameState.MAINMENU:
                if (isPlay)
                {
                    TransitionToState(GameState.STARTRUN);
                }
                else if (isPause)
                {
                    TransitionToState(GameState.PAUSE);
                }
                break;
            case GameState.STARTRUN:
                if (isMainMenu)
                {
                    TransitionToState(GameState.MAINMENU);
                }
                else if (isPause)
                {
                    TransitionToState(GameState.PAUSE);
                }
                else if (isRunEnd)
                {
                    TransitionToState(GameState.RUNEND);
                }
                break;
            case GameState.PAUSE:
                if (isMainMenu)
                {
                    TransitionToState(GameState.MAINMENU);
                }
                else if (isPlay)
                {
                    TransitionToState(GameState.STARTRUN);
                }
                else if (isRunEnd)
                {
                    TransitionToState(GameState.RUNEND);
                }
                break;
            case GameState.RUNEND:
                if (isMainMenu)
                {
                    TransitionToState(GameState.MAINMENU);
                }
                else if (isPlay)
                {
                    TransitionToState(GameState.STARTRUN);
                }
                else if (isPause)
                {
                    TransitionToState(GameState.PAUSE);
                }
                break;
        }
    }

    private void OnStateExit()
    {
        switch (currentState)
        {
            case GameState.MAINMENU:
                isMainMenu = false;
                break;
            case GameState.STARTRUN:
                isPlay = false;
                break;
            case GameState.PAUSE:
                onPauseExit.Invoke();
                Time.timeScale = 1;
                isPause = false;
                break;
            case GameState.RUNEND:
                isRunEnd = false;
                break;
        }
    }

    private void TransitionToState(GameState state)
    {
        OnStateExit();
        currentState = state;
        OnStateEnter();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartRun()
    {
        isPlay = true;
    }

    public void PauseGame()
    {
        isPause = true;
    }

    public void ResumeGame()
    {
        isPlay = true;
    }

    public void RunOver()
    {
        isRunEnd = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void StartNewRun()
    {
        SceneManager.LoadScene("Run");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}