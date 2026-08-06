using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomDropdown : MonoBehaviour
{
    [Header("Главная кнопка")]
    [SerializeField] private Button mainButton;
    [SerializeField] private TextMeshProUGUI mainText; // Текст на главной кнопке

    [Header("Панель и Блокировщик")]
    [SerializeField] private GameObject dropdownPanel;
    [SerializeField] private Button clickBlocker;

    [Header("Список кнопок в меню")]
    [SerializeField] private Button[] optionButtons; // Список всех кнопок-опций (1, 2, 3)

    // Событие, если нужно передать выбранный индекс (0, 1, 2) в настройки
    public System.Action<int, string> onOptionChanged;

    private void Awake()
    {
        // 1. Привязываем открытие/закрытие к главной кнопке и блокеру
        if (mainButton != null)
            mainButton.onClick.AddListener(ToggleDropdown);

        if (clickBlocker != null)
            clickBlocker.onClick.AddListener(CloseDropdown);

        // 2. Автоматически настраиваем каждый пункт списка
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // Локальная копия индекса для лямбды
            Button btn = optionButtons[i];

            if (btn != null)
            {
                btn.onClick.AddListener(() => OnOptionClicked(btn, index));
            }
        }

        CloseDropdown();
    }

    private void OnDisable()
    {
        CloseDropdown();
    }

    public void ToggleDropdown()
    {
        if (dropdownPanel.activeSelf)
            CloseDropdown();
        else
            OpenDropdown();
    }

    public void OpenDropdown()
    {
        if (dropdownPanel != null) dropdownPanel.SetActive(true);
        if (clickBlocker != null) clickBlocker.gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void CloseDropdown()
    {
        if (dropdownPanel != null) dropdownPanel.SetActive(false);
        if (clickBlocker != null) clickBlocker.gameObject.SetActive(false);
    }

    // Программный выбор опции (вызывается из SettingsUI при загрузке/открытии панели)
    public void SelectOption(int index)
    {
        if (optionButtons == null || index < 0 || index >= optionButtons.Length)
            return;

        Button targetButton = optionButtons[index];
        if (targetButton == null) return;

        TextMeshProUGUI optionText = targetButton.GetComponentInChildren<TextMeshProUGUI>();

        if (optionText != null && mainText != null)
        {
            mainText.text = optionText.text;
        }
    }

    // Вызывается при нажатии игрока на любую из кнопок списка
    private void OnOptionClicked(Button clickedButton, int index)
    {
        // 1. Ищем текст внутри нажатой кнопки
        TextMeshProUGUI optionText = clickedButton.GetComponentInChildren<TextMeshProUGUI>();

        if (optionText != null && mainText != null)
        {
            // Копируем текст из нажатой кнопки в главную кнопку
            mainText.text = optionText.text;
        }

        // 2. Закрываем выпадающее меню и заслон
        CloseDropdown();

        // 3. Вызываем событие (для логики настроек)
        onOptionChanged?.Invoke(index, mainText.text);

        Debug.Log($"Выбран пункт #{index} с текстом: {mainText.text}");
    }
}