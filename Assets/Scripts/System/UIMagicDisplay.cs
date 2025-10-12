using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Espelha no HUD a magia atual e a pr�xima a partir da PlayerMagicQueue.
/// Mant�m o tamanho original do HUD por padr�o (n�o usa SetNativeSize).
/// </summary>
public class UIMagicDisplay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMagicQueue magicQueue;
    [SerializeField] private Image currentImage;
    [SerializeField] private Image nextImage;

    [Header("�cones Principais (Atual)")]
    [SerializeField] private Sprite glassIcon;
    [SerializeField] private Sprite plasticIcon;
    [SerializeField] private Sprite metalIcon;
    [SerializeField] private Sprite paperIcon;

    [Header("�cones Secund�rios (Pr�xima)")]
    [SerializeField] private Sprite glassIconNext;
    [SerializeField] private Sprite plasticIconNext;
    [SerializeField] private Sprite metalIconNext;
    [SerializeField] private Sprite paperIconNext;

    [Header("Tamanho/Render")]
    [Tooltip("Se verdadeiro, N�O chama SetNativeSize ao trocar sprites (mant�m tamanho do HUD).")]
    [SerializeField] private bool keepOriginalSize = true;

    [Tooltip("Se verdadeiro, habilita preserveAspect nos Images para evitar distor��o.")]
    [SerializeField] private bool preserveAspect = true;

    private void OnEnable()
    {
        if (!magicQueue) magicQueue = FindFirstObjectByType<PlayerMagicQueue>();
        if (preserveAspect)
        {
            if (currentImage) currentImage.preserveAspect = true;
            if (nextImage) nextImage.preserveAspect = true;
        }

        if (magicQueue)
        {
            magicQueue.OnChanged += HandleQueueChanged;
            // refresh inicial
            HandleQueueChanged(magicQueue.Current, magicQueue.Next);
        }
    }

    private void OnDisable()
    {
        if (magicQueue)
            magicQueue.OnChanged -= HandleQueueChanged;
    }

    private void HandleQueueChanged(WasteType cur, WasteType nxt)
    {
        UpdateView(cur, nxt);
    }

    private void UpdateView(WasteType cur, WasteType nxt)
    {
        if (currentImage) currentImage.sprite = IconFor(cur);
        if (nextImage) nextImage.sprite = IconForNext(nxt);

        // Mant�m o tamanho original do HUD (n�o altera o RectTransform)
        if (!keepOriginalSize)
        {
            if (currentImage) currentImage.SetNativeSize();
            if (nextImage) nextImage.SetNativeSize();
        }
    }

    private Sprite IconFor(WasteType t) => t switch
    {
        WasteType.Glass => glassIcon,
        WasteType.Plastic => plasticIcon,
        WasteType.Metal => metalIcon,
        WasteType.Paper => paperIcon,
        _ => null
    };

    private Sprite IconForNext(WasteType t) => t switch
    {
        WasteType.Glass => glassIconNext ? glassIconNext : glassIcon,
        WasteType.Plastic => plasticIconNext ? plasticIconNext : plasticIcon,
        WasteType.Metal => metalIconNext ? metalIconNext : metalIcon,
        WasteType.Paper => paperIconNext ? paperIconNext : paperIcon,
        _ => null
    };
}
