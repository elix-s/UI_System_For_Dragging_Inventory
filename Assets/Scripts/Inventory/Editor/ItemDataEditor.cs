using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    private const string ShapePropertyName = "ShapeMatrixData";
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector(); 
        ItemData itemData = (ItemData)target;

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Item Shape Configuration", EditorStyles.boldLabel);

        if (itemData.Dimensions.x <= 0 || itemData.Dimensions.y <= 0)
        {
            EditorGUILayout.HelpBox("Dimensions (X and Y) must be greater than 0.", MessageType.Warning);
        }
        else
        {
             if (itemData.Dimensions.x * itemData.Dimensions.y != serializedObject.FindProperty(ShapePropertyName).arraySize)
             {
                 serializedObject.ApplyModifiedPropertiesWithoutUndo(); 
                 serializedObject.Update(); 
             }
             
            EditorGUILayout.LabelField("Define occupied cells (true = part of item):");
            
            for (int y = 0; y < itemData.Dimensions.y; y++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int x = 0; x < itemData.Dimensions.x; x++)
                {
                    int index = y * itemData.Dimensions.x + x;
                    
                    if (index < serializedObject.FindProperty(ShapePropertyName).arraySize)
                    {
                        SerializedProperty cellProp = serializedObject.FindProperty(ShapePropertyName).GetArrayElementAtIndex(index);
                        bool newValue = EditorGUILayout.Toggle(cellProp.boolValue, 
                            GUILayout.Width(25), GUILayout.Height(25));
                        
                        if (newValue != cellProp.boolValue)
                        {
                            cellProp.boolValue = newValue;
                        }
                    } 
                    else 
                    {
                        EditorGUILayout.LabelField("!", GUILayout.Width(25), 
                            GUILayout.Height(25)); // problem with size
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}