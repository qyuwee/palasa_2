using UnityEngine;

public class SettingsSaveSystem : MonoBehaviour
{
    [SerializeField] private GameSettingsSO settings;

    private const string KeyVolume = "Settings_MasterVolume";
    private const string KeyOffset = "Settings_AudioOffset";
    private const string KeyFullscreen = "Settings_Fullscreen";
    private const string KeyQuality = "Settings_QualityIndex";

    private void Awake()
    {
        LoadSettings();
    }

    private void OnEnable()
    {
        if (settings != null)
            settings.OnSettingsChanged += SaveSettings;
    }

    private void OnDisable()
    {
        if (settings != null)
            settings.OnSettingsChanged -= SaveSettings;
    }

    public void LoadSettings()
    {
        if (settings == null) return;

        // Считываем значения с дефолтными фолбеками
        float vol = PlayerPrefs.GetFloat(KeyVolume, 0.8f);
        float offset = PlayerPrefs.GetFloat(KeyOffset, 0f);
        bool full = PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;
        int quality = PlayerPrefs.GetInt(KeyQuality, 2);

        // Передаем значения в ScriptableObject
        settings.SetMasterVolume(vol);
        settings.SetAudioOffset(offset);
        settings.SetFullscreen(full);
        settings.SetQuality(quality);

        // Принудительно уведомляем систему после полной загрузки
        settings.NotifySettingsChanged();
    }

    public void SaveSettings()
    {
        if (settings == null) return;

        PlayerPrefs.SetFloat(KeyVolume, settings.MasterVolume);
        PlayerPrefs.SetFloat(KeyOffset, settings.AudioOffset);
        PlayerPrefs.SetInt(KeyFullscreen, settings.IsFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KeyQuality, settings.QualityIndex);

        PlayerPrefs.Save();
    }
}