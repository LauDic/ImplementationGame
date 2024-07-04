// This script defines a simple box-shaped area in the Unity scene. It detects whether a specified object is inside the area
// and triggers Wwise states accordingly upon entering or exiting the area.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AKLD_StateBox : MonoBehaviour
{
    public Transform objectToCheck;

    [Header("Box")]
    public Vector3 relativeCenter = Vector3.zero;
    public Vector3 size = new Vector3(1f, 1f, 1f);
    public Color gizmoColor = Color.yellow;

    [Header("Wwise On Enter")]
    public List<AK.Wwise.State> enterState = new List<AK.Wwise.State>();
    public string enterMessage = "Inside the area";

    [Header("On Exit")]
    public bool onExit = false;
    public List<AK.Wwise.State> statesOnExit = new List<AK.Wwise.State>();
    public string exitMessage = "Outside the area";

    private bool hasEnteredOnce = false;
    private bool areaActivated = false;
    private bool enterStateTriggered = false;
    private bool exitStateTriggered = false;

    private void Update()
    {
        if (objectToCheck == null)
            return;

        bool isInside = IsInsideArea(objectToCheck.position);

        if (!isInside || (areaActivated && !hasEnteredOnce))
        {
            if (!isInside)
            {
                areaActivated = false;
                if (onExit && hasEnteredOnce && !exitStateTriggered)
                {
                    foreach (var exitState in statesOnExit)
                    {
                        if (exitState != null)
                            StateOnExit(exitState);
                    }
                    Debug.Log(exitMessage);
                    exitStateTriggered = true;
                }
            }
        }
        else
        {
            if (!areaActivated)
            {
                foreach (var enterState in enterState)
                {
                    if (enterState != null)
                        UpdateState(enterState);
                }
                Debug.Log(enterMessage);
                enterStateTriggered = true;
            }
            areaActivated = true;
            hasEnteredOnce = true;
        }

        // Reset flags when object re-enters the area
        if (isInside && areaActivated && exitStateTriggered)
        {
            enterStateTriggered = false;
            exitStateTriggered = false;
        }
    }

    private bool IsInsideArea(Vector3 position)
    {
        Vector3 areaCenter = transform.position + relativeCenter;
        float minX = areaCenter.x - size.x / 2;
        float maxX = areaCenter.x + size.x / 2;
        float minY = areaCenter.y - size.y / 2;
        float maxY = areaCenter.y + size.y / 2;
        float minZ = areaCenter.z - size.z / 2;
        float maxZ = areaCenter.z + size.z / 2;

        return (position.x >= minX && position.x <= maxX &&
                position.y >= minY && position.y <= maxY &&
                position.z >= minZ && position.z <= maxZ);
    }

    private void UpdateState(AK.Wwise.State mystate)
    {
        mystate.SetValue();
    }

    private void StateOnExit(AK.Wwise.State myState)
    {
        myState.SetValue();
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(AKLD_StateBox))]
public class AKLD_StateBoxEditor : Editor
{
    private Texture2D image;

    private void OnEnable()
    {
        image = Resources.Load<Texture2D>("Titulo script 7");
    }

    private void OnSceneGUI()
    {
        AKLD_StateBox manager = target as AKLD_StateBox;

        if (manager != null)
        {
            DrawAreaGizmo(manager);
        }
    }

    public override void OnInspectorGUI()
    {
        if (image != null)
        {
            GUILayout.Space(10f);
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(image.height));
            EditorGUI.DrawTextureTransparent(rect, image, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUILayout.HelpBox("Failed to load the image. Make sure it's in the Resources folder.", MessageType.Warning);
        }

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectToCheck"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relativeCenter"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("size"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoColor"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enterState"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enterMessage"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onExit"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("statesOnExit"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitMessage"), true);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawAreaGizmo(AKLD_StateBox manager)
    {
        Vector3 areaGlobalCenter = manager.transform.position + manager.relativeCenter;

        Handles.color = manager.gizmoColor;
        Handles.DrawWireCube(areaGlobalCenter, manager.size);

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(areaGlobalCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(manager, "Move Area");
            manager.relativeCenter = newPosition - manager.transform.position;
        }
    }
}

#endif
