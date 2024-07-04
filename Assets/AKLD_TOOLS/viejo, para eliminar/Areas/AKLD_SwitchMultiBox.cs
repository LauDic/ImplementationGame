using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AKLD_SwitchMultiBox : MonoBehaviour
{
    public Transform objectToCheck;  // Object whose position is checked to determine area activation

    [System.Serializable]
    public class AreaData
    {
        [Header("Box")]
        public Vector3 relativeCenter = Vector3.zero;
        public Vector3 size = new Vector3(1f, 1f, 1f);
        public Color gizmoColor = Color.yellow;

        [Header("Wwise On Enter")]
        public AK.Wwise.Switch enterSwitch = null;
        public bool DebugOn = false;
        public string message = "Inside the area";

        public bool otherOwner = false;
        public GameObject ownerSwitch = null;

        [Header("On Exit")]
        public bool OnExit = false;
        public AK.Wwise.Switch switchOnExit = null;
        public bool otherOwnerExit = false;
        public GameObject ownerSwitchExit = null;

        [HideInInspector]
        public bool areaActivated = false;
        [HideInInspector]
        public bool exitActivated = false;  // Added for tracking exit activation
        [HideInInspector]
        public Vector3 position;

        public void Initialize()
        {
            size = new Vector3(1f, 1f, 1f);
        }
    }

    public List<AreaData> areas = new List<AreaData>() { new AreaData() };

    private void Update()
    {
        if (objectToCheck == null)
        {
            return;
        }

        foreach (var area in areas)
        {
            if (IsInsideArea(objectToCheck.position, area) && !area.areaActivated)
            {
                if (area.DebugOn)
                    Debug.Log(area.message);

                if (area.enterSwitch != null)
                    UpdateSwitch(area.enterSwitch, area.otherOwner, area.ownerSwitch);

                area.areaActivated = true;
            }

            else if (!IsInsideArea(objectToCheck.position, area))
            {
                area.areaActivated = false;

                if (area.OnExit && area.exitActivated)  // Check if exit activated and configured to trigger
                {
                    SwitchOnExit(area.switchOnExit, area.otherOwnerExit, area.ownerSwitchExit);
                }
                else if (area.OnExit && area.areaActivated)  // If exit not yet activated and area is activated
                {
                    area.exitActivated = true;  // Mark exit as activated
                }
            }
        }
    }

    private bool IsInsideArea(Vector3 position, AreaData area)
    {
        Vector3 areaCenter = transform.position + area.relativeCenter;
        float minX = areaCenter.x - area.size.x / 2;
        float maxX = areaCenter.x + area.size.x / 2;
        float minY = areaCenter.y - area.size.y / 2;
        float maxY = areaCenter.y + area.size.y / 2;
        float minZ = areaCenter.z - area.size.z / 2;
        float maxZ = areaCenter.z + area.size.z / 2;

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
[CustomEditor(typeof(AKLD_SwitchMultiBox))]
public class AKLD_SwitchMultiBoxEditor : Editor
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
        AKLD_SwitchMultiBox manager = target as AKLD_SwitchMultiBox;

        if (manager != null)
        {
            foreach (var area in manager.areas)
            {
                DrawAreaGizmo(manager, area);
            }
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

    private void DrawAreaGizmo(AKLD_SwitchMultiBox manager, AKLD_SwitchMultiBox.AreaData area)
    {
        Vector3 areaGlobalCenter = manager.transform.position + area.relativeCenter;

        Handles.color = area.gizmoColor;
        Handles.DrawWireCube(areaGlobalCenter, area.size);

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(areaGlobalCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(manager, "Move Area");
            area.relativeCenter = newPosition - manager.transform.position;
        }
    }
}
#endif
