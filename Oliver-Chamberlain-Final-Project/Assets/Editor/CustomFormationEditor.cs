using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RowData))]
public class RowDataDrawer:  PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PrefixLabel(position, label);
        Rect newPos = position;
        newPos.y += 18f;
        SerializedProperty data = property.FindPropertyRelative("rows");

        for(int i = 0; i < 7; i++)
        {
            SerializedProperty row = data.GetArrayElementAtIndex(i).FindPropertyRelative("row");
            newPos.height = 18f;
            if (row.arraySize != 9)
                row.arraySize = 9;
            newPos.width = position.width / 9;
            for(int j = 0; j < 9; j++)
            {
                EditorGUI.PropertyField(newPos, row.GetArrayElementAtIndex(j), GUIContent.none);
                newPos.x += newPos.width;
            }
            newPos.x = position.x;
            newPos.y += 18f;
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 18f * 8;
    }
}
