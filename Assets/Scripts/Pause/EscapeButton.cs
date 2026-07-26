using UnityEngine;
using UnityEngine.InputSystem;

public class EscapeButton : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject pausaUI;
    public GameObject pausaMenu;
    public GameObject settingsMenu;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource levelMusic; // Ссылка на музыку уровня

    public bool IsPaused
    {
        get
        {
            return pausaUI != null && pausaUI.activeSelf;
        }
    }

    private void Start()
    {
        if (pausaUI != null)
        {
            pausaUI.SetActive(false);
        }

        Time.timeScale = 1f;

        // Если забыл перетащить музыку в инспекторе, попробуем найти её сами
        if (levelMusic == null)
        {
            levelMusic = FindFirstObjectByType<AudioSource>();
        }
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

        // ИСПРАВЛЕНИЕ: Останавливаем трек, чтобы спавнер не сходил с ума, 
        // а ноты не улетали вперед во время паузы
        if (levelMusic != null && levelMusic.isPlaying)
        {
            levelMusic.Pause();
        }

        if (pausaUI != null)
            pausaUI.SetActive(true);

        if (pausaMenu != null)
            pausaMenu.SetActive(true);
        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        Debug.Log("Пауза включена, музыка на паузе");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        // ИСПРАВЛЕНИЕ: Возобновляем проигрывание трека ровно с того же места
        if (levelMusic != null)
        {
            levelMusic.UnPause();
        }

        if (pausaUI != null)
            pausaUI.SetActive(false);

        Debug.Log("Пауза выключена, музыка возобновлена");
    }

    public void ShowSettingsMenu()
    {
        if (pausaMenu != null)
            pausaMenu.SetActive(false);
        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        if (settingsMenu != null)
            settingsMenu.SetActive(false);
        if (pausaMenu != null)
            pausaMenu.SetActive(true);
    }
}