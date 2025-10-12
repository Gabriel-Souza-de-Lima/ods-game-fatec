using System;
using UnityEngine;

/// <summary>
/// Fila de magias no estilo Tetris: mant�m Atual e Pr�xima; ao consumir, avan�a e sorteia nova Pr�xima.
/// </summary>
public class PlayerMagicQueue : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Evita repetir a mesma magia ao sortear a Pr�xima.")]
    [SerializeField] private bool avoidImmediateRepeat = true;

    [Header("Estado (read-only)")]
    [SerializeField] private WasteType current = WasteType.Glass;
    [SerializeField] private WasteType next = WasteType.Plastic;

    public event Action<WasteType, WasteType> OnChanged;

    private void Start()
    {
        // Inicializa��o aleat�ria (Atual e Pr�xima)
        current = RandomType();
        do { next = RandomType(); } while (avoidImmediateRepeat && next == current);
        RaiseChanged();
    }

    /// <summary> Consome a magia Atual, avan�a a fila e sorteia nova Pr�xima. </summary>
    public void ConsumeAndAdvance()
    {
        current = next;
        WasteType newNext;
        do { newNext = RandomType(); } while (avoidImmediateRepeat && newNext == current);
        next = newNext;
        RaiseChanged();
    }

    public WasteType Current => current;
    public WasteType Next => next;

    private WasteType RandomType()
    {
        int v = UnityEngine.Random.Range(0, 4);
        return (WasteType)v;
    }

    private void RaiseChanged() => OnChanged?.Invoke(current, next);
}
