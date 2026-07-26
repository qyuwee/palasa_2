using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Описываем структуру данных для десериализации JSON
    [System.Serializable]
    public class NoteData
    {
        public float time;
        public float x;
        public NoteController.NoteType type; // ИСПРАВЛЕНО: Используем актуальный NoteController.NoteType
    }

    [System.Serializable]
    public class LevelData
    {
        public List<NoteData> notes;
    }

    [Header("Настройки спавна")]
    [SerializeField] private TextAsset jsonFile; // Сюда перетаскиваем наш level1.json в инспекторе
    [SerializeField] private GameObject[] notePrefabs; // Массив префабов нот (размер 2: для Left_SD и Right_KL)
    [SerializeField] private float spawnYPosition = 0f; // Фиксированная высота полосы по Y

    [Header("Аудио")]
    [SerializeField] private AudioSource musicSource;

    private List<NoteData> levelNotes = new List<NoteData>();
    private int currentNoteIndex = 0;
    private bool isLevelPlaying = false;

    void Start()
    {
        LoadLevel();
        if (musicSource != null)
        {
            musicSource.Play();
            isLevelPlaying = true;
        }
    }

    void Update()
    {
        if (!isLevelPlaying || currentNoteIndex >= levelNotes.Count) return;

        // Отслеживаем точное время воспроизведения музыки
        float currentTime = musicSource.time;

        // Проверяем, не пора ли спавнить текущую ноту
        while (currentNoteIndex < levelNotes.Count && currentTime >= levelNotes[currentNoteIndex].time)
        {
            SpawnNote(levelNotes[currentNoteIndex]);
            currentNoteIndex++;
        }
    }

    void LoadLevel()
    {
        if (jsonFile != null)
        {
            // Парсим JSON стандартным JsonUtility Unity
            // JsonUtility автоматически превратит числа 0 и 1 из JSON в соответствующие значения NoteType!
            LevelData data = JsonUtility.FromJson<LevelData>(jsonFile.text);
            levelNotes = data.notes;

            // Сортируем ноты по времени на всякий случай, чтобы спавн не сломался
            levelNotes.Sort((a, b) => a.time.CompareTo(b.time));
        }
        else
        {
            Debug.LogError("JSON файл уровня не назначен!");
        }
    }

    void SpawnNote(NoteData noteData)
    {
        // Переводим Enum в int, чтобы использовать его как индекс массива префабов
        int prefabIndex = (int)noteData.type;

        // Проверяем корректность индекса префаба
        if (prefabIndex < 0 || prefabIndex >= notePrefabs.Length) return;

        // Берем нужный префаб из массива на основе индекса
        GameObject prefabToSpawn = notePrefabs[prefabIndex];

        if (prefabToSpawn == null)
        {
            Debug.LogError($"Префаб ноты под индексом {prefabIndex} не назначен в массиве notePrefabs!");
            return;
        }

        Vector3 spawnPosition = new Vector3(noteData.x, spawnYPosition, 0f);

        // Спавнить правильный префаб
        GameObject spawnedNote = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // ИСПРАВЛЕНО: Теперь ищем компонент NoteController на корневом объекте спавна
        NoteController noteComponent = spawnedNote.GetComponent<NoteController>();
        if (noteComponent != null)
        {
            noteComponent.noteType = noteData.type;
        }
        else
        {
            Debug.LogWarning($"На заспавненном префабе {spawnedNote.name} отсутствует скрипт NoteController!");
        }
    }
}