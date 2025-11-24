using UnityEngine;

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
            _save.Records.Clear();

            // Проверка: если _duration меньше или равно _delay, устанавливаем _duration в 5 раз больше _delay
            if (_duration <= _delay)
            {
                _duration = _delay * 5f;
                Debug.LogWarning($"Duration was too small. Auto-adjusted to {_duration}");
            }
        }

        private void Update()
        {
            _duration -= Time.deltaTime;
            if (_duration <= 0f)
            {
                enabled = false;
                Debug.Log($"<b>{name}</b> finished", this);
                return;
            }

            //todo comment: Почему не написать (_delay -= Time.deltaTime;) по аналогии с полем _duration?
            // ОТВЕТ: _delay - это константный интервал между сохранениями, а _currentDelay - текущий отсчёт. 
            // Если уменьшать _delay, то интервалы между сохранениями будут постоянно сокращаться
            _currentDelay -= Time.deltaTime;
            if (_currentDelay <= 0f)
            {
                _currentDelay = _delay;
                _save.Records.Add(new PositionSaver.Data
                {
                    Position = transform.position,
                    //todo comment: Для чего сохраняется значение игрового времени?
                    // ОТВЕТ: Для интерполяции и плавного воспроизведения в ReplayMover с правильной временной шкалой
                    Time = Time.time,
                });
            }
        }
    }
}