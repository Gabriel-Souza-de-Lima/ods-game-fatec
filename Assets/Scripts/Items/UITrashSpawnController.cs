using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Spawner de lixos (UI) com variação por item: velocidade e rotação.
/// </summary>
public class UITrashSpawnController : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private RectTransform trashLayer;
    [SerializeField] private RectTransform trashPrefab;

    [Header("Sprites por Tipo")]
    [SerializeField] private List<Sprite> glassSprites = new();
    [SerializeField] private List<Sprite> plasticSprites = new();
    [SerializeField] private List<Sprite> metalSprites = new();
    [SerializeField] private List<Sprite> paperSprites = new();

    [Header("Cores (tint)")]
    [SerializeField] private Color glassTint = new(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color plasticTint = new(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color metalTint = new(0.95f, 0.85f, 0.15f, 1f);
    [SerializeField] private Color paperTint = new(0.2f, 0.45f, 0.95f, 1f);

    [Header("Spawn (Timing)")]
    [SerializeField] private float baseSpawnInterval = 2.0f;
    [SerializeField] private float minSpawnInterval = 0.45f;
    [SerializeField] private float spawnAcceleration = 0.01f;

    [Header("Movimento (Velocidade base px/s)")]
    [SerializeField] private float baseSpeed = 80f;
    [SerializeField] private float speedGrowthPerSecond = 4f;

    [Header("Variedade por Item")]
    [Tooltip("Multiplicador aleatório aplicado à velocidade atual (min..max).")]
    [SerializeField] private Vector2 perItemSpeedMultiplierRange = new(0.8f, 1.3f);

    [Tooltip("Velocidade de rotação por item (graus/s). Use valores baixos para rotação suave.")]
    [SerializeField] private Vector2 perItemRotationDegPerSecRange = new(-25f, 25f);

    [Tooltip("Evita rotações muito próximas de 0 (zona morta, em graus/s).")]
    [SerializeField] private float rotationDeadZone = 3f;

    [Header("Dificuldade (Quantidade simultânea)")]
    [SerializeField] private int initialMaxConcurrent = 1;
    [SerializeField] private float secondsPerConcurrentIncrease = 25f;
    [SerializeField] private int hardCapMaxConcurrent = 20;

    [Header("Distribuição de Tipos (Pesos)")]
    [Range(0f, 1f)][SerializeField] private float weightGlass = 0.25f;
    [Range(0f, 1f)][SerializeField] private float weightPlastic = 0.25f;
    [Range(0f, 1f)][SerializeField] private float weightMetal = 0.25f;
    [Range(0f, 1f)][SerializeField] private float weightPaper = 0.25f;

    [Header("Spawn Area")]
    [SerializeField] private float horizontalPadding = 40f;

    private float _elapsed;
    private float _spawnTimer;
    private int _currentMaxConcurrent;
    private readonly List<UITrashItem> _alive = new();

    private void Awake()
    {
        _currentMaxConcurrent = Mathf.Max(1, initialMaxConcurrent);
    }

    private void Update()
    {
        if (UIPauseController.IsPaused) return;
        if (trashLayer == null || trashPrefab == null) return;

        float dt = Time.deltaTime;
        _elapsed += dt;
        _spawnTimer += dt;

        float baseForNow = Mathf.Max(10f, baseSpeed + speedGrowthPerSecond * _elapsed);
        float m = Mathf.Clamp(baseForNow / Mathf.Max(1f, baseSpeed), 1f, 10f);
        ScoreController.I?.SetMultiplier(m);

        CullDestroyed();

        float currentInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - spawnAcceleration * _elapsed);

        int targetMaxConcurrent = Mathf.Min(
            hardCapMaxConcurrent,
            initialMaxConcurrent + Mathf.FloorToInt(_elapsed / Mathf.Max(1f, secondsPerConcurrentIncrease))
        );
        if (targetMaxConcurrent > _currentMaxConcurrent)
            _currentMaxConcurrent = targetMaxConcurrent;

        if (_alive.Count < _currentMaxConcurrent && _spawnTimer >= currentInterval)
        {
            TrySpawnOne();
            _spawnTimer = 0f;
        }
    }

    private void TrySpawnOne()
    {
        WasteType type = RollWasteType();
        Sprite sprite = PickSpriteFor(type);
        if (sprite == null) return;

        RectTransform itemRt = Instantiate(trashPrefab, trashLayer);
        itemRt.localRotation = Quaternion.identity;

        Vector2 spawnPos = GetTopSpawnPosition(itemRt);
        itemRt.anchoredPosition = spawnPos;

        float baseForNow = Mathf.Max(10f, baseSpeed + speedGrowthPerSecond * _elapsed);
        float mult = Random.Range(perItemSpeedMultiplierRange.x, perItemSpeedMultiplierRange.y);
        float itemSpeed = Mathf.Max(5f, baseForNow * Mathf.Max(0.05f, mult));

        float itemRot = Random.Range(perItemRotationDegPerSecRange.x, perItemRotationDegPerSecRange.y);
        if (Mathf.Abs(itemRot) < rotationDeadZone)
        {
            // empurra para longe da zona morta preservando o sinal
            itemRot = (itemRot < 0f ? -1f : 1f) * rotationDeadZone;
        }

        Color tint = TintFor(type);

        var item = itemRt.GetComponent<UITrashItem>();
        if (item == null) item = itemRt.gameObject.AddComponent<UITrashItem>();
        item.Init(type, sprite, itemSpeed, trashLayer, itemRot);

        _alive.Add(item);
    }

    private void CullDestroyed()
    {
        for (int i = _alive.Count - 1; i >= 0; i--)
            if (_alive[i] == null) _alive.RemoveAt(i);
    }

    private WasteType RollWasteType()
    {
        float g = Mathf.Max(0f, weightGlass);
        float p = Mathf.Max(0f, weightPlastic);
        float m = Mathf.Max(0f, weightMetal);
        float b = Mathf.Max(0f, weightPaper);
        float sum = g + p + m + b;

        if (sum <= 0f) return (WasteType)Random.Range(0, 4);

        float r = Random.value * sum;
        if (r < g) return WasteType.Glass; r -= g;
        if (r < p) return WasteType.Plastic; r -= p;
        if (r < m) return WasteType.Metal;
        return WasteType.Paper;
    }

    private Sprite PickSpriteFor(WasteType type)
    {
        var list = type switch
        {
            WasteType.Glass => glassSprites,
            WasteType.Plastic => plasticSprites,
            WasteType.Metal => metalSprites,
            WasteType.Paper => paperSprites,
            _ => null
        };
        if (list == null || list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    private Color TintFor(WasteType type) => type switch
    {
        WasteType.Glass => glassTint,
        WasteType.Plastic => plasticTint,
        WasteType.Metal => metalTint,
        WasteType.Paper => paperTint,
        _ => Color.white
    };

    private Vector2 GetTopSpawnPosition(RectTransform item)
    {
        float halfW = trashLayer.rect.width * 0.5f;
        float halfH = trashLayer.rect.height * 0.5f;
        float x = Random.Range(-halfW + horizontalPadding, halfW - horizontalPadding);
        float y = halfH + (item.rect.height * 0.5f) + 40f;
        return new Vector2(x, y);
    }
}
