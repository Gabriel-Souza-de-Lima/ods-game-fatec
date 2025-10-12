using System.Collections.Generic;

/// <summary>
/// Registro global de lixos ativos na UI (para checagem de colisão).
/// </summary>
public static class UITrashRegistry
{
    private static readonly HashSet<UITrashItem> _items = new();

    public static void Register(UITrashItem item)
    {
        if (item != null) _items.Add(item);
    }

    public static void Unregister(UITrashItem item)
    {
        if (item != null) _items.Remove(item);
    }

    public static void SnapshotTo(List<UITrashItem> buffer)
    {
        buffer.Clear();
        foreach (var it in _items)
            if (it != null) buffer.Add(it);
    }
}
