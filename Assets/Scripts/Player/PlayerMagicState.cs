using UnityEngine;

/// <summary>
/// Estado global mínimo do tipo de magia atual do jogador.
/// Útil para evitar acoplamento direto entre Shooter e Projectile.
/// Atualize PlayerMagicState.Current quando o jogador trocar de magia.
/// </summary>
public static class PlayerMagicState
{
    /// <summary> Tipo de magia atualmente selecionado pelo jogador. </summary>
    public static WasteType Current = WasteType.Glass;
}
