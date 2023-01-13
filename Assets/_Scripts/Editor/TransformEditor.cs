using UnityEditor;
using UnityEngine;


namespace EditorStuff
{
    [CustomEditor(typeof(Transform))]
    public class TransformToMouse : Editor
    {
        private void OnEnable ()
        {
            SceneView.duringSceneGui += SceneView_duringSceneGui;
        }

        private void OnDisable ()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
        }

        private void SceneView_duringSceneGui (SceneView view)
        {
            Event e = Event.current;
            if (e.control && e.keyCode == KeyCode.Space && e.type == EventType.KeyDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Undo.RecordObject(target as Transform, "Move to mouse");
                    (target as Transform).position = hit.point;
                }
            }
        }
    }
}
