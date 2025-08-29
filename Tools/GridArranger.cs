#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Gameplay.Script.Tools
{
    [ExecuteInEditMode]
    public class GridArranger : MonoBehaviour
    {
        public int rows = 5;
        public int columns = 8;
        public float spacingX = .55f;
        public float spacingY = .35f;
        public bool autoUpdate = true;
    
        private void OnValidate()
        {
            if (autoUpdate && !Application.isPlaying)
            {
                ArrangeChildren();
            }
        }
    
        public void ArrangeChildren()
        {
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (index >= transform.childCount) return;
                
                    Vector3 newPosition = new Vector3(
                        col * spacingX - (columns - 1) * spacingX * 0.5f,
                        row * spacingY - (rows - 1) * spacingY * 0.5f,
                        0
                    );
                
                    transform.GetChild(index).localPosition = newPosition;
                    index++;
                }
            }
        }
    }

    [CustomEditor(typeof(GridArranger))]
    public class GridArrangerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            GridArranger arranger = (GridArranger)target;
        
            if (GUILayout.Button("Arrange Children"))
            {
                arranger.ArrangeChildren();
            }
        }
    }
}
#endif