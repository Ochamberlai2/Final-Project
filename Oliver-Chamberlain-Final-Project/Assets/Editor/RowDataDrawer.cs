using UnityEngine;
using UnityEditor;

//ref https://www.youtube.com/watch?v=mxqD1B2e4ME (referenced in blog post: )

[CustomPropertyDrawer(typeof(RowData))]
public class RowDataDrawer:  PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PrefixLabel(position, label);
        Rect newPos = position;
        newPos.y += 18f;
        SerializedProperty data = property.FindPropertyRelative("rows");
        SerializedProperty numAgents = property.FindPropertyRelative("NumAgents");
        if(data.arraySize != numAgents.intValue)
        {
            data.arraySize = numAgents.intValue;
        }
        for(int i = 0; i <= data.arraySize -1; i++)
        {
            SerializedProperty row = data.GetArrayElementAtIndex(i).FindPropertyRelative("Column");
            newPos.height = 18f;
            if (row.arraySize != data.arraySize)
                row.arraySize = data.arraySize;
            newPos.width = position.width / data.arraySize;
            for(int j = 0; j < row.arraySize; j++)
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
        SerializedProperty numAgents = property.FindPropertyRelative("NumAgents");
        return 18f * (numAgents.intValue + 1);
    }
}
