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
        [SerializeField] private TextAsset _json;

        [SerializeField] private List<Data> _records;
        public List<Data> Records
        {
            get => _records ??= new List<Data>();
            private set => _records = value;
        }

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

            var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
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

        [ContextMenu("Save Records")]
        public void SaveRecordsToFile()
        {
#if UNITY_EDITOR
            if (Records.Count > 0)
            {
                // Правильная сериализация без wrapper
                string jsonData = JsonUtility.ToJson(new Serialization<List<Data>>(Records), true);
                string filePath = Path.Combine(Application.dataPath, "Path.txt");
                File.WriteAllText(filePath, jsonData);
                UnityEditor.AssetDatabase.Refresh();
                Debug.Log($"Saved {Records.Count} records to file");
            }
            else
            {
                Debug.LogWarning("No records to save");
            }
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (_json != null && Records.Count > 0)
            {
                SaveRecordsToFile();
            }
#endif
        }

        // Класс для правильной сериализации списка
        [System.Serializable]
        private class Serialization<T>
        {
            public T target;
            public Serialization(T target) => this.target = target;
        }
#endif
    }
}