/**
 * LabeledArrayDrawer.cs
 * Created by: Joao Borks [joao.borks@gmail.com]
 * Created on: 28/12/17 (dd/mm/yy)
 * Reference from John Avery: https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/
 */

using UnityEngine;
using UnityEditor;
using System.Linq;

// Don't forget to put this file inside an Editor folder!
[CustomPropertyDrawer(typeof(LabeledArrayAttribute))]
public class LabeledArrayDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(rect, label, property);
        try
        {
            var path = property.propertyPath;
            int pos = int.Parse(path.Split('[').LastOrDefault().TrimEnd(']'));

            var att =(LabeledArrayAttribute)attribute;

            for (int i = 0; i < att.values.Length; i++)
            {
                if (att.values[i] == pos)
                {
                    EditorGUI.PropertyField(rect, property, new GUIContent(att.names[i]),true);
                    break;
                }
            }

        }
        catch
        {
            EditorGUI.PropertyField(rect, property, label, true);
        }
        EditorGUI.EndProperty();
    }
}