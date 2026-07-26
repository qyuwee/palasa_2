using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class MenuButtonUI : MonoBehaviour
{
    [Header("Настройки UI")]
    public GameObject panelToShow;
    public GameObject[] panelsToHide;
    public bool togglePanel = false;

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioClip clickSound;

    private Button button;
    private AudioSource audioSource;

    private void Awake()
    {
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }

        // Подписываемся на стандартный клик кнопки Unity UI
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // 1. Проигрываем звук
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // 2. Управляем панелями
        if (togglePanel && panelToShow != null)
        {
            panelToShow.SetActive(!panelToShow.activeSelf);
        }
        else
        {
            if (panelsToHide != null)
            {
                foreach (GameObject panel in panelsToHide)
                {
                    if (panel != null) panel.SetActive(false);
                }
            }

            if (panelToShow != null)
                panelToShow.SetActive(true);
        }
    }
}