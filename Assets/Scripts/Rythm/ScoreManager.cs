using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public enum HitGrade { Perfect, Clear, Ok, Miss }

    // --- НОВЫЙ ENUM ДЛЯ СТАТУСА ЗАБЕГА ---
    public enum RunStatus { PerfectCombo, ClearCombo, FullCombo, None }

    // Передаем текущее комбо и глобальный статус забега
    public static System.Action<int, RunStatus> OnComboChanged;

    [Header("Текущая статистика")]
    public int currentCombo = 0;
    public int maxCombo = 0;

    private int perfectCount = 0;
    private int clearCount = 0;
    private int okCount = 0;
    private int missCount = 0;

    private bool brokenPerfectCombo = false;
    private bool brokenClearCombo = false;
    private bool brokenFullCombo = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterHit(int distanceX)
    {
        HitGrade grade;

        if (distanceX <= 110)
        {
            grade = HitGrade.Perfect;
            perfectCount++;
        }
        else if (distanceX <= 220)
        {
            grade = HitGrade.Clear;
            clearCount++;
            brokenPerfectCombo = true; // Роняем планку до Clear Combo
        }
        else
        {
            grade = HitGrade.Ok;
            okCount++;
            brokenPerfectCombo = true;
            brokenClearCombo = true;   // Роняем планку до Full Combo
        }

        currentCombo++;
        if (currentCombo > maxCombo) maxCombo = currentCombo;

        LogResult(grade, distanceX);

        // Публикуем обновление для UI
        OnComboChanged?.Invoke(currentCombo, CalculateCurrentRunStatus());
    }

    public void RegisterMiss()
    {
        missCount++;

        // Мисс уничтожает вообще все шансы на комбо-титулы
        brokenPerfectCombo = true;
        brokenClearCombo = true;
        brokenFullCombo = true;

        currentCombo = 0;

        LogResult(HitGrade.Miss, -1);

        // Публикуем обновление для UI
        OnComboChanged?.Invoke(currentCombo, CalculateCurrentRunStatus());
    }

    // Вспомогательный метод: вычисляет, на какой титул игрок ВСЁ ЕЩЕ претендует
    private RunStatus CalculateCurrentRunStatus()
    {
        if (!brokenPerfectCombo) return RunStatus.PerfectCombo;
        if (!brokenClearCombo) return RunStatus.ClearCombo;
        if (!brokenFullCombo) return RunStatus.FullCombo;
        return RunStatus.None;
    }

    [ContextMenu("Вывести оценку уровня")]
    public void EvaluateLevelResult()
    {
        string finalComboTitle = "No Combo";
        RunStatus finalStatus = CalculateCurrentRunStatus();

        switch (finalStatus)
        {
            case RunStatus.PerfectCombo: finalComboTitle = "<color=gold>✨ PERFECT COMBO ✨</color>"; break;
            case RunStatus.ClearCombo: finalComboTitle = "<color=orange>⭐ CLEAR COMBO ⭐</color>"; break;
            case RunStatus.FullCombo: finalComboTitle = "<color=green>⚡ FULL COMBO ⚡</color>"; break;
            case RunStatus.None: finalComboTitle = $"Комбо-отрезок (Max Streak): {maxCombo}"; break;
        }

        Debug.Log($"\n================ ИТОГИ УРОВНЯ ================\n" +
                  $"Результат: <b>{finalComboTitle}</b>\n" +
                  $"Наибольшая серия (Max Combo): {maxCombo}\n" +
                  $"---------------------------------------------\n" +
                  $"Perfect: {perfectCount} | Clear: {clearCount} | Ok: {okCount} | Miss: {missCount}\n" +
                  $"=============================================");
    }

    private void LogResult(HitGrade grade, int distance)
    {
        string color = "white";
        string info = distance >= 0 ? $" (дистанция: {distance})" : "";

        switch (grade)
        {
            case HitGrade.Perfect: color = "cyan"; break;
            case HitGrade.Clear: color = "orange"; break;
            case HitGrade.Ok: color = "yellow"; break;
            case HitGrade.Miss: color = "red"; break;
        }

        Debug.Log($"<color={color}>[{grade.ToString().ToUpper()}]</color> Текущее комбо: <b>{currentCombo}</b>{info}");
    }
}