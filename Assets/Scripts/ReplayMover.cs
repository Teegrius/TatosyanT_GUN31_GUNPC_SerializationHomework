using System;
using UnityEngine;

namespace DefaultNamespace
{
	[RequireComponent(typeof(PositionSaver))]
	public class ReplayMover : MonoBehaviour
	{
		private PositionSaver _save;

		private int _index;
		private PositionSaver.Data _prev;
		private float _duration;

		private void Start()
		{
            ////todo comment: зачем нужны эти проверки?
            // ОТВЕТ: Чтобы убедиться что компонент PositionSaver существует и в нём есть записи для воспроизведения
            if (!TryGetComponent(out _save) || _save.Records.Count == 0)
			{
				Debug.LogError("Records incorrect value", this);
                //todo comment: Для чего выключается этот компонент?
                // ОТВЕТ: Чтобы остановить воспроизведение если нет данных для работы (избежать ошибок)
                enabled = false;
			}
		}

		private void Update()
		{
			var curr = _save.Records[_index];
            //todo comment: Что проверяет это условие (с какой целью)? 
            // ОТВЕТ: Проверяет, настало ли время переходить к следующей точке анимации
            if (Time.time > curr.Time)
			{
				_prev = curr;
				_index++;
                //todo comment: Для чего нужна эта проверка?
                // ОТВЕТ: Чтобы остановить воспроизведение когда все точки траектории пройдены
                if (_index >= _save.Records.Count)
				{
					enabled = false;
					Debug.Log($"<b>{name}</b> finished", this);
				}
			}
            //todo comment: Для чего производятся эти вычисления (как в дальнейшем они применяются)?
            // ОТВЕТ: Вычисляется интерполяционный коэффициент (0-1) для плавного движения между точками
            var delta = (Time.time - _prev.Time) / (curr.Time - _prev.Time);
            //todo comment: Зачем нужна эта проверка?
            // ОТВЕТ: Чтобы избежать ошибки деления на ноль, если временные метки точек совпадают
            if (float.IsNaN(delta)) delta = 0f;
            //todo comment: Опишите, что происходит в этой строчке так подробно, насколько это возможно
            // ОТВЕТ: Производится линейная интерполяция позиции между предыдущей и текущей точками. 
            // Коэффициент delta (0-1) определяет, насколько близко объект находится к текущей точке:
            // - 0 = полностью в предыдущей позиции
            // - 0.5 = посередине между точками  
            // - 1 = полностью в текущей позиции
            // Это создаёт плавное движение вместо резких скачков между точками
            transform.position = Vector3.Lerp(_prev.Position, curr.Position, delta);
		}
	}
}