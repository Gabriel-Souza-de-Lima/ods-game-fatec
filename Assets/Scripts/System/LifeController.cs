using UnityEngine;
using UnityEngine.UI;

public class LifeController : MonoBehaviour
{
    [Header("HUD de Vida (3 corações)")]
    [SerializeField] private Image[] heartImages;   // 3 elementos, na ordem da esquerda p/ direita
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel; // desativado na cena

    [Header("Config")]
    [SerializeField, Range(0, 3)] private int maxLives = 3;
    [SerializeField, Range(0, 3)] private int startLives = 3;

    private int _lives;

    public static bool IsGameOver { get; private set; }

    private void Awake()
    {
        IsGameOver = false;
        _lives = Mathf.Clamp(startLives, 0, maxLives);
        ApplyHearts();
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }


    private void OnEnable()
    {
        UITrashItem.OnAnyMissed += HandleMissedTrash;
    }

    private void OnDisable()
    {
        UITrashItem.OnAnyMissed -= HandleMissedTrash;
    }

    private void HandleMissedTrash()
    {
        if (IsGameOver) return;

        _lives = Mathf.Max(0, _lives - 1);
        ApplyHearts();

        if (_lives <= 0)
            TriggerGameOver();
    }

    private void ApplyHearts()
    {
        if (heartImages == null) return;
        for (int i = 0; i < heartImages.Length; i++)
        {
            var img = heartImages[i];
            if (!img) continue;

            // Índices < _lives ficam cheios, o resto vazio
            img.sprite = (i < _lives) ? heartFull : heartEmpty;
        }
    }

    private void TriggerGameOver()
    {
        IsGameOver = true;                 // << NOVO: sinaliza estado de game over
        Time.timeScale = 0f;               // pausa o tempo (mesma lógica do pause)
        // Se seu UIPauseController tiver uma API pública, você pode também marcar lá:
        // UIPauseController.SetPaused(true); // (use se existir)
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    public void ResetLives()
    {
        _lives = Mathf.Clamp(startLives, 0, maxLives);
        ApplyHearts();
        if (gameOverPanel) gameOverPanel.SetActive(false);
        IsGameOver = false;                // << NOVO: limpa o estado de game over
        Time.timeScale = 1f;               // despausa para nova rodada
    }
}
