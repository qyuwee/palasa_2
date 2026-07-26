using UnityEngine;
using UnityEngine.InputSystem;

public class NoteController : MonoBehaviour
{
    public enum NoteType { Left_SD, Right_KL }

    [Header("Основные настройки ноты")]
    public NoteType noteType;
    [SerializeField] private float fadeSpeed = 5f;       // Скорость исчезновения при пропуске
    [SerializeField] private float appearanceSpeed = 8f; // Скорость появления ноты

    [Header("Время жизни ноты")]
    [SerializeField] private float lifetime = 2f;        // Сколько секунд нота ждет игрока после появления

    private bool isHovered = false;       // Наведен ли управляющий кружок
    private bool isFadingOut = false;     // Запущено ли плавное исчезновение (при пропуске)
    private bool isAppearing = true;      // Флаг плавного появления при старте
    private float currentAlpha = 0f;      // Текущая прозрачность
    private float lifetimeTimer = 0f;     // Таймер времени жизни

    // Кэш для компонентов
    private SpriteRenderer[] childRenderers;
    private ParticleSystem[] childParticles;
    private float[] maxEmissionRates;
    private Collider2D mainCollider;

    // Кэш для оптимизации: Ссылка на конкретный ползунок для этой ноты
    private PlayerController targetSlider;

    void Start()
    {
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
        childParticles = GetComponentsInChildren<ParticleSystem>();
        mainCollider = GetComponent<Collider2D>();

        if (childParticles != null)
        {
            maxEmissionRates = new float[childParticles.Length];
            for (int i = 0; i < childParticles.Length; i++)
            {
                maxEmissionRates[i] = childParticles[i].emission.rateOverTime.constant;
            }
        }

        ApplyAlpha(currentAlpha);

        // Оптимизация: Ищем ползунок строго один раз при создании ноты
        CacheTargetSlider();
    }

    void Update()
    {
        // 1. ФАЗА ПОЯВЛЕНИЯ
        if (isAppearing)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, 1f, appearanceSpeed * Time.deltaTime);
            ApplyAlpha(currentAlpha);

            if (currentAlpha >= 1f)
            {
                isAppearing = false;
                lifetimeTimer = lifetime;
            }
            return;
        }

        // 2. ИГРОВАЯ ФАЗА
        if (!isFadingOut)
        {
            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0f)
            {
                Miss();
                return;
            }

            // Проверяем ввод напрямую через закешированную ссылку
            if (isHovered && CheckInput())
            {
                Hit();
                return;
            }
        }

        // 3. ФАЗА ИСЧЕЗНОВЕНИЯ
        if (isFadingOut)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, fadeSpeed * Time.deltaTime);
            ApplyAlpha(currentAlpha);

            if (currentAlpha <= 0.02f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void CacheTargetSlider()
    {
        PlayerController[] allSliders = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var slider in allSliders)
        {
            if (noteType == NoteType.Left_SD && (slider.leftKey == Key.S || slider.rightKey == Key.D))
            {
                targetSlider = slider;
                break;
            }
            if (noteType == NoteType.Right_KL && (slider.leftKey == Key.K || slider.rightKey == Key.L))
            {
                targetSlider = slider;
                break;
            }
        }

        if (targetSlider == null)
        {
            Debug.LogWarning($"<color=yellow>[WARN]</color> Ползунок для ноты {noteType} не найден в сцене при старте!");
        }
    }

    private bool CheckInput()
    {
        if (targetSlider == null) return false;

        // Если ползунок не в состоянии триггера хита ИЛИ хит уже был поглощен другой нотой в этом кадре
        if (!targetSlider.JustFrozen || targetSlider.consumedHitThisFrame) return false;

        // --- ДИСТАНЦИОННЫЙ АРБИТРАЖ ---
        // Находим все ноты на сцене, чтобы определить, кто ближе всех к ползунку
        NoteController[] allNotes = Object.FindObjectsByType<NoteController>(FindObjectsSortMode.None);
        NoteController closestNote = null;
        float minDistanceX = float.MaxValue;

        foreach (var note in allNotes)
        {
            // Рассматриваем только живые, наведенные ноты ТОГО ЖЕ ТИПА управления
            if (note.isHovered && note.noteType == this.noteType && !note.isFadingOut)
            {
                float distX = Mathf.Abs(note.transform.position.x - targetSlider.transform.position.x);
                if (distX < minDistanceX)
                {
                    minDistanceX = distX;
                    closestNote = note;
                }
            }
        }

        // Если самая близкая к центру ползунка нота — это НЕ мы, то блокируем нажатие для себя
        if (closestNote != this) return false;

        return true;
    }

    public void Hit()
    {
        if (targetSlider != null)
        {
            targetSlider.consumedHitThisFrame = true;

            // Воспроизводим звук клика через закешированный ползунок!
            targetSlider.PlayHitSound();

            int distanceX = Mathf.RoundToInt(Mathf.Abs(transform.position.x - targetSlider.transform.position.x) * 1000);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RegisterHit(distanceX);
            }
        }

        Destroy(gameObject);
    }

    private void Miss()
    {
        // ОТПРАВЛЯЕМ СИГНАЛ О ПРОПУСКЕ В МЕНЕДЖЕР ОЧКОВ
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterMiss();
        }
        else
        {
            Debug.Log($"<color=red>[ПРОПУСК]</color> Время жизни ноты {noteType} истекло.");
        }

        isFadingOut = true;
        isHovered = false;

        if (mainCollider != null) mainCollider.enabled = false;
    }

    public void OnAnimationFinished() { }

    private void ApplyAlpha(float alpha)
    {
        if (childRenderers == null) return;
        foreach (var renderer in childRenderers)
        {
            if (renderer != null)
            {
                Color c = renderer.color;
                c.a = alpha;
                renderer.color = c;
            }
        }

        if (childParticles == null) return;
        for (int i = 0; i < childParticles.Length; i++)
        {
            if (childParticles[i] != null)
            {
                var emission = childParticles[i].emission;
                if (maxEmissionRates != null && i < maxEmissionRates.Length)
                {
                    emission.rateOverTime = maxEmissionRates[i] * alpha;
                }

                var main = childParticles[i].main;
                Color particleColor = main.startColor.color;
                particleColor.a = alpha;
                main.startColor = particleColor;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HitZone zone = collision.GetComponent<HitZone>();
        if (zone != null && zone.zoneType == (HitZone.ZoneType)noteType)
        {
            isHovered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        HitZone zone = collision.GetComponent<HitZone>();
        if (zone != null && zone.zoneType == (HitZone.ZoneType)noteType)
        {
            isHovered = false;
        }
    }
}