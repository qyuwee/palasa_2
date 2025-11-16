using UnityEngine;
using UnityEngine.InputSystem;

public class EscapeButton : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject pausaUI;          // Родительский объект Pausa_UI
    public GameObject pausaMenu;        // Основное меню pausa_menu
    public GameObject settingsMenu;     // Меню настроек settings_menu

    public bool IsPaused 
    { 
        get 
        { 
            return pausaUI != null && pausaUI.activeSelf; 
        } 
    }

    private void Start()
    {
        // Скрываем всю паузу при старте
        if (pausaUI != null)
        {
            pausaUI.SetActive(false);
        }
        
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        
        // Включаем родительский объект
        if (pausaUI != null)
            pausaUI.SetActive(true);
        
        // Показываем основное меню паузы, скрываем настройки
        if (pausaMenu != null)
            pausaMenu.SetActive(true);
        if (settingsMenu != null)
            settingsMenu.SetActive(false);
        
        Debug.Log("Пауза включена, показано основное меню");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        
        // Скрываем весь родительский объект паузы
        if (pausaUI != null)
            pausaUI.SetActive(false);
        
        Debug.Log("Пауза выключена");
    }

    // Метод для показа настроек
    public void ShowSettingsMenu()
    {
        if (pausaMenu != null)
            pausaMenu.SetActive(false);
        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }

    // Метод для скрытия настроек и возврата в основное меню
    public void BackToPauseMenu()
    {
        if (settingsMenu != null)
            settingsMenu.SetActive(false);
        if (pausaMenu != null)
            pausaMenu.SetActive(true);
    }
}