using UnityEditor;

namespace RPG.CameraUI
{
    // TODO consider changing to a property drawer
    [CustomEditor(typeof(CameraRaycaster))]
    public class CameraRaycasterEditor : Editor
    {
        bool isLayerPrioritiesUnfolded = true; // store the UI state
        bool isFadableLayersUnfolded = true; // store the UI state

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Serialize cameraRaycaster instance

            isLayerPrioritiesUnfolded = EditorGUILayout.Foldout(isLayerPrioritiesUnfolded, "Layer Priorities");
            if (isLayerPrioritiesUnfolded)
            {
                EditorGUI.indentLevel++;
                {
                    BindArraySize("layerPriorities");
                    BindArrayElements("layerPriorities", "Layer");
                }
                EditorGUI.indentLevel--;
            }

            isFadableLayersUnfolded = EditorGUILayout.Foldout(isFadableLayersUnfolded, "Fadable Layers");
            if (isFadableLayersUnfolded)
            {
                EditorGUI.indentLevel++;
                {
                    BindArraySize("FadableLayers");
                    BindArrayElements("FadableLayers", "Layer"); 
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties(); // De-serialize back to cameraRaycaster (and create undo point)
        }

        void BindArraySize(string fieldName)
        {
            int currentArraySize = serializedObject.FindProperty(fieldName + ".Array.size").intValue;
            int requiredArraySize = EditorGUILayout.IntField("Size", currentArraySize);
            if (requiredArraySize != currentArraySize)
            {
                serializedObject.FindProperty(fieldName + ".Array.size").intValue = requiredArraySize;
            }
        }

        void BindArrayElements(string fieldName, string subFieldName)
        {
            int currentArraySize = serializedObject.FindProperty(fieldName + ".Array.size").intValue;
            for (int i = 0; i < currentArraySize; i++)
            {
                var prop = serializedObject.FindProperty(string.Format(fieldName + ".Array.data[{0}]", i));
                prop.intValue = EditorGUILayout.LayerField(string.Format(subFieldName + " {0}:", i), prop.intValue);
            }
        }
    }
}
