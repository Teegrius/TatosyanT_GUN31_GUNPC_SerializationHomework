using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        // Сохраняем предыдущее состояние GUI
        bool previousGUIState = GUI.enabled;

        // Отключаем редактирование
        GUI.enabled = false;

        // Отрисовываем поле (только для чтения)
        UnityEditor.EditorGUI.PropertyField(position, property, label, true);

        // Восстанавливаем состояние GUI
        GUI.enabled = previousGUIState;
    }
}
#endif