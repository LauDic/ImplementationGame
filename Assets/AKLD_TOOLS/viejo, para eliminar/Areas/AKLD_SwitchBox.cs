using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AKLD_SwitchBox : CustomEditorBase
{
    public Transform objectToCheck;

    [Header("Box")]
    public Vector3 relativeCenter = Vector3.zero;
    public Vector3 size = new Vector3(1f, 1f, 1f);
    public Color gizmoColor = Color.yellow;

    // Lista de interruptores para la entrada
    [Header("Wwise On Enter")]
    public List<AK.Wwise.Switch> enterSwitches = new List<AK.Wwise.Switch>();
    public bool DebugOn = false;
    public string message = "Inside the area";

    public bool otherOwner = false;
    public GameObject ownerSwitch = null;

    // Lista de interruptores para la salida
    [Header("On Exit")]
    public bool OnExit = false;
    public List<AK.Wwise.Switch> switchesOnExit = new List<AK.Wwise.Switch>();

    public bool otherOwnerExit = false;
    public GameObject ownerSwitchExit = null;

    [HideInInspector]
    public bool areaActivated = false;
    [HideInInspector]
    public Vector3 position;

    private void Update()
    {
        if (objectToCheck == null)
        {
            return;
        }

        if (IsInsideArea(objectToCheck.position) && !areaActivated)
        {
            if (DebugOn)
                Debug.Log(message);

            foreach (var enterSwitch in enterSwitches)
            {
                if (enterSwitch != null)
                    UpdateSwitch(enterSwitch, otherOwner, ownerSwitch);
            }
            areaActivated = true;
        }
        else if (!IsInsideArea(objectToCheck.position))
        {
            areaActivated = false;

            if (OnExit)
            {
                foreach (var switchOnExit in switchesOnExit)
                {
                    if (switchOnExit != null)
                        SwitchOnExit(switchOnExit, otherOwnerExit, ownerSwitchExit);
                }
            }
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

        return (position.x > minX && position.x < maxX &&
                position.y > minY && position.y < maxY &&
                position.z > minZ && position.z < maxZ);
    }

    private void UpdateSwitch(AK.Wwise.Switch mySwitch, bool owner, GameObject otherOwner)
    {
        if (!owner)
            mySwitch.SetValue(this.gameObject);
        else
            mySwitch.SetValue(otherOwner);
    }

    private void SwitchOnExit(AK.Wwise.Switch mySwitch, bool owner, GameObject otherOwner)
    {
        if (!owner)
            mySwitch.SetValue(this.gameObject);
        else
            mySwitch.SetValue(otherOwner);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AKLD_SwitchBox))]
public class AKLD_SwitchBoxEditor : Editor
{
    private Texture2D image;
    private SerializedProperty m_Script;

    private void OnEnable()
    {
        m_Script = serializedObject.FindProperty("m_Script");
        image = Resources.Load<Texture2D>("Titulo script 7");
    }

    private void OnSceneGUI()
    {
        AKLD_SwitchBox manager = target as AKLD_SwitchBox;

        if (manager != null)
        {
            DrawAreaGizmo(manager);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

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

        DrawPropertiesExcluding(serializedObject, "m_Script");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawAreaGizmo(AKLD_SwitchBox manager)
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
