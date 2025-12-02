using UnityEngine;
using TMPro;
using DG.Tweening;

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

    [Header("Feedback de Erro (DOTween)")]
    [SerializeField] private float penaltyShakeDuration = 0.2f;
    [SerializeField] private float penaltyShakeStrength = 20f;
    [SerializeField] private int penaltyShakeVibrato = 20;
    [SerializeField] private bool penaltyShakeSnapping = false;
    [SerializeField] private bool penaltyShakeFadeOut = true;

    private Vector2 _scoreTextInitialPos;
    private Tween _penaltyTween;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        if (scoreTextGameplay != null)
        {
            // guarda a posição original do texto pra resetar depois do shake
            _scoreTextInitialPos = scoreTextGameplay.rectTransform.anchoredPosition;
        }
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

    public void AddPenalty(int basePenalty)
    {
        if (basePenalty <= 0) return;

        int finalPenalty = Mathf.RoundToInt(basePenalty * multiplier);

        score = Mathf.Max(0, score - finalPenalty);
        Refresh();

        PlayPenaltyFeedback();
    }

    private void PlayPenaltyFeedback()
    {
        if (!scoreTextGameplay) return;

        var rt = scoreTextGameplay.rectTransform;

        // se já tiver um tween rolando, mata ele pra não acumular
        if (_penaltyTween != null && _penaltyTween.IsActive())
        {
            _penaltyTween.Kill();
            _penaltyTween = null;
        }

        // reseta pra posição original antes de chacoalhar
        rt.anchoredPosition = _scoreTextInitialPos;

        // shake horizontal usando a posição de âncora
        _penaltyTween = rt.DOShakeAnchorPos(
            duration: penaltyShakeDuration,
            strength: new Vector2(penaltyShakeStrength, 0f), // só no eixo X
            vibrato: penaltyShakeVibrato,
            randomness: 90f,
            snapping: penaltyShakeSnapping,
            fadeOut: penaltyShakeFadeOut
        );
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
