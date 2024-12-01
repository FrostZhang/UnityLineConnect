using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ConnectGraph
{
    [CustomEditor(typeof(DLLineSegment))]
    public class DLLineSegmentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var dl = target as DLLineSegment;

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.LabelField(" ‰≥ˆ");
                if (dl.linesOut != null)
                {
                    foreach (var item in dl.linesOut)
                    {
                        EditorGUILayout.ObjectField(item, typeof(DLLine), false);
                    }
                }
                EditorGUILayout.LabelField(" ‰»Î");
                if (dl.linesIn != null)
                {
                    foreach (var item in dl.linesIn)
                    {
                        EditorGUILayout.ObjectField(item, typeof(DLLine), false);
                    }
                }
            }
        }
    }
}