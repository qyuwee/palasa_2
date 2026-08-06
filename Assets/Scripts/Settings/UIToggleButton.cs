using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class UIToggleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Спрайты состояний")]
    [SerializeField] private Sprite offSprite;       // Выключена (мышцы нет)
    [SerializeField] private Sprite onSprite;        // Включена (мыши нет)
    [SerializeField] private Sprite hoverOffSprite; // Наведение на ВЫКЛЮЧЕННУЮ
    [SerializeField] private Sprite hoverOnSprite;  // Наведение на ВКЛЮЧЁННУЮ
    [SerializeField] private Sprite pressedSprite;  // Момент клика

    [Header("Текущее состояние")]
    [SerializeField] private bool isOn = false;

    [Header("События (Unity Event)")]
    // Динамическое событие, передающее значение true/false
    public UnityEvent<bool> onValueChanged;

    private Image targetImage;
    private Button button;
    private bool isHovered = false;

    // Геттер для чтения значения из других скриптов в коде
    public bool IsOn => isOn;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        button = GetComponent<Button>();

        // Отключаем стандартный Transition кнопки, так как спрайтами управляем сами
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(ToggleState);
    }

    private void Start()
    {
        UpdateVisuals();
    }

    // Переключение состояния на противоположное
    public void ToggleState()
    {
        SetState(!isOn);
    }

    // Принудительное изменение состояния (например, из настроек сохранения)
    public void SetState(bool value)
    {
        isOn = value;
        UpdateVisuals();

        // Оповещаем все привязанные методы о новом значении
        onValueChanged?.Invoke(isOn);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisuals();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisuals();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // При клике на короткое время ставится спрайт нажатия
        if (targetImage != null && pressedSprite != null)
        {
            targetImage.sprite = pressedSprite;
        }
    }

    private void UpdateVisuals()
    {
        if (targetImage == null) return;

        if (isHovered)
        {
            // Если мышь наведена, показываем соответствующий hover-спрайт
            targetImage.sprite = isOn ? hoverOnSprite : hoverOffSprite;
        }
        else
        {
            // Если мышь убрана, показываем базовый спрайт состояния
            targetImage.sprite = isOn ? onSprite : offSprite;
        }
    }
}