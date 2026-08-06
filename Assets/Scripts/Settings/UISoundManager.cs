using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [Header("Компоненты")]
    [SerializeField] private AudioSource audioSource;

    [Header("Звуки UI")]
    [SerializeField] private AudioClip defaultClickSound;

    private void Awake()
    {
        // Настройка Singleton для быстрого доступа из любого скрипта
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Автоматически добавляем AudioSource, если забыли назначить
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Важно: воспроизводить звук UI даже если игра на паузе
        audioSource.ignoreListenerPause = true;
    }

    /// <summary>
    /// Воспроизводит стандартный звук клика кнопки.
    /// </summary>
    public void PlayClick()
    {
        PlaySound(defaultClickSound);
    }

    /// <summary>
    /// Воспроизводит любой переданный аудиоклип.
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // PlayOneShot накладывает звуки друг на друга и не прерывается при закрытии UI
            audioSource.PlayOneShot(clip);
        }
    }
}