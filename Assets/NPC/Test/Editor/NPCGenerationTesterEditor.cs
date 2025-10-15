using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCGenerationTester))]
public class NPCGenerationTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NPCGenerationTester test = (NPCGenerationTester)target;
        if (GUILayout.Button("Generate NPC"))
        {
            test.SpawnPatient();
        }

        if (GUILayout.Button("Clear"))
        {
            test.DeleteChildren();
        }
    }
}
