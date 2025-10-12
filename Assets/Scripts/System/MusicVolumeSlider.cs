using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Liga um Slider ao volume do MusicController referenciado no Inspector.
/// </summary>
[RequireComponent(typeof(Slider))]
public class MusicVolumeSlider : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private MusicController musicController;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (_slider == null) _slider = GetComponent<Slider>();

        if (musicController == null)
        {
            Debug.LogWarning($"{nameof(MusicVolumeSlider)}: MusicController não atribuído no Inspector.");
            return;
        }

        // Valor inicial do slider reflete o volume atual da música
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        _slider.wholeNumbers = false;
        _slider.value = GetCurrentVolume();

        _slider.onValueChanged.AddListener(HandleVolumeChange);
    }

    private void OnDisable()
    {
        _slider?.onValueChanged.RemoveListener(HandleVolumeChange);
    }

    private void HandleVolumeChange(float value)
    {
        if (musicController != null)
            musicController.SetVolume(value);
    }

    // Usa o volume atual do AudioSource do MusicController como fonte da verdade
    private float GetCurrentVolume()
    {
        var src = musicController.GetComponent<AudioSource>();
        if (src != null) return src.volume;
        return 0.5f;
    }
}
