using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class Editor_PoseProcessor : EditorWindow
{
    private Transform poseableObjectRoot;
    private Transform doctorPose;
    private Transform patientPose;
    private RoomPose targetPoseObject;

    [MenuItem("MyTools/Office Pose Processor")]
    public static void ShowWindow()
    {
        GetWindow<Editor_PoseProcessor>("Office Pose Processor");
    }

    void OnGUI()
    {
        GUILayout.Label("Process Transform Data to ScriptableObject", EditorStyles.boldLabel);
        GUILayout.Space(25);

        // Allow user to drag and drop or select the source Prefab
        GUILayout.Label("Sources", EditorStyles.boldLabel);
        poseableObjectRoot = (Transform)EditorGUILayout.ObjectField("Poseable Objects Parent", poseableObjectRoot, typeof(Transform), true);

        GUILayout.Space(15);
        GUILayout.Label("Target", EditorStyles.boldLabel);
        // Allow user to assign an existing SO or leave blank to create a new one
        targetPoseObject = (RoomPose)EditorGUILayout.ObjectField("Target ScriptableObject", targetPoseObject, typeof(RoomPose), true);

        GUILayout.Space(15);
        if (GUILayout.Button("Process Data and Save"))
        {
            if (targetPoseObject != null)
            {
                ProcessAndSaveData();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign a target.", "OK");
            }
        }
    }

    private void ProcessAndSaveData() {
        Undo.RecordObject(targetPoseObject, "Capture Room Pose");

        targetPoseObject.decorPose ??= new List<ObjectPose>();
        targetPoseObject.decorPose.Clear();

        if (poseableObjectRoot != null) {
            for (int i = 0; i < poseableObjectRoot.childCount; i++) {
                Transform t = poseableObjectRoot.GetChild(i);
                targetPoseObject.decorPose.Add(new ObjectPose(t.localPosition, t.localRotation));
            }
        }

        EditorUtility.SetDirty(targetPoseObject);
        AssetDatabase.SaveAssets();
    }
}
