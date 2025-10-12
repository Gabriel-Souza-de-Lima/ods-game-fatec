using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Projétil em UI: move em direção definida, detecta colisão com lixo e
/// aplica visual (sprite/tint) conforme o tipo de magia.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIProjectile : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 600f;

    [Header("Limites / Root")]
    [SerializeField] private RectTransform boundsRoot;
    [SerializeField] private string boundsTag = "Playfield";

    [Header("Magia / Regras")]
    [SerializeField] private WasteType magicType = WasteType.Glass;
    [SerializeField] private bool destroyOnMismatch = false;

    [Header("Visual")]
    [SerializeField] private Image spriteTarget; // arraste o Image do filho (ex.: ProjectileSprite)
    [SerializeField] private List<MagicVisual> visuals = new(); // 1 entrada por WasteType

    private RectTransform _rt;
    private Vector2 _dir = Vector2.up;
    private Dictionary<WasteType, MagicVisual> _visualMap;

    private static readonly Vector3[] _corners = new Vector3[4];
    private static readonly Vector3[] _cornersOther = new Vector3[4];
    private readonly List<UITrashItem> _trashBuffer = new();

    private void Awake()
    {
        _rt = transform as RectTransform;

        if (boundsRoot == null && !string.IsNullOrEmpty(boundsTag))
        {
            var go = GameObject.FindWithTag(boundsTag);
            if (go) boundsRoot = go.transform as RectTransform;
        }
        if (boundsRoot == null && _rt.parent != null)
            boundsRoot = _rt.parent as RectTransform;

        if (spriteTarget == null)
            spriteTarget = GetComponentInChildren<Image>(true);

        // mapa de visuais
        _visualMap = new Dictionary<WasteType, MagicVisual>(visuals.Count);
        foreach (var v in visuals) _visualMap[v.type] = v;

        // aplica o visual inicial
        ApplyVisual(magicType);
    }

    private void Update()
    {
        if (UIPauseController.IsPaused || _rt == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        var pos = _rt.anchoredPosition;
        pos += _dir * speed * dt;
        _rt.anchoredPosition = pos;

        // colisões com lixo spawnado
        UITrashRegistry.SnapshotTo(_trashBuffer);
        for (int i = 0; i < _trashBuffer.Count; i++)
        {
            var trash = _trashBuffer[i];
            if (trash == null) continue;

            if (OverlapsWorld(_rt, trash.RectT))
            {
                if (trash.Type == magicType)
                {
                    ScoreController.I?.AddBasePoints(10);
                    trash.DestroySelf();
                    DestroySelf();
                }
                else if (destroyOnMismatch)
                {
                    DestroySelf();
                }
                return;
            }
        }

        // saiu dos limites?
        if (boundsRoot != null && IsOutside(boundsRoot))
            DestroySelf();
    }

    public void SetMagicType(WasteType type)
    {
        magicType = type;
        ApplyVisual(magicType);
    }

    /// <summary>Define a direção do projétil (será normalizada).</summary>
    public void SetDirection(Vector2 dir)
    {
        _dir = dir.sqrMagnitude < 0.0001f ? Vector2.up : dir.normalized;
    }

    private void ApplyVisual(WasteType type)
    {
        if (!spriteTarget) return;
        if (_visualMap != null && _visualMap.TryGetValue(type, out var vis))
        {
            if (vis.sprite) spriteTarget.sprite = vis.sprite;
            spriteTarget.color = Color.white;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"UIProjectile: sprite não configurado para {type}.");
#endif
            spriteTarget.color = Color.white;
        }
    }

    private bool IsOutside(RectTransform root)
    {
        GetWorldRect(_rt, _corners);
        GetWorldRect(root, _cornersOther);
        Rect rProj = RectFromCorners(_corners);
        Rect rBounds = RectFromCorners(_cornersOther);
        return !rProj.Overlaps(rBounds, true);
    }

    private static bool OverlapsWorld(RectTransform a, RectTransform b)
    {
        GetWorldRect(a, _corners);
        Rect ra = RectFromCorners(_corners);
        GetWorldRect(b, _cornersOther);
        Rect rb = RectFromCorners(_cornersOther);
        return ra.Overlaps(rb, true);
    }

    private static void GetWorldRect(RectTransform rt, Vector3[] buf) => rt.GetWorldCorners(buf);

    private static Rect RectFromCorners(Vector3[] c)
    {
        float minX = Mathf.Min(c[0].x, c[1].x, c[2].x, c[3].x);
        float maxX = Mathf.Max(c[0].x, c[1].x, c[2].x, c[3].x);
        float minY = Mathf.Min(c[0].y, c[1].y, c[2].y, c[3].y);
        float maxY = Mathf.Max(c[0].y, c[1].y, c[2].y, c[3].y);
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private void DestroySelf()
    {
        if (this && gameObject) Destroy(gameObject);
    }

    [System.Serializable]
    public struct MagicVisual
    {
        public WasteType type;
        public Sprite sprite;
    }

    public WasteType MagicType => magicType;
}
