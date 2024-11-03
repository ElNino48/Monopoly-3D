using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(MonopolyBoard))]
public class NodeSetEditor : Editor
{
    SerializedProperty nodeSetListProperty;

    private void OnEnable()
    {
        nodeSetListProperty = serializedObject.FindProperty("nodeSetList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //КОД ДЛЯ СМЕНЫ ЦВЕТОВ: 

        MonopolyBoard monopolyBoard = (MonopolyBoard)target;//делаем inspecting monopolyBoard 
        EditorGUILayout.PropertyField(nodeSetListProperty, true);
        if (GUILayout.Button("Изменить цвет картинок"))
        {
            Undo.RecordObject(monopolyBoard, "Images color change");
            for (int i = 0; i < monopolyBoard.nodeSetList.Count; i++)
            {
                MonopolyBoard.NodeSet nodeSet = monopolyBoard.nodeSetList[i];

                for (int j = 0; j < nodeSet.nodesInSetList.Count; j++)
                {
                    MonopolyNode node = nodeSet.nodesInSetList[j];
                    Image image = node.propertyColorField;
                    if (image != null)
                    {
                        Undo.RecordObject(image, "Image color change");
                        image.color = nodeSet.setColor;
                    }
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
