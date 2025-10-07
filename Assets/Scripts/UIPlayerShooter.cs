using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIPlayerShooter : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private RectTransform projectileLayer; // container dentro do Canvas
    [SerializeField] private RectTransform shootOrigin;     // ícone/empty do player
    [SerializeField] private RectTransform projectilePrefab;

    [Header("Input / UI")]
    [SerializeField] private GraphicRaycaster uiRaycaster;  // arraste o GraphicRaycaster do seu Canvas

    [Header("Tuning")]
    [SerializeField] private float projectileSpeed = 1500f; // px/s
    [SerializeField] private float lifetime = 1.5f;

    // Para Overlay, use null; para Screen Space - Camera, use Camera.main
    private Camera CamForUI => null;

    private void Awake()
    {
        if (uiRaycaster == null)
        {
            // tenta achar automaticamente no Canvas pai
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                uiRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }
    }

    private void Update()
    {
        if (UIPauseController.IsPaused) return;
        if (Mouse.current == null || projectileLayer == null || projectilePrefab == null) return;

        // Se o clique foi sobre um botão (UI interativa), não atira
        if (IsPointerOverButton(Mouse.current.position.ReadValue()))
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 1) Mouse em tela -> local do projectileLayer
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                projectileLayer, mouseScreen, CamForUI, out var mouseLocal);

            // 2) ShootPoint -> local do projectileLayer
            Vector2 originLocal = WorldToLocalIn(projectileLayer, shootOrigin);

            // 3) Direção e spawn
            Vector2 dir = (mouseLocal - originLocal).normalized;

            RectTransform proj = Instantiate(projectilePrefab, projectileLayer);
            proj.anchoredPosition = originLocal;
            proj.localRotation = Quaternion.identity;

            proj.gameObject.AddComponent<UIProjectileMover>()
                .Init(dir, projectileSpeed, lifetime);
        }
    }

    private bool IsPointerOverButton(Vector2 mouseScreen)
    {
        if (EventSystem.current == null || uiRaycaster == null)
            return false;

        var ped = new PointerEventData(EventSystem.current) { position = mouseScreen };
        var results = new List<RaycastResult>();
        uiRaycaster.Raycast(ped, results);

        foreach (var r in results)
        {
            // bloqueia apenas se for botão ou qualquer Selectable interativo
            var selectable = r.gameObject.GetComponent<Selectable>();
            if (selectable != null && selectable.IsActive() && selectable.interactable)
                return true;
        }

        return false;
    }

    private Vector2 WorldToLocalIn(RectTransform targetSpace, RectTransform rect)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(CamForUI, rect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetSpace, screen, CamForUI, out var local);
        return local;
    }
}

public class UIProjectileMover : MonoBehaviour
{
    private RectTransform _rt;
    private Vector2 _dir;
    private float _speed;
    private float _life;
    private float _t;

    public void Init(Vector2 dir, float speed, float lifetime)
    {
        _rt = transform as RectTransform;
        _dir = dir;
        _speed = speed;
        _life = lifetime;
        _t = 0f;
    }

    private void Update()
    {
        if (_rt == null) return;
        if (UIPauseController.IsPaused) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        _rt.anchoredPosition += _dir * _speed * dt;
        _t += dt;

        if (_t >= _life) Destroy(gameObject);
    }
}
