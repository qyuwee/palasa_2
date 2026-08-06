using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Ресурс данных настроек")]
    [SerializeField] private GameSettingsSO settings;

    [Header("Громкость (+ / - и Заполнение)")]
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private Image volumeFillImage;
    [SerializeField] private float volumeStep = 0.1f;

    [Header("Переключатель (Вкл / Выкл)")]
    [SerializeField] private UIToggleButton toggleButton;

    [Header("Выпадающее меню")]
    [SerializeField] private CustomDropdown customDropdown;

    private void OnEnable()
    {
        // При каждом открытии панели принудительно синхронизируем визуал всех UI-элементов
        UpdateAllVisuals();
    }

    private void Start()
    {
        // Подписываемся на события кнопок и UI-элементов
        if (plusButton != null)
            plusButton.onClick.AddListener(IncreaseVolume);

        if (minusButton != null)
            minusButton.onClick.AddListener(DecreaseVolume);

        if (toggleButton != null)
            toggleButton.onValueChanged.AddListener(OnToggleChanged);

        if (customDropdown != null)
            customDropdown.onOptionChanged += OnDropdownOptionChanged;
    }

    private void OnDestroy()
    {
        // Отписываемся от событий во избежание утечек памяти
        if (customDropdown != null)
            customDropdown.onOptionChanged -= OnDropdownOptionChanged;
    }

    // --- Синхронизация UI с данными из GameSettingsSO ---
    public void UpdateAllVisuals()
    {
        if (settings == null) return;

        // 1. Полоска громкости
        if (volumeFillImage != null)
            volumeFillImage.fillAmount = settings.MasterVolume;

        // 2. Переключатель (Fullscreen)
        if (toggleButton != null)
            toggleButton.SetState(settings.IsFullscreen);

        // 3. Выпадающее меню (Качество)
        if (customDropdown != null)
            customDropdown.SelectOption(settings.QualityIndex); // Убедись, что в CustomDropdown есть метод установки индекса
    }

    // --- Обработчики действий игрока ---
    private void IncreaseVolume()
    {
        if (settings == null) return;
        settings.SetMasterVolume(settings.MasterVolume + volumeStep);
        if (volumeFillImage != null)
            volumeFillImage.fillAmount = settings.MasterVolume;
    }

    private void DecreaseVolume()
    {
        if (settings == null) return;
        settings.SetMasterVolume(settings.MasterVolume - volumeStep);
        if (volumeFillImage != null)
            volumeFillImage.fillAmount = settings.MasterVolume;
    }

    private void OnToggleChanged(bool isOn)
    {
        if (settings == null) return;
        settings.SetFullscreen(isOn);
    }

    private void OnDropdownOptionChanged(int index, string optionName)
    {
        if (settings == null) return;
        settings.SetQuality(index);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            gameObject.SetActive(false);
        }
    }
}