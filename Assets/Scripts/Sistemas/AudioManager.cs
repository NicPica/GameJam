using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Gestor centralizado de audio del juego
/// Controla volúmenes, reproduce sonidos y gestiona la música
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [Tooltip("Audio Mixer principal del juego")]
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    [Tooltip("Audio Source para efectos de sonido")]
    public AudioSource sfxSource;

    [Tooltip("Audio Source para música")]
    public AudioSource musicSource;

    [Tooltip("Audio Source para voces/diálogos")]
    public AudioSource voiceSource;

    [Header("Default Volumes")]
    [Range(0f, 1f)]
    public float defaultMasterVolume = 0.8f;

    [Range(0f, 1f)]
    public float defaultSFXVolume = 1f;

    [Range(0f, 1f)]
    public float defaultMusicVolume = 0.7f;

    [Range(0f, 1f)]
    public float defaultVoiceVolume = 1f;

    // Nombres de los parámetros expuestos en el Audio Mixer
    private const string MASTER_VOLUME = "MasterVolume";
    private const string SFX_VOLUME = "SFXVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string VOICE_VOLUME = "VoiceVolume";

    // Singleton
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Crear AudioSources si no existen
        SetupAudioSources();

        // Cargar volúmenes guardados o usar defaults
        LoadVolumes();
    }

    /// <summary>
    /// Crea los AudioSources necesarios si no están asignados
    /// </summary>
    void SetupAudioSources()
    {
        // SFX Source
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        // Music Source
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Music_Source");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        // Voice Source
        if (voiceSource == null)
        {
            GameObject voiceObj = new GameObject("Voice_Source");
            voiceObj.transform.SetParent(transform);
            voiceSource = voiceObj.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
        }

        // Asignar outputs al mixer
        if (audioMixer != null)
        {
            var groups = audioMixer.FindMatchingGroups("SFX");
            if (groups.Length > 0)
                sfxSource.outputAudioMixerGroup = groups[0];

            groups = audioMixer.FindMatchingGroups("Music");
            if (groups.Length > 0)
                musicSource.outputAudioMixerGroup = groups[0];

            groups = audioMixer.FindMatchingGroups("Voice");
            if (groups.Length > 0)
                voiceSource.outputAudioMixerGroup = groups[0];
        }
    }

    #region Volume Control

    /// <summary>
    /// Establece el volumen master (0-1)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        float dbVolume = ConvertToDecibels(volume);
        audioMixer.SetFloat(MASTER_VOLUME, dbVolume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    /// <summary>
    /// Establece el volumen de SFX (0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        float dbVolume = ConvertToDecibels(volume);
        audioMixer.SetFloat(SFX_VOLUME, dbVolume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    /// <summary>
    /// Establece el volumen de música (0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        float dbVolume = ConvertToDecibels(volume);
        audioMixer.SetFloat(MUSIC_VOLUME, dbVolume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    /// <summary>
    /// Establece el volumen de voz (0-1)
    /// </summary>
    public void SetVoiceVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        float dbVolume = ConvertToDecibels(volume);
        audioMixer.SetFloat(VOICE_VOLUME, dbVolume);
        PlayerPrefs.SetFloat("VoiceVolume", volume);
    }

    /// <summary>
    /// Convierte volumen lineal (0-1) a decibelios (-80 a 0)
    /// </summary>
    float ConvertToDecibels(float volume)
    {
        if (volume <= 0f)
            return -80f;

        return Mathf.Log10(volume) * 20f;
    }

    /// <summary>
    /// Carga los volúmenes guardados o usa defaults
    /// </summary>
    void LoadVolumes()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        float voiceVol = PlayerPrefs.GetFloat("VoiceVolume", defaultVoiceVolume);

        SetMasterVolume(masterVol);
        SetSFXVolume(sfxVol);
        SetMusicVolume(musicVol);
        SetVoiceVolume(voiceVol);
    }

    #endregion

    #region Play Sounds

    /// <summary>
    /// Reproduce un efecto de sonido
    /// </summary>
    /// <summary>
    /// Reproduce un SFX con variación aleatoria de pitch para evitar repetitividad.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (clip == null) return;

        // Aplicamos el pitch aleatorio al source antes de disparar el sonido
        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(clip, volumeScale);

        // Opcional: Resetear el pitch a 1 para que otros sonidos no se vean afectados
        sfxSource.pitch = 1f; 
    }

    /// <summary>
    /// Reproduce un efecto de sonido en una posición 3D
    /// </summary>
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Intentando reproducir SFX nulo");
            return;
        }

        AudioSource.PlayClipAtPoint(clip, position, volumeScale);
    }

    /// <summary>
    /// Reproduce música de fondo
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Intentando reproducir música nula");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    /// <summary>
    /// Detiene la música
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Reproduce un clip de voz/diálogo
    /// </summary>
    public void PlayVoice(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Intentando reproducir voz nula");
            return;
        }

        voiceSource.PlayOneShot(clip, volumeScale);
    }

    /// <summary>
    /// Detiene la voz actual
    /// </summary>
    public void StopVoice()
    {
        voiceSource.Stop();
    }

    #endregion

    #region Getters

    /// <summary>
    /// Obtiene el volumen master actual (0-1)
    /// </summary>
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
    }

    /// <summary>
    /// Obtiene el volumen de SFX actual (0-1)
    /// </summary>
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
    }

    /// <summary>
    /// Obtiene el volumen de música actual (0-1)
    /// </summary>
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
    }

    /// <summary>
    /// Obtiene el volumen de voz actual (0-1)
    /// </summary>
    public float GetVoiceVolume()
    {
        return PlayerPrefs.GetFloat("VoiceVolume", defaultVoiceVolume);
    }

    #endregion
}