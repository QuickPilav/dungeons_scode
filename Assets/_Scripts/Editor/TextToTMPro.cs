using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NicholasSheehan
{
    //Place in an editor folder
    class TextToTMPro
    {
        [MenuItem("Tools/Replace Text Component With Text Mesh Pro", true)]
        static bool TextSelectedValidation()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0) return false;

            foreach (var selectedObject in selectedObjects)
            {
                var text = selectedObject.GetComponent<Text>();
                if (!text) return false;
            }

            return true;
        }

        [MenuItem("Tools/Replace Text Component With Text Mesh Pro")]
        static void ReplaceSelectedObjects()
        {
            var selectedObjects = Selection.gameObjects;
            Undo.RecordObjects(selectedObjects, "Replace Text Component with Text Mesh Pro Component");

            foreach (var selectedObject in selectedObjects)
            {
                var textComp = selectedObject.GetComponent<Text>();
                var textSizeDelta = textComp.rectTransform.sizeDelta;
                //text component is still alive in memory, so the settings are still intact
                Undo.DestroyObjectImmediate(textComp);

                var tmp = Undo.AddComponent<TextMeshProUGUI>(selectedObject);

                tmp.text = textComp.text;

                tmp.fontSize = textComp.fontSize;

                var fontStyle = textComp.fontStyle;
                switch (fontStyle)
                {
                    case FontStyle.Normal:
                        tmp.fontStyle = FontStyles.Normal;
                        break;
                    case FontStyle.Bold:
                        tmp.fontStyle = FontStyles.Bold;
                        break;
                    case FontStyle.Italic:
                        tmp.fontStyle = FontStyles.Italic;
                        break;
                    case FontStyle.BoldAndItalic:
                        tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
                        break;
                }

                tmp.enableAutoSizing = textComp.resizeTextForBestFit;
                tmp.fontSizeMin = textComp.resizeTextMinSize;
                tmp.fontSizeMax = textComp.resizeTextMaxSize;

                var alignment = textComp.alignment;
                switch (alignment)
                {
                    case TextAnchor.UpperLeft:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Left;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Top;
                        break;
                    case TextAnchor.UpperCenter:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Top;
                        break;
                    case TextAnchor.UpperRight:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Right;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Top;
                        break;
                    case TextAnchor.MiddleLeft:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Left;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
                        break;
                    case TextAnchor.MiddleCenter:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
                        break;
                    case TextAnchor.MiddleRight:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Right;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
                        break;
                    case TextAnchor.LowerLeft:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Left;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Bottom;
                        break;
                    case TextAnchor.LowerCenter:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Bottom;
                        break;
                    case TextAnchor.LowerRight:
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Right;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Bottom;
                        break;
                }

                tmp.enableWordWrapping = textComp.horizontalOverflow == HorizontalWrapMode.Wrap;

                tmp.color = textComp.color;
                tmp.raycastTarget = textComp.raycastTarget;
                tmp.richText = textComp.supportRichText;

                tmp.rectTransform.sizeDelta = textSizeDelta;

                string fontPath = AssetDatabase.GetAssetPath(textComp.font);
                fontPath = fontPath.Substring(0,fontPath.Length - 4) + " SDF.asset";
                Debug.Log(fontPath);
                tmp.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(fontPath);
            }
        }
    }
}