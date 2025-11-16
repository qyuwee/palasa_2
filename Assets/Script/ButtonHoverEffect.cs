using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    [Header("Sprite Settings")]
    public SpriteRenderer normalSprite;
    public SpriteRenderer hoverSprite;
    public SpriteRenderer clickSprite;

    [Header("UI Settings")]
    public GameObject panelToShow;           // Панель которую показать
    public GameObject[] panelsToHide;        // Массив панелей которые скрыть
    public bool togglePanel = false;         // Переключать панель вместо замены

    private Mouse mouse;
    private bool isMouseOver = false;
    private bool isClicked = false;

    private void Start()
    {
        if (normalSprite == null)
            normalSprite = transform.Find("play")?.GetComponent<SpriteRenderer>();
        
        if (hoverSprite == null)
            hoverSprite = transform.Find("play_p")?.GetComponent<SpriteRenderer>();
        
        if (clickSprite == null)
            clickSprite = transform.Find("play_c")?.GetComponent<SpriteRenderer>();

        UpdateSpriteStates();
        mouse = Mouse.current;
    }

    private void Update()
    {
        if (mouse == null) return;
        
        Vector2 mousePosition = mouse.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        
        bool nowMouseOver = (hit.collider != null && hit.collider.gameObject == gameObject);
        
        if (nowMouseOver && !isMouseOver)
        {
            OnMouseEnter();
        }
        else if (!nowMouseOver && isMouseOver)
        {
            OnMouseExit();
        }
        
        // Обработка нажатия ЛКМ
        if (nowMouseOver && mouse.leftButton.wasPressedThisFrame && !isClicked)
        {
            OnMouseClick();
        }
        
        // Обработка ОТПУСКАНИЯ ЛКМ - ВЫПОЛНЯЕМ ДЕЙСТВИЕ ЗДЕСЬ
        if (isClicked && mouse.leftButton.wasReleasedThisFrame)
        {
            OnMouseRelease();
            HandleButtonAction(); // Действие при отпускании
        }
        
        isMouseOver = nowMouseOver;
    }

    private void OnMouseEnter()
    {
        if (!isClicked) ChangeToHoverState();
    }

    private void OnMouseExit()
    {
        if (!isClicked) ChangeToNormalState();
    }

    private void OnMouseClick()
    {
        isClicked = true;
        ChangeToClickState();
    }

    private void OnMouseRelease()
    {
        isClicked = false;
        if (isMouseOver) ChangeToHoverState();
        else ChangeToNormalState();
    }

    private void HandleButtonAction()
    {
        if (togglePanel && panelToShow != null)
        {
            // Переключаем видимость панели
            panelToShow.SetActive(!panelToShow.activeSelf);
        }
        else
        {
            // Скрываем ВСЕ указанные панели
            if (panelsToHide != null)
            {
                foreach (GameObject panel in panelsToHide)
                {
                    if (panel != null)
                        panel.SetActive(false);
                }
            }
            
            // Показываем нужную панель
            if (panelToShow != null)
                panelToShow.SetActive(true);
        }
    }

    private void ChangeToHoverState()
    {
        SetSpriteState(false, true, false);
    }

    private void ChangeToNormalState()
    {
        SetSpriteState(true, false, false);
    }

    private void ChangeToClickState()
    {
        SetSpriteState(false, false, true);
    }

    private void SetSpriteState(bool normal, bool hover, bool click)
    {
        if (normalSprite != null) normalSprite.enabled = normal;
        if (hoverSprite != null) hoverSprite.enabled = hover;
        if (clickSprite != null) clickSprite.enabled = click;
    }

    private void UpdateSpriteStates()
    {
        SetSpriteState(true, false, false);
    }
}