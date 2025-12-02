using System;
using UnityEngine;

/// <summary>
/// Fila de magias no estilo Tetris: mantém Atual e Próxima; ao consumir, avança e sorteia nova Próxima.
/// </summary>
public class PlayerMagicQueue : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Evita repetir a mesma magia ao sortear a Próxima.")]
    [SerializeField] private bool avoidImmediateRepeat = true;

    [Header("Estado (read-only)")]
    [SerializeField] private WasteType current = WasteType.Glass;
    [SerializeField] private WasteType next = WasteType.Plastic;

    public event Action<WasteType, WasteType> OnChanged;

    private void Start()
    {
        // Inicialização aleatória (Atual e Próxima)
        current = RandomType();
        do { next = RandomType(); } while (avoidImmediateRepeat && next == current);
        RaiseChanged();
    }

    /// <summary> Consome a magia Atual, avança a fila e sorteia nova Próxima. </summary>
    public void ConsumeAndAdvance()
    {
        current = next;
        WasteType newNext;
        do { newNext = RandomType(); } while (avoidImmediateRepeat && newNext == current);
        next = newNext;
        RaiseChanged();
    }

    /// <summary>
    /// Troca a magia Atual com a Próxima sem consumir nem sortear uma nova.
    /// Útil para permitir que o jogador alterne entre as duas (ex.: Tab).
    /// </summary>
    public void SwapCurrentAndNext()
    {
        // se quiser, pode checar se current == next e não fazer nada, mas não é obrigatório
        (current, next) = (next, current);
        RaiseChanged();
    }

    public WasteType Current => current;
    public WasteType Next => next;

    private WasteType RandomType()
    {
        int v = UnityEngine.Random.Range(0, 4);
        return (WasteType)v;
    }

    private void RaiseChanged()
    {
        // opcional: manter um estado global da magia atual
        PlayerMagicState.Current = current;

        OnChanged?.Invoke(current, next);
    }
}
