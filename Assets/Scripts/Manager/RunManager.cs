using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Michsky.UI.Dark;
using System;
using System.Collections;
using UnityEngine.Rendering;

public class RunManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private MusicController _musicController;
    [SerializeField] private HotelRateManager _hotelRateManager;
    [SerializeField] private MoneyManager _moneyManager;
    [SerializeField] private UIDissolveEffect _dissolveEffect;

    [Space(10)]
    [Header("Jeton")]
    [SerializeField] private JetonSO _jetonSO;

    [Space(10)]
    [Header("Music")]
    [SerializeField] private AudioClip _runMusic;
    [SerializeField] private AudioClip _runWinMusic;
    [SerializeField] private AudioClip _runLostMusic;

    [Space(10)]
    [Header("Events")]
    public UnityEvent onRunStart = new UnityEvent();
    public UnityEvent onRunPause = new UnityEvent();
    public UnityEvent onRunResume = new UnityEvent();
    public UnityEvent onRunLost = new UnityEvent();
    public UnityEvent onRunWin = new UnityEvent();
    public event Action OnAddCoin;

    [Space(10)]
    [Header("Debug Mode")]
    [SerializeField] private bool _debugMode = false;

    [Space(10)]
    [Header("End Screen Stats")]
    public float runChrono = 0f;
    public bool isGameRunning = false;
    public int coinsGranted;

    public InputAction pauseAction;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != "Run")
        {
            Debug.LogError("RunManager is not in the Run Scene");
            return;
        }

        // Trouvez les r�f�rences aux composants n�cessaires dans la sc�ne
        _saveManager = FindObjectOfType<SaveManager>();
        _hotelRateManager = FindObjectOfType<HotelRateManager>();
        _moneyManager = FindObjectOfType<MoneyManager>();

        // Activez l'effet de dissolution s'il n'est pas d�j� actif
        if (_dissolveEffect != null)
        {
            GameObject dissolveObject = _dissolveEffect.gameObject;
            if (!dissolveObject.activeSelf)
            {
                dissolveObject.SetActive(true);
            }
        }

        // V�rifiez que toutes les r�f�rences sont bien assign�es
        CheckRef();

        // Ajoutez les �couteurs d'�v�nements pour les changements d'�tat du jeu
        GameManager.Instance.onPlay.AddListener(OnRunStart);
        GameManager.Instance.onPauseEnter.AddListener(OnRunPause);
        GameManager.Instance.onPauseExit.AddListener(OnRunResume);

        // Ajoutez la fonction HandlePauseInput � l'action de pause
        pauseAction.performed += HandlePauseInput;
    }

    private void OnEnable()
    {
        // Activez l'action de pause
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        // D�sactivez l'action de pause pour �viter les entr�es ind�sirables
        pauseAction.Disable();
    }

    /// <summary>
    /// G�re l'entr�e pour mettre en pause ou reprendre le jeu.
    /// </summary>
    private void HandlePauseInput(InputAction.CallbackContext context)
    {
        // G�re la pause et la reprise du jeu en fonction de l'�tat actuel
        if (GameManager.Instance.isPause)
        {
            GameManager.Instance.ResumeGame();
            OnRunResume();
        }
        else
        {
            GameManager.Instance.PauseGame();
            OnRunPause();
        }
    }

    /// <summary>
    /// Fonction appel�e au d�marrage pour initialiser la musique et les effets.
    /// </summary>
    public void Start()
    {
        // D�marrez la musique de la course et d�clenchez l'effet de dissolution au d�but
        _musicController.PlayMusic(_runMusic, true);
        StartCoroutine(DissolveOutCoroutine());
    }

    private void Update()
    {
        // V�rifiez si la satisfaction de l'h�tel est � z�ro ou en dessous, ce qui signifie que la course est perdue
        if (_hotelRateManager.hotelRating.satisfactionQuantity <= 0)
        {
            OnRunLost();
        }
        // V�rifiez si la satisfaction de l'h�tel a atteint le maximum, ce qui signifie que la course est gagn�e
        else if (_hotelRateManager.hotelRating.satisfactionQuantity >= 100f)
        {
            OnRunWin();
        }

        // Si l'argent du joueur est �puis�, la course est perdue
        if (_moneyManager.playerMoney <= 0)
        {
            OnRunLost();
        }

        // Activez les contr�les de d�bogage si en mode d�bogage
        if (_debugMode)
        {
            DebugMode();
        }

        // Mettez � jour le chronom�tre de la course si le jeu est en cours
        if (isGameRunning)
        {
            runChrono += Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        // Retirez les �couteurs d'�v�nements pour �viter les fuites de m�moire
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlay.RemoveListener(OnRunStart);
            GameManager.Instance.onPauseEnter.RemoveListener(OnRunPause);
            GameManager.Instance.onPauseExit.RemoveListener(OnRunResume);
        }

        // Retirez le callback de l'action de pause pour �viter un comportement ind�sir�
        pauseAction.performed -= HandlePauseInput;
    }

    /// <summary>
    /// V�rifiez si les r�f�rences sont correctement assign�es et affichez des erreurs le cas �ch�ant.
    /// </summary>
    private void CheckRef()
    {
        if (_saveManager == null)
        {
            Debug.LogError("SaveManager is missing");
        }

        if (_hotelRateManager == null)
        {
            Debug.LogError("HotelRateManager is missing");
        }

        if (_moneyManager == null)
        {
            Debug.LogError("MoneyManager is missing");
        }
    }

    /// <summary>
    /// Commencez la course, sauvegardez l'�tat du jeu, et r�initialisez les variables n�cessaires.
    /// </summary>
    private void OnRunStart()
    {
        onRunStart.Invoke();
        _saveManager.SaveGame();
        isGameRunning = true;
        coinsGranted = 0;
    }

    /// <summary>
    /// Invoque l'�v�nement de pause.
    /// </summary>
    private void OnRunPause()
    {
        _musicController.ChangeVolume(0.1f);
        onRunPause.Invoke();
    }

    /// <summary>
    /// Invoque l'�v�nement de reprise.
    /// </summary>
    private void OnRunResume()
    {
        _musicController.ChangeVolume(_musicController.volume);
        onRunResume.Invoke();
    }

    /// <summary>
    /// G�re la perte de la course, sauvegarde l'�tat du jeu et arr�te la musique.
    /// </summary>
    [ContextMenu("Run Lost")]
    public void OnRunLost()
    {
        _saveManager.SaveGame();
        _musicController.StopMusic(true);
        onRunLost.Invoke();
        isGameRunning = false;

        // Attendez avant de mettre le jeu en pause pour permettre aux autres animations ou effets de se jouer
        StartCoroutine(WaitAndPause(3f));

        // Ajoutez des pi�ces en fonction de la performance du joueur
        CoinScoreAdd();
        Debug.Log("Run Lost");
    }

    /// <summary>
    /// Calcule et ajoute des pi�ces au total du joueur en fonction des avis re�us.
    /// </summary>
    private void CoinScoreAdd()
    {
        coinsGranted = (int)_hotelRateManager.totalReviews;
        _jetonSO.AddCoin(coinsGranted);
        OnAddCoin?.Invoke();
    }

    /// <summary>
    /// G�re la victoire de la course, sauvegarde l'�tat du jeu et arr�te la musique.
    /// </summary>
    [ContextMenu("Run Win")]
    public void OnRunWin()
    {
        _saveManager.SaveGame();
        _musicController.StopMusic(true);
        onRunWin.Invoke();
        isGameRunning = false;

        // Attendez avant de mettre le jeu en pause pour permettre aux autres animations ou effets de se jouer
        StartCoroutine(WaitAndPause(3f));

        // Ajoutez des pi�ces en fonction de la performance du joueur
        CoinScoreAdd();
        Debug.Log("Run Win");
    }

    /// <summary>
    /// Chargez la sc�ne du menu principal.
    /// </summary>
    public void LoadMainMenu()
    {
        GameManager.Instance.BackToMainMenu();
    }

    /// <summary>
    /// Attendez un temps sp�cifi� avant de mettre le jeu en pause.
    /// </summary>
    private IEnumerator WaitAndPause(float time)
    {
        yield return new WaitForSeconds(time);
        GameManager.Instance.PauseGame();
    }

    /// <summary>
    /// Contr�les de d�bogage pour modifier rapidement les variables du jeu.
    /// </summary>
    private void DebugMode()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _moneyManager.PayTaxe(50f);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            _moneyManager.AddMoney(100f);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            _hotelRateManager.AddReview(new RateReviews(5, "Wowwww dev good", "Dev", MonsterType.DEMON, 10));
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            _hotelRateManager.AddReview(new RateReviews(0, "Wowwww dev bad", "Dev", MonsterType.WITCH, -10));
        }
    }

    /// <summary>
    /// Assurez-vous que la valeur d'entr�e est bien entre -100 et 100, puis convertissez-la en une plage de 0 � 100.
    /// </summary>
    public float ConvertSatisfactionValue(float value)
    {
        value = Mathf.Clamp(value, -100f, 100f);
        return (value + 100f) / 2f;
    }

    /// <summary>
    /// Attendez 2 secondes avant de d�clencher l'effet de dissolution.
    /// </summary>
    private IEnumerator DissolveOutCoroutine()
    {
        yield return new WaitForSeconds(2f);
        _dissolveEffect.DissolveOut();
    }
}