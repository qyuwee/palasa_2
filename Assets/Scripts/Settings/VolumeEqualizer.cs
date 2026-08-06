using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

// Интерфейсы IPointerDownHandler и IDragHandler отслеживают клик и перетаскивание мыши
public class VolumeEqualizer : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI компоненты")]
    [SerializeField] private Image fillImage; // Сюда перетащи Volume_Fill (спрайт 100%)

    [Header("Настройки микшера")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParamName = "MasterVolume";

    [Header("Шаг изменения кнопками (5% = 0.05)")]
    [SerializeField] private float step = 0.05f;

    [Range(0f, 1f)]
    [SerializeField] private float currentValue = 1f;

    private RectTransform rectTransform;

    private void Awake()
    {
        // Кешируем RectTransform объекта шкалы, по которому кликает мышь
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Загружаем сохраненное значение громкости (по умолчанию 100%)
        currentValue = PlayerPrefs.GetFloat(exposedParamName, 1f);
        ApplyVolume(currentValue);
    }

    // --- ОБРАБОТКА МЫШИ (Клик и Зажатие/Таскание) ---

    public void OnPointerDown(PointerEventData eventData)
    {
        HandleMouseInput(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandleMouseInput(eventData);
    }

    private void HandleMouseInput(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        // Переводим координаты клика мыши в локальные координаты объекта шкалы
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            float width = rectTransform.rect.width;
            if (width <= 0f) return;

            // Вычисляем процент заполнения от 0.0 до 1.0 в зависимости от X-координаты мыши
            float normalizedX = (localPoint.x - rectTransform.rect.xMin) / width;
            ApplyVolume(normalizedX);
        }
    }

    // --- МЕТОДЫ ДЛЯ КНОПОК "+" И "-" (Шаг 5%) ---

    public void IncreaseVolume()
    {
        ApplyVolume(currentValue + step);
    }

    public void DecreaseVolume()
    {
        ApplyVolume(currentValue - step);
    }

    // --- ОСНОВНАЯ ЛОГИКА И ИЗМЕНЕНИЕ ЗВУКА ---

    private void ApplyVolume(float value)
    {
        // Ограничиваем значение строго в пределах от 0.0 до 1.0
        currentValue = Mathf.Clamp01(value);

        // 1. Обновляем визуальное заполнение полосы эквалайзера
        if (fillImage != null)
        {
            fillImage.fillAmount = currentValue;
        }

        // 2. Логарифмический пересчет в Децибелы (-80 dB .. 0 dB)
        float dbValue;
        if (currentValue <= 0.0001f)
        {
            dbValue = -80f; // Полный мут
        }
        else
        {
            dbValue = Mathf.Log10(currentValue) * 20f;
        }

        if (audioMixer != null)
        {
            audioMixer.SetFloat(exposedParamName, dbValue);
        }

        // 3. Сохраняем настройку
        PlayerPrefs.SetFloat(exposedParamName, currentValue);
        PlayerPrefs.Save();
    }
}