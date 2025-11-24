using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class PositionSaver : MonoBehaviour
    {
        [System.Serializable]
        public struct Data
        {
            public Vector3 Position;
            public float Time;
        }

        [ReadOnly]
        [Tooltip("Для заполнения воспользуйтесь контекстным меню - 'Create File'")]
        private TextAsset _json;

        [HideInInspector]
        [SerializeField]
        public List<Data> Records { get; private set; }

        private void Awake()
        {
            //todo comment: Что будет, если в теле этого условия не сделать выход из метода?
            // ОТВЕТ: Будет вызвана ошибка JsonUtility.FromJsonOverwrite на null reference, так как _json = null
            if (_json == null)
            {
                gameObject.SetActive(false);
                Debug.LogError("Please, create TextAsset and add in field _json");
                return;
            }

            JsonUtility.FromJsonOverwrite(_json.text, this);
            //todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
            // ОТВЕТ: Чтобы избежать NullReferenceException если десериализация не создала список Records
            if (Records == null)
                Records = new List<Data>(10);
        }

        private void OnDrawGizmos()
        {
            //todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
            // ОТВЕТ: Чтобы избежать ошибок при отрисовке Gizmos если список пуст или не инициализирован
            if (Records == null || Records.Count == 0) return;
            var data = Records;
            var prev = data[0].Position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(prev, 0.3f);
            //todo comment: Почему итерация начинается не с нулевого элемента?
            // ОТВЕТ: Потому что первый элемент уже обработан до цикла (prev = data[0]), 
            for (int i = 1; i < data.Count; i++)
            {
                var curr = data[i].Position;
                Gizmos.DrawWireSphere(curr, 0.3f);
                Gizmos.DrawLine(prev, curr);
                prev = curr;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Create File")]
        private void CreateFile()
        {
            //todo comment: Что происходит в этой строке?
            // ОТВЕТ: Создаётся файл "Path.txt" в папке Assets проекта
            var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
            //todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав) 
            // ОТВЕТ: Освобождает файловый поток, чтобы файл стал доступен для работы Unity
            // Без этого AssetDatabase не сможет работать с файлом (файл заблокирован)
            stream.Dispose();
            UnityEditor.AssetDatabase.Refresh();
            //В Unity можно искать объекты по их типу, для этого используется префикс "t:"
            //После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
            var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
            foreach (var guid in guids)
            {
                //Этой командой можно получить путь к ассету через его гуид
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                //Этой командой можно загрузить сам ассет
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                //todo comment: Для чего нужны эти проверки?
                // ОТВЕТ: Чтобы найти именно TextAsset с именем "Path" и избежать NullReferenceException
                if (asset != null && asset.name == "Path")
                {
                    _json = asset;
                    UnityEditor.EditorUtility.SetDirty(this);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    //todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
                    // ОТВЕТ: Потому что нужный файл уже найден, нет смысла продолжать поиск
                    return;
                }
            }
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            // ОТВЕТ: Сохраняем данные Records в JSON файл при уничтожении объекта
            if (_json != null && Records != null && Records.Count > 0)
            {
                // Создаем временный объект для сериализации только Records
                var saveData = new { Records = this.Records };
                string jsonData = JsonUtility.ToJson(saveData, true);

                // Записываем в файл
                string filePath = Path.Combine(Application.dataPath, "Path.txt");
                File.WriteAllText(filePath, jsonData);

                UnityEditor.AssetDatabase.Refresh();
                Debug.Log($"Saved {Records.Count} records to {filePath}");
            }
#endif
        }
#endif
    }
}