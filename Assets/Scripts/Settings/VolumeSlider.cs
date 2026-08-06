using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSlider : MonoBehaviour
{
    [Header("Компоненты UI")]
    [SerializeField] private Image fillImage; // Сюда перетащи Volume_Fill (спрайт 100%)

    [Header("Настройки Аудио")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParamName = "MasterVolume"; // Имя из AudioMixer

    [Header("Значение")]
    [Range(0f, 1f)]
    [SerializeField] private float currentValue = 1f; // Значение от 0.0 до 1.0

    private void Start()
    {
        // Загружаем сохраненное значение громкости (если есть) или ставим 1.0 по умолчанию
        currentValue = PlayerPrefs.GetFloat(exposedParamName, 1f);
        UpdateVolume(currentValue);
    }

    /// <summary>
    /// Вызывается при изменении громкости (например, при клике/стрелках)
    /// </summary>
    /// <param name="value">Значение от 0.0 до 1.0</param>
    public void UpdateVolume(float value)
    {
        currentValue = Mathf.Clamp01(value);

        // 1. Обновляем визуальное заполнение спрайта эквалайзера (от 0 до 1)
        if (fillImage != null)
        {
            fillImage.fillAmount = currentValue;
        }

        // 2. Пересчитываем линейный 0..1 в логарифмические Децибелы (-80dB .. 0dB)
        float dbValue;
        if (currentValue <= 0.0001f)
        {
            dbValue = -80f; // Полный мут при 0%
        }
        else
        {
            dbValue = Mathf.Log10(currentValue) * 20f; // Перевод в логарифм
        }

        // 3. Отправляем значение в AudioMixer
        if (audioMixer != null)
        {
            audioMixer.SetFloat(exposedParamName, dbValue);
        }

        // 4. Сохраняем настройку
        PlayerPrefs.SetFloat(exposedParamName, currentValue);
        PlayerPrefs.Save();
    }

    // Вспомогательные методы для кнопок "+" и "-" или стрелок
    public void IncreaseVolume(float step = 0.1f) => UpdateVolume(currentValue + step);
    public void DecreaseVolume(float step = 0.1f) => UpdateVolume(currentValue - step);
}