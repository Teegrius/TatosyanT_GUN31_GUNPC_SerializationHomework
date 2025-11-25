using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(PositionSaver))]
    public class EditorMover : MonoBehaviour
    {
        private PositionSaver _save;
        private float _currentDelay;

        [Range(0.2f, 1.0f)]
        private float _delay = 0.5f;

        [Min(0.2f)]
        private float _duration = 5f;

        private void Start()
        {
            _save = GetComponent<PositionSaver>();
            _save.Records.Clear(); // Теперь это работает с правильным свойством

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
                Debug.Log($"<b>{name}</b> finished. Total records: {_save.Records.Count}", this);
                _save.SaveRecordsToFile();
                return;
            }

            _currentDelay -= Time.deltaTime;
            if (_currentDelay <= 0f)
            {
                _currentDelay = _delay;
                _save.Records.Add(new PositionSaver.Data
                {
                    Position = transform.position,
                    Time = Time.time,
                });
            }
        }
    }
}