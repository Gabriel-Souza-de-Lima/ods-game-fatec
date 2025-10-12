using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla navegação entre cenas (Menu, Gameplay, Retry) e exibição de painéis (Opções, Como Jogar, Créditos).
/// Também permite encerrar o jogo.
/// Pensado para ser chamado por botões de UI.
/// </summary>
public class SceneController : MonoBehaviour
{
    [Header("Nomes das cenas")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "Gameplay";

    [Header("Painéis (arraste no Inspector)")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject creditsPanel;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    // --- Troca de cenas ---
    public void LoadMainMenu()
    {
        SafeUnpause();
        SceneManager.LoadScene(mainMenuScene);
    }

    public void StartGame()
    {
        SafeUnpause();
        SceneManager.LoadScene(gameplayScene);
    }

    public void Retry()
    {
        SafeUnpause();
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.name);
    }

    // --- Painéis ---
    public void OpenOptions() => SetPanel(optionsPanel, true);
    public void CloseOptions() => SetPanel(optionsPanel, false);

    public void OpenHowToPlay() => SetPanel(howToPlayPanel, true);
    public void CloseHowToPlay() => SetPanel(howToPlayPanel, false);

    public void OpenCredits() => SetPanel(creditsPanel, true);
    public void CloseCredits() => SetPanel(creditsPanel, false);

    /// <summary>
    /// Fecha todos os painéis abertos (opcional).
    /// </summary>
    public void CloseAllPanels()
    {
        SetPanel(optionsPanel, false);
        SetPanel(howToPlayPanel, false);
        SetPanel(creditsPanel, false);
    }

    // --- Sair do jogo ---
    public void QuitGame()
    {
        Debug.Log("Encerrando o jogo...");

        SafeUnpause();

#if UNITY_EDITOR
        // Encerra o modo Play no editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Fecha o aplicativo no build final
        Application.Quit();
#endif
    }

    // --- Utilidades ---
    private void SetPanel(GameObject panel, bool state)
    {
        if (panel != null)
            panel.SetActive(state);
    }

    private void SafeUnpause()
    {
        // Garante tempo normal
        Time.timeScale = 1f;

        // E limpa o estado estático de pausa (importante!)
        UIPauseController.ForceUnpause(); // novo método
    }


    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
    }
}
