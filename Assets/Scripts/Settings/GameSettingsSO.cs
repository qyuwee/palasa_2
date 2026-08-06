using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings Asset")]
public class GameSettingsSO : ScriptableObject
{
    // Событие, вызываемое при любом изменении параметров
    public event Action OnSettingsChanged;

    [Header("Звук")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.8f;
    [SerializeField] private float audioOffset = 0f; // Задержка звука (специфично для ритм-игр)

    [Header("Графика")]
    [SerializeField] private bool isFullscreen = true;
    [SerializeField] private int qualityIndex = 2; // 0 - Low, 1 - Medium, 2 - High и т.д.

    // Публичные свойства только для чтения (Getter)
    public float MasterVolume => masterVolume;
    public float AudioOffset => audioOffset;
    public bool IsFullscreen => isFullscreen;
    public int QualityIndex => qualityIndex;

    // --- Методы изменения значений (Setter) ---

    public void SetMasterVolume(float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (Mathf.Approximately(masterVolume, clamped)) return;

        masterVolume = clamped;
        NotifySettingsChanged();
    }

    public void SetAudioOffset(float value)
    {
        if (Mathf.Approximately(audioOffset, value)) return;

        audioOffset = value;
        NotifySettingsChanged();
    }

    public void SetFullscreen(bool value)
    {
        if (isFullscreen == value) return;

        isFullscreen = value;
        NotifySettingsChanged();
    }

    public void SetQuality(int index)
    {
        if (qualityIndex == index) return;

        qualityIndex = index;
        NotifySettingsChanged();
    }

    // Вызов события для слушателей (AudioMixer, ScreenManager, SaveSystem)
    public void NotifySettingsChanged()
    {
        OnSettingsChanged?.Invoke();
    }
}