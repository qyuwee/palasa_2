using UnityEngine;
using UnityEngine.Audio;

public class SettingsApplier : MonoBehaviour
{
    [SerializeField] private GameSettingsSO settings;

    [Header("Настройки Аудио")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string volumeParamName = "MasterVolume"; // Название exposed-параметра в AudioMixer

    private void OnEnable()
    {
        if (settings != null)
            settings.OnSettingsChanged += ApplyAllSettings;
    }

    private void OnDisable()
    {
        if (settings != null)
            settings.OnSettingsChanged -= ApplyAllSettings;
    }

    private void Start()
    {
        ApplyAllSettings();
    }

    public void ApplyAllSettings()
    {
        if (settings == null) return;

        ApplyAudio();
        ApplyGraphics();
    }

    private void ApplyAudio()
    {
        if (mainMixer == null) return;

        float vol = settings.MasterVolume;
        // Переводим линейное значение (0.0..1.0) в логарифмические децибелы (-80dB..0dB)
        float db = vol > 0.0001f ? Mathf.Log10(vol) * 20f : -80f;

        mainMixer.SetFloat(volumeParamName, db);
    }

    private void ApplyGraphics()
    {
        Screen.fullScreen = settings.IsFullscreen;

        if (QualitySettings.GetQualityLevel() != settings.QualityIndex)
        {
            QualitySettings.SetQualityLevel(settings.QualityIndex, true);
        }
    }
}