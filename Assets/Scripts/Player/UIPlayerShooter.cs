using UnityEngine;

/// <summary>
/// Controla o disparo do jogador: instancia projétil, define tipo atual de magia
/// e aponta a direção para o mouse na mesma área do Canvas.
/// </summary>
public class UIPlayerShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform projectilePrefab;
    [SerializeField] private RectTransform shootParent;
    [SerializeField] private PlayerMagicQueue magicQueue;
    [SerializeField] private RectTransform projectileLayer; // alvo correto dos projéteis

    [Header("Disparo")]
    [SerializeField] private float fireCooldown = 0.25f;
    [SerializeField] private KeyCode fireKey = KeyCode.Space;

    private float _cd;

    private void Awake()
    {
        if (!magicQueue) magicQueue = FindFirstObjectByType<PlayerMagicQueue>();
        if (!shootParent && transform.parent) shootParent = transform.parent as RectTransform;
    }

    private void Update()
    {
        if (UIPauseController.IsPaused || LifeController.IsGameOver) return;

        _cd -= Time.deltaTime;
        if (_cd <= 0f && Input.GetKey(fireKey))
            TryShoot();
    }

    private void TryShoot()
    {
        if (!projectilePrefab || !magicQueue) return;

        // Parent alvo: usa ProjectileLayer se existir; senão, cai no shootParent
        RectTransform parent = projectileLayer ? projectileLayer : shootParent;
        if (!parent) return;

        var playerRt = transform as RectTransform;

        // Câmera do Canvas (Overlay = null está ok)
        var canvas = parent.GetComponentInParent<Canvas>();
        var uiCam = canvas ? canvas.worldCamera : null;

        // Converte posição do PLAYER para o espaço local do parent (ProjectileLayer)
        Vector2 spawnPosLocal = Vector2.zero;
        if (playerRt)
        {
            Vector2 playerScreen = RectTransformUtility.WorldToScreenPoint(uiCam, playerRt.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, playerScreen, uiCam, out spawnPosLocal);
        }

        // Converte MOUSE para o mesmo espaço local do parent (ProjectileLayer)
        Vector2 mouseLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Input.mousePosition, uiCam, out mouseLocal);

        // Direção do spawn até o mouse
        Vector2 dir = mouseLocal - spawnPosLocal;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

        // Instancia como FILHO do ProjectileLayer, mantendo o espaço local
        var projRt = Instantiate(projectilePrefab);
        projRt.SetParent(parent, false);
        projRt.anchoredPosition = spawnPosLocal;

        var proj = projRt.GetComponent<UIProjectile>();
        if (proj)
        {
            proj.SetMagicType(magicQueue.Current); // visual conforme magia
            proj.SetDirection(dir);                // direção no espaço do layer
        }

        magicQueue.ConsumeAndAdvance();
        _cd = fireCooldown;
    }
}
