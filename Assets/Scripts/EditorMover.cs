using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace DefaultNamespace
{
    [RequireComponent(typeof(PositionSaver))]
    public class EditorMover : MonoBehaviour
    {
        private PositionSaver _save;
        private float _currentDelay;

        //todo comment: Что произойдёт, если _delay > _duration?
        // ОТВЕТ: Сохранится только одна позиция, так как общее время записи закончится до следующего сохранения
        [Range(0.2f, 1.0f)]
        private float _delay = 0.5f;

        [Min(0.2f)]
        private float _duration = 5f;

        private void Start()
        {
            //todo comment: Почему этот поиск производится здесь, а не в начале метода Update?
            // ОТВЕТ: GetComponent() - дорогостоящая операция, делается один раз при инициализации
            _save = GetComponent<PositionSaver>();

            // Очищаем записи только если они существуют
            if (_save.Records != null)
            {
                _save.Records.Clear();
                Debug.Log("Records cleared");
            }
            else
            {
                Debug.LogWarning("Records is null - creating new list");
                // Если Records null, создаем новый список
                _save.Records = new List<PositionSaver.Data>();
            }

            // Проверка: если _duration меньше или равно _delay, устанавливаем _duration в 5 раз больше _delay
            if (_duration <= _delay)
            {
                _duration = _delay * 5f;
                Debug.LogWarning($"Duration was too small. Auto-adjusted to {_duration}");
            }

            Debug.Log($"EditorMover started: delay={_delay}, duration={_duration}");
        }

        private void Update()
        {
            _duration -= Time.deltaTime;

            // Если время записи закончилось
            if (_duration <= 0f)
            {
                enabled = false;
                Debug.Log($"<b>{name}</b> finished. Total records: {(_save.Records?.Count ?? 0)}", this);

                // СОХРАНЯЕМ ДАННЫЕ СРАЗУ ПОСЛЕ ЗАПИСИ
                if (_save != null && _save.Records != null && _save.Records.Count > 0)
                {
                    Debug.Log($"Attempting to save {_save.Records.Count} records");
                    _save.SaveRecordsToFile();
                }
                else
                {
                    Debug.LogWarning("No records to save - list is empty or null");
                }
                return;
            }

            // Обработка сохранения позиций
            _currentDelay -= Time.deltaTime;
            if (_currentDelay <= 0f)
            {
                _currentDelay = _delay;

                // ДОБАВЛЯЕМ ОТЛАДОЧНУЮ ИНФОРМАЦИЮ
                Debug.Log($"Saving position: {transform.position} at time: {Time.time}");

                // Проверяем и создаем список если нужно
                if (_save.Records == null)
                {
                    _save.Records = new List<PositionSaver.Data>();
                    Debug.Log("Created new Records list");
                }

                _save.Records.Add(new PositionSaver.Data
                {
                    Position = transform.position,
                    //todo comment: Для чего сохраняется значение игрового времени?
                    // ОТВЕТ: Для интерполяции и плавного воспроизведения в ReplayMover с правильной временной шкалой
                    Time = Time.time,
                });

                Debug.Log($"Total records now: {_save.Records.Count}");
            }
        }

        // Вспомогательный метод для принудительного сохранения
        [ContextMenu("Force Save Records")]
        private void ForceSaveRecords()
        {
            if (_save != null)
            {
                _save.SaveRecordsToFile();
            }
        }
    }
}