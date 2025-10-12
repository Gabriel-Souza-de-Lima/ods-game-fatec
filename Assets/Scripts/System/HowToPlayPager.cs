using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla a navegação entre subpainéis do "Como Jogar".
/// Cada clique em Next() desativa o painel atual e ativa o próximo da lista.
/// </summary>
public class HowToPlayPager : MonoBehaviour
{
    [Header("Onde estão os subpainéis (na ordem do Hierarchy)")]
    [SerializeField] private Transform pagesParent;

    [Tooltip("Se vazio, os filhos diretos de 'pagesParent' serão usados na ordem do Hierarchy.")]
    [SerializeField] private List<GameObject> pages = new List<GameObject>();

    [Header("Config")]
    [SerializeField] private int startIndex = 0;       // normalmente 0
    [SerializeField] private bool autoCollectChildren = true; // coleta filhos de pagesParent se 'pages' estiver vazia

    private int _index = 0;

    private void OnEnable()
    {
        BuildPagesIfNeeded();
        Show(startIndex);
    }

    /// <summary>Vai para o próximo painel. Se já estiver no último, não faz nada.</summary>
    public void Next()
    {
        if (pages.Count == 0) return;
        if (_index >= pages.Count - 1) return;

        SetActive(_index, false);
        _index++;
        SetActive(_index, true);
    }

    /// <summary>Volta um painel (opcional, use se quiser um botão Voltar).</summary>
    public void Prev()
    {
        if (pages.Count == 0) return;
        if (_index <= 0) return;

        SetActive(_index, false);
        _index--;
        SetActive(_index, true);
    }

    /// <summary>Exibe apenas o painel no índice informado.</summary>
    public void Show(int index)
    {
        if (pages.Count == 0) return;

        _index = Mathf.Clamp(index, 0, pages.Count - 1);
        for (int i = 0; i < pages.Count; i++)
            SetActive(i, i == _index);
    }

    private void BuildPagesIfNeeded()
    {
        if (pagesParent == null) pagesParent = transform;

        if ((pages == null || pages.Count == 0) && autoCollectChildren)
        {
            pages = new List<GameObject>(pagesParent.childCount);
            for (int i = 0; i < pagesParent.childCount; i++)
            {
                var child = pagesParent.GetChild(i).gameObject;
                pages.Add(child);
            }
        }
    }

    private void SetActive(int i, bool state)
    {
        if (i < 0 || i >= pages.Count) return;
        if (pages[i] != null) pages[i].SetActive(state);
    }
}
