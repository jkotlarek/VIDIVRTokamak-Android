using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LoadParticles))]
public class LoadParticlesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LoadParticles script = (LoadParticles)target;

        if (GUILayout.Button("Load"))
        {
            script.DoLoadParticles();
        }
    }
}
