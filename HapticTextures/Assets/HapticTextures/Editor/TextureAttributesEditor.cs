#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureAttributes))]
public class TextureAttributesEditor : Editor
{

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Update Attributes"))
            {
                TextureAttributes t = (TextureAttributes)serializedObject.targetObject;
                t.ForceUpdateTexture();
            }
        }
        DrawDefaultInspector();
    }
}
#endif