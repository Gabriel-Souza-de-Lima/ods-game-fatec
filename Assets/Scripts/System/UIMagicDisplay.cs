using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Espelha no HUD a magia atual e a próxima a partir da PlayerMagicQueue.
/// Mantém o tamanho original do HUD por padrão (não usa SetNativeSize).
/// </summary>
public class UIMagicDisplay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMagicQueue magicQueue;
    [SerializeField] private Image currentImage;
    [SerializeField] private Image nextImage;

    [Header("Ícones Principais (Atual)")]
    [SerializeField] private Sprite glassIcon;
    [SerializeField] private Sprite plasticIcon;
    [SerializeField] private Sprite metalIcon;
    [SerializeField] private Sprite paperIcon;

    [Header("Ícones Secundários (Próxima)")]
    [SerializeField] private Sprite glassIconNext;
    [SerializeField] private Sprite plasticIconNext;
    [SerializeField] private Sprite metalIconNext;
    [SerializeField] private Sprite paperIconNext;

    [Header("Tamanho/Render")]
    [Tooltip("Se verdadeiro, NÃO chama SetNativeSize ao trocar sprites (mantém tamanho do HUD).")]
    [SerializeField] private bool keepOriginalSize = true;

    [Tooltip("Se verdadeiro, habilita preserveAspect nos Images para evitar distorção.")]
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

        // Mantém o tamanho original do HUD (não altera o RectTransform)
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
