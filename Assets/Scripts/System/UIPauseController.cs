using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Botão Pause (HUD) ou Start (gamepad) abrem o menu e pausam o jogo.
/// ESC só RESUME (não pausa).
/// Botão Resume fecha e retoma.
/// </summary>
public class UIPauseController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject pauseMenu;     // painel/empty desabilitado
    [SerializeField] private Button pauseButton;       // botão na HUD (abre/pause)
    [SerializeField] private Button resumeButton;      // botão no menu (fecha/resume)

    [Header("Inputs")]
    [SerializeField] private Key resumeKey = Key.Escape; // ESC só para RESUMIR

    [Header("Also Pause (optional)")]
    [SerializeField] private bool pauseAudio = true;
    [SerializeField] private bool freezeAnimators = true;
    [SerializeField] private bool freezeParticles = true;

    private Animator[] _animators;
    private float[] _animatorPrevSpeeds;
    private ParticleSystem[] _particles;

    public static bool IsPaused { get; private set; }

    // -------- Singleton opcional --------
    private static UIPauseController _instance;
    // ------------------------------------

    private void Awake()
    {
        _instance = this;

        if (pauseButton) pauseButton.onClick.AddListener(Pause);
        if (resumeButton) resumeButton.onClick.AddListener(Resume);

        if (pauseMenu) pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        if (freezeAnimators)
        {
            _animators = FindObjectsByType<Animator>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            _animatorPrevSpeeds = new float[_animators.Length];
            for (int i = 0; i < _animators.Length; i++)
                _animatorPrevSpeeds[i] = _animators[i].speed;
        }

        if (freezeParticles)
            _particles = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    private void Update()
    {
        // ESC NÃO PAUSA. Se estiver pausado, ESC RESUME.
        if (Keyboard.current != null && Keyboard.current[resumeKey].wasPressedThisFrame)
        {
            if (IsPaused) { Resume(); }
            return;
        }

        // Start do gamepad (opcional): pode pausar e despausar
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        if (pauseMenu) pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        if (pauseAudio) AudioListener.pause = true;

        if (freezeAnimators && _animators != null)
            for (int i = 0; i < _animators.Length; i++)
                if (_animators[i]) _animators[i].speed = 0f;

        if (freezeParticles && _particles != null)
            foreach (var ps in _particles)
                if (ps) ps.Pause();

        if (resumeButton) EventSystem.current?.SetSelectedGameObject(resumeButton.gameObject);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;

        if (pauseMenu) pauseMenu.SetActive(false);
        Time.timeScale = 1f;

        if (pauseAudio) AudioListener.pause = false;

        if (freezeAnimators && _animators != null)
            for (int i = 0; i < _animators.Length; i++)
                if (_animators[i]) _animators[i].speed = _animatorPrevSpeeds[i];

        if (freezeParticles && _particles != null)
            foreach (var ps in _particles)
                if (ps) ps.Play();

        EventSystem.current?.SetSelectedGameObject(null);
    }

    // -------- API estática segura --------
    public static void SetPaused(bool paused)
    {
        // Se houver uma instância na cena, delega para ela (garante UI/áudio/animators)
        if (_instance != null)
        {
            if (paused) _instance.Pause();
            else _instance.Resume();
            return;
        }

        // Fallback: controla apenas timeScale/áudio estático
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
    }

    public static void ForceUnpause() => SetPaused(false);
    // ------------------------------------
}
