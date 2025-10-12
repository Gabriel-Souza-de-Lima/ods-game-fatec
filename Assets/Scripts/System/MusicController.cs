using System.Collections;
using UnityEngine;

/// <summary>
/// Controla a música de fundo da cena: toca um AudioClip em loop com volume ajustável
/// e fades opcionais. Um por cena.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    [Header("Música")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    [Header("Fade (opcional)")]
    [Tooltip("Duração em segundos para entrar com fade ao iniciar a música (0 = sem fade).")]
    [SerializeField, Min(0f)] private float fadeInSeconds = 0f;

    private AudioSource _source;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = loop;
        _source.clip = musicClip;
        _source.volume = volume;
    }

    private void Start()
    {
        if (playOnStart)
        {
            if (fadeInSeconds > 0f) PlayWithFadeIn(fadeInSeconds);
            else Play();
        }
    }

    /// <summary>Toca a música imediatamente no volume definido.</summary>
    public void Play()
    {
        if (!_source || !musicClip) return;
        StopFade();
        _source.volume = volume;
        if (_source.clip != musicClip) _source.clip = musicClip;
        _source.loop = loop;
        if (!_source.isPlaying) _source.Play();
    }

    /// <summary>Para a música imediatamente.</summary>
    public void Stop()
    {
        if (!_source) return;
        StopFade();
        _source.Stop();
    }

    /// <summary>Inicia a música com fade-in.</summary>
    public void PlayWithFadeIn(float seconds)
    {
        if (!_source || !musicClip) return;
        StopFade();
        if (_source.clip != musicClip) _source.clip = musicClip;
        _source.loop = loop;
        _source.volume = 0f;
        _source.Play();
        _fadeRoutine = StartCoroutine(FadeTo(volume, seconds));
    }

    /// <summary>Faz fade-out e para.</summary>
    public void StopWithFadeOut(float seconds)
    {
        if (!_source || !_source.isPlaying) return;
        StopFade();
        _fadeRoutine = StartCoroutine(FadeTo(0f, seconds, stopAfterFade: true));
    }

    /// <summary>Altera o volume alvo (aplicado imediatamente).</summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (_source) _source.volume = volume;
    }

    /// <summary>Troca a música em tempo real (opcionalmente com fade).</summary>
    public void SwitchMusic(AudioClip newClip, bool withFade = true, float fadeSeconds = 0.5f)
    {
        if (newClip == null) return;
        if (!_source) return;

        if (withFade && _source.isPlaying && fadeSeconds > 0f)
        {
            StopFade();
            _fadeRoutine = StartCoroutine(SwitchWithFadeCoroutine(newClip, fadeSeconds));
        }
        else
        {
            _source.clip = newClip;
            _source.volume = volume;
            _source.loop = loop;
            _source.Play();
        }
        musicClip = newClip;
    }

    // ----------------- Helpers -----------------
    private IEnumerator FadeTo(float target, float seconds, bool stopAfterFade = false)
    {
        if (seconds <= 0f)
        {
            _source.volume = target;
            if (stopAfterFade) _source.Stop();
            yield break;
        }

        float start = _source.volume;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / seconds; // usa unscaled para funcionar com pause baseado em timeScale
            _source.volume = Mathf.Lerp(start, target, t);
            yield return null;
        }
        _source.volume = target;
        if (stopAfterFade) _source.Stop();
        _fadeRoutine = null;
    }

    private IEnumerator SwitchWithFadeCoroutine(AudioClip newClip, float seconds)
    {
        // fade out atual
        yield return FadeTo(0f, seconds);
        _source.clip = newClip;
        _source.loop = loop;
        _source.Play();
        // fade in novo
        yield return FadeTo(volume, seconds);
        _fadeRoutine = null;
    }

    private void StopFade()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }
    }
}
