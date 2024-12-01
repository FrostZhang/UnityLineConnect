using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ConnectGraph
{
    [CustomEditor(typeof(DLLine))]
    public class DLLineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var dl = target as DLLine;
            if (dl.Segments != null)
            {
                foreach (var item in dl.Segments)
                {
                    EditorGUILayout.ObjectField(item, typeof(DLLineSegment), false);
                }
            }
        }
    }
}