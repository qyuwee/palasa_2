using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboUIController : MonoBehaviour
{
    [Header("Настройки Восклицательных Знаков")]
    [SerializeField] private Image exclamationImage;
    [SerializeField] private Sprite vosklPerfect;
    [SerializeField] private Sprite vosklClear;
    [SerializeField] private Sprite vosklOk;

    [Header("Настройки Цифр (Спрайты от 0 до 9)")]
    [SerializeField] private Sprite[] digitSprites;

    [Header("Иерархия Интерфейса")]
    [SerializeField] private Transform digitsContainer;
    [SerializeField] private GameObject digitPrefab;

    private List<Image> digitImagesPool = new List<Image>();

    void OnEnable()
    {
        ScoreManager.OnComboChanged += UpdateComboUI;
    }

    void OnDisable()
    {
        ScoreManager.OnComboChanged -= UpdateComboUI;
    }

    void Start()
    {
        // ИСПРАВЛЕНИЕ: На старте уровня игрок чист, у него потенциальное Perfect Combo!
        if (exclamationImage != null && vosklPerfect != null)
        {
            exclamationImage.enabled = true;
            exclamationImage.sprite = vosklPerfect;
            exclamationImage.SetNativeSize();
        }

        UpdateDigitsDisplay(0);
    }

    private void UpdateComboUI(int combo, ScoreManager.RunStatus status)
    {
        // 1. УПРАВЛЕНИЕ ЗНАКОМ СТАТУСА ТЕКУЩЕГО ЗАБЕГА
        if (exclamationImage != null)
        {
            if (status == ScoreManager.RunStatus.None)
            {
                // Если full комбо запорото — гасим знак навсегда
                exclamationImage.enabled = false;
            }
            else
            {
                exclamationImage.enabled = true;

                // Меняем знак в зависимости от того, какое максимальное комбо ЕЩЕ возможно
                switch (status)
                {
                    case ScoreManager.RunStatus.PerfectCombo: exclamationImage.sprite = vosklPerfect; break;
                    case ScoreManager.RunStatus.ClearCombo: exclamationImage.sprite = vosklClear; break;
                    case ScoreManager.RunStatus.FullCombo: exclamationImage.sprite = vosklOk; break;
                }

                exclamationImage.SetNativeSize();
            }
        }

        // 2. УПРАВЛЕНИЕ ЦИФРАМИ (Отображают текущую серию подряд)
        UpdateDigitsDisplay(combo);
    }

    private void UpdateDigitsDisplay(int combo)
    {
        string comboStr = combo.ToString();

        while (digitImagesPool.Count < comboStr.Length)
        {
            GameObject newDigit = Instantiate(digitPrefab, digitsContainer);
            Image img = newDigit.GetComponent<Image>();
            if (img != null)
            {
                digitImagesPool.Add(img);
            }
        }

        for (int i = 0; i < digitImagesPool.Count; i++)
        {
            if (i < comboStr.Length)
            {
                digitImagesPool[i].gameObject.SetActive(true);
                int digitIndex = int.Parse(comboStr[i].ToString());
                digitImagesPool[i].sprite = digitSprites[digitIndex];
                digitImagesPool[i].SetNativeSize();
            }
            else
            {
                digitImagesPool[i].gameObject.SetActive(false);
            }
        }
    }
}