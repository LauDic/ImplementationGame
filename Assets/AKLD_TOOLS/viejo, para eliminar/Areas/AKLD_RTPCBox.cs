using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public class AKLD_RTPCBox : MonoBehaviour
{
    public Transform objectToCheck;

    [System.Serializable]
    public class RTPC
    {
        public AK.Wwise.RTPC rTPC = null;
        public float value = 0f;
        public bool global = false;
        public bool otherGameObject = false;
        public GameObject otherObject = null;

        public RTPC() { }
    }

    // Zone configuration in the form of a box
    [Header("Box")]
    public Vector3 relativeCenter = Vector3.zero;
    public Vector3 size = new Vector3(1f, 1f, 1f);
    public Color gizmoColor = Color.yellow;

    [Header("Wwise On Enter")]
    public List<RTPC> enterRTPC = new List<RTPC>();
    public bool debugOn = false;
    public string message = "Inside the area";

    [Header("On Exit")]
    public bool onExit = false;
    public List<RTPC> exitRTPC = new List<RTPC>();

    [HideInInspector]
    public bool areaActivated = false;  // Indicates if the zone is currently activated
    [HideInInspector]
    public Vector3 position; // Posición del gizmo de manipulación

    private void Update()
    {
        if (objectToCheck == null)
        {
            return;
        }

        bool insideArea = IsInsideArea(objectToCheck.position);

        if (insideArea && !areaActivated)
        {
            if (debugOn)
                Debug.Log(message);

            foreach (var rtpc in enterRTPC)
            {
                if (rtpc != null && rtpc.rTPC != null)
                    UpdateRTPC(rtpc.rTPC, rtpc.value, rtpc.global, rtpc.otherGameObject, rtpc.otherObject);
            }
            areaActivated = true;
        }
        else if (!insideArea)
        {
            if (areaActivated)
            {
                areaActivated = false;
                Exit();
            }
        }
    }

    private void Exit()
    {
        Debug.Log("Exiting area");

        if (onExit)
        {
            Debug.Log("Performing exit actions");

            foreach (var rtpc in exitRTPC)
            {
                if (rtpc != null && rtpc.rTPC != null)
                    RTPCExit(rtpc.rTPC, rtpc.value, rtpc.global, rtpc.otherGameObject, rtpc.otherObject);
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

    private void UpdateRTPC(AK.Wwise.RTPC myRTPC, float valor, bool global, bool other, GameObject otherGO)
    {
        if (global)
            myRTPC.SetGlobalValue(valor);
        else if (other)
            myRTPC.SetValue(otherGO, valor);
        else
            myRTPC.SetValue(this.gameObject, valor);

        Debug.Log("Enter value " + valor + " " + myRTPC);
    }

    private void RTPCExit(AK.Wwise.RTPC myRTPC, float valor, bool global, bool other, GameObject otherGO)
    {
        if (global)
            myRTPC.SetGlobalValue(valor);
        else if (other)
            myRTPC.SetValue(otherGO, valor);
        else
            myRTPC.SetValue(this.gameObject, valor);

        Debug.Log("Exit value " + valor + " " + myRTPC);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Vector3 areaCenter = transform.position + relativeCenter;
        Gizmos.DrawWireCube(areaCenter, size);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AKLD_RTPCBox))]
public class AKLD_RTPCBoxEditor : Editor
{
    private AKLD_RTPCBox box;
    private Texture2D image;


    private void OnEnable()
    {
        box = (AKLD_RTPCBox)target;
        image = Resources.Load<Texture2D>("Titulo script 7");

    }
    public override void OnInspectorGUI()
    {
        // Actualizar el objeto serializado
        serializedObject.Update();

        // Mostrar la imagen en el Inspector (arriba de todo)
        if (image != null)
        {
            GUILayout.Space(10f);
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(image.height));
            EditorGUI.DrawTextureTransparent(rect, image, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUILayout.HelpBox("No se pudo cargar la imagen. Asegúrate de que está en la carpeta Resources.", MessageType.Warning);
        }

        // Mostrar los campos predeterminados del script excepto el campo m_Script
        DrawPropertiesExcluding(serializedObject, "m_Script");
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(box.transform.position + box.relativeCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(box, "Move Box");
            box.relativeCenter = newPosition - box.transform.position;
        }
    }
}
#endif