using UnityEngine;
using TMPro;

/// <summary>
/// Placar simples com multiplicador. Atualiza um TMP_Text com números crus (ex.: 10, 20, 1000).
/// </summary>
public class ScoreController : MonoBehaviour
{
    public static ScoreController I;            // acesso rápido

    [SerializeField] private TMP_Text scoreTextGameplay;
    [SerializeField] private TMP_Text scoreTextGameover;
    [SerializeField] private int score = 0;
    [SerializeField] private float multiplier = 1f; // ajustado pelo spawner

    private void Awake()
    {
        I = this;
        Refresh();
    }

    public void AddBasePoints(int basePoints)
    {
        if (basePoints <= 0) return;
        score += Mathf.RoundToInt(basePoints * Mathf.Max(0.1f, multiplier));
        Refresh();
    }

    public void SetMultiplier(float m)
    {
        multiplier = Mathf.Max(0.1f, m);
        // não precisa dar Refresh aqui, pois a UI só mostra o score cru
    }

    public void ResetScore(int startScore = 0, float startMult = 1f)
    {
        score = Mathf.Max(0, startScore);
        multiplier = Mathf.Max(0.1f, startMult);
        Refresh();
    }

    private void Refresh()
    {
        if (scoreTextGameplay)
        {
            scoreTextGameplay.text = score.ToString();
            scoreTextGameover.text = score.ToString();
        }
    }
}
