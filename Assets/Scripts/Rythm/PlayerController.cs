using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Скорости движения")]
    public float speed = 5f;

    [Header("Границы движения")]
    public float leftBound = -8f;
    public float rightBound = 8f;

    [Header("Настройки управления")]
    public Key leftKey = Key.S;
    public Key rightKey = Key.D;

    [Header("Настройки смены спрайта")]
    [SerializeField] private Sprite specialSprite;
    [SerializeField] private float changeDuration = 0.5f;

    [Header("Глобальные настройки разгона")]
    [SerializeField] private float rampUpDuration = 0.6f;
    [SerializeField] private float startSpeedMultiplier = 0.7f;
    [SerializeField] private float endSpeedMultiplier = 1.2f;

    [Header("Настройки оцепенения")]
    [SerializeField] private float freezeDuration = 0.2f;
    [SerializeField] private float turnBufferDuration = 0.06f;

    [Header("Настройки инерции")]
    [SerializeField] private float inertiaDuration = 0.05f;

    [Header("Защита от дребезга (с места)")]
    [SerializeField] private float staticInputBuffer = 0.04f; // Время ожидания второй клавиши (в сек.)

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioSource clickAudioSource; // Сюда перетащи первый AudioSource (без Loop)
    [SerializeField] private AudioSource slideAudioSource; // Сюда перетащи второй AudioSource (с Loop)
    [SerializeField] private AudioClip notePressedSound;   // Файл note_pressed.wav
    [SerializeField] private AudioClip slideSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;

    private Coroutine spriteCoroutine;

    [HideInInspector] public bool consumedHitThisFrame = false;
    private bool hasTriggeredBoth = false;
    private bool hasFrozenThisPress = false;
    private float freezeTimer = 0f;
    private float accelerationTimer = 0f;
    private float lastDesiredSpeed = 0f;
    private float currentVelocityX = 0f;
    private float dualKeyOverlapTimer = 0f;
    private float currentTurnaroundDuration = 0f;

    // Таймер задержки старта для фильтрации одиночного нажатия
    private float staticStartBufferTimer = 0f;

    private float justFrozenTimer = 0f;
    public bool JustFrozen => justFrozenTimer > 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalSprite = spriteRenderer.sprite;

        currentTurnaroundDuration = rampUpDuration;
    }

    void Update()
    {
        HandleMovement();
        ClampPosition();
    }

    void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool leftPressed = keyboard[leftKey].isPressed;
        bool rightPressed = keyboard[rightKey].isPressed;

        float targetSpeed = 0f;
        bool useBufferMovement = false;

        if (justFrozenTimer > 0f)
        {
            justFrozenTimer -= Time.deltaTime;
        }

        // --- ФАЗА 1: ОБРАБОТКА ДВУХ КЛАВИШ И ОКНА ПРОЩЕНИЯ ---
        if (leftPressed && rightPressed)
        {
            // ИСПРАВЛЕНИЕ: Если мы успели прожать комбо внутри стартового буфера покоя,
            // сбрасываем скорость в 0, чтобы предотвратить пре-движение!
            if (lastDesiredSpeed == 0f)
            {
                currentVelocityX = 0f;
            }

            bool wasMovingBefore = lastDesiredSpeed != 0f;

            if (!hasTriggeredBoth)
            {
                dualKeyOverlapTimer = 0f;
                hasTriggeredBoth = true;
                hasFrozenThisPress = false;
            }

            dualKeyOverlapTimer += Time.deltaTime;

            if (!hasFrozenThisPress)
            {
                if (wasMovingBefore && dualKeyOverlapTimer < turnBufferDuration)
                {
                    targetSpeed = lastDesiredSpeed;
                    useBufferMovement = true;
                }
                else
                {
                    freezeTimer = freezeDuration;
                    accelerationTimer = 0f;
                    currentTurnaroundDuration = rampUpDuration;
                    hasFrozenThisPress = true;

                    justFrozenTimer = 0.05f;
                    consumedHitThisFrame = false;

                    if (spriteCoroutine != null) StopCoroutine(spriteCoroutine);
                    spriteCoroutine = StartCoroutine(ChangeSpriteRoutine());
                }
            }
            else
            {
                targetSpeed = 0f;
                useBufferMovement = true;
            }
        }
        else
        {
            hasTriggeredBoth = false;
            hasFrozenThisPress = false;
            dualKeyOverlapTimer = 0f;
        }

        // Обработка активного оцепенения
        if (freezeTimer > 0f)
        {
            freezeTimer -= Time.deltaTime;
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, 0f, (speed / inertiaDuration) * Time.deltaTime);
            rb.linearVelocity = new Vector2(currentVelocityX, 0f);
            staticStartBufferTimer = 0f; // Сбрасываем буфер старта
            return;
        }

        // --- ФАЗА 2: ОПРЕДЕЛЕНИЕ НАПРАВЛЕНИЯ С ЗАЩИТОЙ ОТ ДРЕБЕНЗА ---
        if (!useBufferMovement)
        {
            bool wantsToMove = (leftPressed && !rightPressed) || (rightPressed && !leftPressed);

            if (wantsToMove)
            {
                // Если до этого мы никуда не ехали (стояли на месте)
                if (lastDesiredSpeed == 0f)
                {
                    // Накапливаем таймер ожидания второй кнопки
                    staticStartBufferTimer += Time.deltaTime;

                    if (staticStartBufferTimer < staticInputBuffer)
                    {
                        // Удерживаем шарик на месте, пока игрок прожимает комбо
                        targetSpeed = 0f;
                    }
                    else
                    {
                        // Время вышло, второй кнопки нет — погнали
                        if (leftPressed) targetSpeed = -speed;
                        if (rightPressed) targetSpeed = speed;
                    }
                }
                else
                {
                    // Если мы УЖЕ ехали, то задержка не нужна (это обычное удерживание)
                    if (leftPressed) targetSpeed = -speed;
                    if (rightPressed) targetSpeed = speed;
                }
            }
            else
            {
                // Кнопки отпущены — сбрасываем буфер
                staticStartBufferTimer = 0f;
                targetSpeed = 0f;
            }
        }

        // --- ФАЗА 3: РАЗГОН И ИНЕРЦИЯ ---
        if (targetSpeed != 0f)
        {
            // ДЕТЕКТ РАЗВОРOТА
            if (!useBufferMovement && Mathf.Sign(targetSpeed) != Mathf.Sign(lastDesiredSpeed) && lastDesiredSpeed != 0f)
            {
                currentTurnaroundDuration = rampUpDuration * 0.4f;
                float startProgress = (1.0f - startSpeedMultiplier) / (endSpeedMultiplier - startSpeedMultiplier);
                accelerationTimer = startProgress * currentTurnaroundDuration;
            }

            if (!useBufferMovement)
            {
                accelerationTimer += Time.deltaTime;
            }

            float progress = Mathf.Clamp01(accelerationTimer / currentTurnaroundDuration);
            float currentMultiplier = Mathf.Lerp(startSpeedMultiplier, endSpeedMultiplier, progress);

            currentVelocityX = targetSpeed * currentMultiplier;

            if (!useBufferMovement)
            {
                lastDesiredSpeed = targetSpeed;
            }
        }
        else
        {
            // Если мы стоим и ждем в буфере, то не сбрасываем lastDesiredSpeed в 0, 
            // чтобы система понимала, что мы инициализировали движение с нуля.
            if (staticStartBufferTimer == 0f)
            {
                accelerationTimer = 0f;
                lastDesiredSpeed = 0f;
                currentTurnaroundDuration = rampUpDuration;
            }

            float decelerationRate = speed / inertiaDuration;
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, 0f, decelerationRate * Time.deltaTime);
        }

        rb.linearVelocity = new Vector2(currentVelocityX, 0f);
        if (slideAudioSource != null && slideSound != null)
        {
            // Если игра на паузе, звук скольжения должен молчать
            if (Time.timeScale == 0f)
            {
                if (slideAudioSource.isPlaying)
                {
                    slideAudioSource.Pause();
                }
            }
            else
            {
                // Если ползунок движется (скорость по X заметно больше нуля)
                if (Mathf.Abs(currentVelocityX) > 0.1f)
                {
                    // Если звук еще не играет, запускаем его
                    if (!slideAudioSource.isPlaying)
                    {
                        slideAudioSource.clip = slideSound;
                        slideAudioSource.Play();
                    }
                    else
                    {
                        // Если звук был на паузе из-за Esc, снимаем с паузы
                        slideAudioSource.UnPause();
                    }
                }
                else
                {
                    // Если ползунок остановился — выключаем звук скольжения
                    if (slideAudioSource.isPlaying)
                    {
                        slideAudioSource.Stop();
                    }
                }
            }
        }
    }

    public void PlayHitSound()
    {
        if (clickAudioSource != null && notePressedSound != null)
        {
            clickAudioSource.PlayOneShot(notePressedSound);
        }
    }

    private void OnDisable()
    {
        if (slideAudioSource != null && slideAudioSource.isPlaying)
        {
            slideAudioSource.Stop();
        }
    }

    private IEnumerator ChangeSpriteRoutine()
    {
        if (spriteRenderer != null && specialSprite != null)
        {
            spriteRenderer.sprite = specialSprite;
        }
        yield return new WaitForSeconds(changeDuration);
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = originalSprite;
        }
        spriteCoroutine = null;
    }

    void ClampPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, leftBound, rightBound);
        transform.position = clampedPosition;
    }

    public void SetControlKeys(Key newLeft, Key newRight)
    {
        leftKey = newLeft;
        rightKey = newRight;
    }
}