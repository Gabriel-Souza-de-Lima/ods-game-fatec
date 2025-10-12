using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lixo em UI: tipo, sprite, movimento vertical, rotação e ajuste de tamanho mínimo.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UITrashItem : MonoBehaviour
{
    [Header("Read-only (debug)")]
    [SerializeField] private WasteType type;
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeedDegPerSec;

    private RectTransform _rt;
    private Image _image;
    private RectTransform _parentLayer;

    // Disparado quando um lixo sai da tela sem ser destruído pelo jogador
    public static System.Action OnAnyMissed;

    [Header("Escala Mínima (opcional)")]
    [SerializeField] private float minSizePixels = 80f;

    [Header("Variação de Tamanho")]
    [Tooltip("Se verdadeiro, aplica variação de tamanho por item (redução a partir do tamanho nativo).")]
    [SerializeField] private bool enableSizeVariance = true;

    [Tooltip("Redução MÁXIMA (em pixels) aplicada sobre a maior dimensão do sprite. O tamanho nativo é o teto.")]
    [SerializeField] private float maxShrinkPixels = 100f;

    public void Init(WasteType wasteType, Sprite sprite, float moveSpeed, RectTransform parentLayer, float rotationDegPerSec)
    {
        _rt = transform as RectTransform;
        _image = GetComponent<Image>();
        _parentLayer = parentLayer;

        type = wasteType;
        speed = moveSpeed;
        rotationSpeedDegPerSec = rotationDegPerSec;

        if (_image)
        {
            _image.sprite = sprite;
            _image.preserveAspect = true;
            _image.color = Color.white;

            // Tamanho nativo = tamanho MÁXIMO
            _image.SetNativeSize();

            // Aplica variação (apenas diminui, nunca aumenta; respeita minSizePixels)
            ApplyRandomSizeVariance();

            // Garante mínimo absoluto
            ApplyMinimumSize();
        }
    }

    private void OnEnable() => UITrashRegistry.Register(this);
    private void OnDisable() => UITrashRegistry.Unregister(this);

    /// <summary>
    /// Reduz uniformemente a maior dimensão do sprite em até 'maxShrinkPixels',
    /// sem deixar ficar abaixo de 'minSizePixels'. Mantém proporção.
    /// </summary>
    private void ApplyRandomSizeVariance()
    {
        if (!enableSizeVariance || _rt == null || _image == null || _image.sprite == null) return;

        // Dimensão atual após SetNativeSize (escala = 1)
        float width = _rt.rect.width;
        float height = _rt.rect.height;
        float largest = Mathf.Max(width, height);
        if (largest <= 0f) return;

        // Se já é menor ou igual ao mínimo, não reduz
        if (largest <= minSizePixels) return;

        // Redução máxima permitida sem passar do mínimo
        float allowedMaxShrink = Mathf.Min(Mathf.Max(0f, maxShrinkPixels), largest - minSizePixels);
        if (allowedMaxShrink <= 0f) return;

        float shrink = Random.Range(0f, allowedMaxShrink);
        float targetLargest = largest - shrink;

        // Escala uniforme: alvo / original
        float scale = targetLargest / largest;
        _rt.localScale = new Vector3(scale, scale, 1f);
    }

    private void ApplyMinimumSize()
    {
        if (_rt == null || _image.sprite == null) return;

        // Considera o tamanho JÁ escalado pela variação
        float width = _rt.rect.width;
        float height = _rt.rect.height;
        float largest = Mathf.Max(width, height);

        if (largest < minSizePixels && largest > 0f)
        {
            float scaleUp = minSizePixels / largest;
            // Multiplica pela escala atual para chegar no mínimo
            _rt.localScale = new Vector3(_rt.localScale.x * scaleUp, _rt.localScale.y * scaleUp, 1f);
        }
        // Caso contrário, mantém a escala definida (nativa ou com variação).
    }

    private void Update()
    {
        if (UIPauseController.IsPaused) return;
        if (_rt == null || _parentLayer == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        _rt.anchoredPosition -= new Vector2(0f, speed * dt);
        _rt.Rotate(0f, 0f, rotationSpeedDegPerSec * dt, Space.Self);

        float halfHeight = _parentLayer.rect.height * 0.5f;
        if (_rt.anchoredPosition.y < -halfHeight - 200f)
        {
            OnAnyMissed?.Invoke();
            DestroySelf();
            return;
        }
    }

    public void DestroySelf()
    {
        if (this != null && gameObject != null)
            Destroy(gameObject);
    }

    public WasteType Type => type;
    public float Speed => speed;
    public float RotationSpeedDegPerSec => rotationSpeedDegPerSec;
    public RectTransform RectT => _rt != null ? _rt : (RectTransform)transform;
}
