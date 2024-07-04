using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

public class AKLD_TimerRTPC : MonoBehaviour
{
    public Transform objectToCheck;

    public enum OperationType
    {
        Sum,
        Subtract
    }

    [System.Serializable]
    public class EnterRTPC
    {
        public AK.Wwise.RTPC rTPC = null;
        public float initialValue = 0f; // Valor inicial del RTPC
        public float incrementAmount = 0f; // Incremento del valor con el tiempo
        public float incrementInterval = 1f; // Intervalo de incremento en segundos
        public OperationType operationType = OperationType.Sum; // Tipo de operación (suma o resta)
        public bool global = false;
        public bool otherGameObject = false;
        public GameObject otherObject = null;
        [HideInInspector]
        public float currentValue = 0f; // Valor actual del RTPC
        [HideInInspector]
        public Coroutine coroutine; // Corrutina para el incremento del valor del RTPC

        public EnterRTPC() { }
    }

    [System.Serializable]
    public class RTPC
    {
        public AK.Wwise.RTPC rTPC = null;
        public float value = 0f; // Valor a establecer al salir del área
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
    public List<EnterRTPC> enterRTPC = new List<EnterRTPC>();
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
                {
                    // Establecer el valor inicial del RTPC al entrar
                    rtpc.currentValue = rtpc.initialValue;
                    UpdateRTPC(rtpc);

                    // Reiniciar la corrutina para incrementar el valor del RTPC
                    if (rtpc.incrementAmount != 0f)
                    {
                        rtpc.coroutine = StartCoroutine(IncrementRTPCValue(rtpc));
                    }
                }
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

    private IEnumerator IncrementRTPCValue(EnterRTPC rtpc)
    {
        while (true)
        {
            yield return new WaitForSeconds(rtpc.incrementInterval);

            // Actualizar el valor del RTPC según el tipo de operación (suma o resta)
            if (rtpc.operationType == OperationType.Sum)
                rtpc.currentValue += rtpc.incrementAmount;
            else if (rtpc.operationType == OperationType.Subtract)
                rtpc.currentValue -= rtpc.incrementAmount;

            UpdateRTPC(rtpc);
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
                {
                    // Detener la corrutina para incrementar el valor del RTPC
                    foreach (var enterRTPC in enterRTPC)
                    {
                        if (enterRTPC.coroutine != null)
                        {
                            StopCoroutine(enterRTPC.coroutine);
                        }
                    }

                    // Establecer el valor del RTPC de salida
                    if (rtpc.global)
                        rtpc.rTPC.SetGlobalValue(rtpc.value);
                    else if (rtpc.otherGameObject)
                        rtpc.rTPC.SetValue(rtpc.otherObject, rtpc.value);
                    else
                        rtpc.rTPC.SetValue(gameObject, rtpc.value);
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

    private void UpdateRTPC(EnterRTPC rtpc)
    {
        if (rtpc.global)
            rtpc.rTPC.SetGlobalValue(rtpc.currentValue);
        else if (rtpc.otherGameObject)
            rtpc.rTPC.SetValue(rtpc.otherObject, rtpc.currentValue);
        else
            rtpc.rTPC.SetValue(gameObject, rtpc.currentValue);

        Debug.Log("EnterPa " + rtpc.currentValue + " " + rtpc.rTPC);
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
[CustomEditor(typeof(AKLD_TimerRTPC))]
public class AKLD_TimerRTPCEditor : Editor
{
    private AKLD_TimerRTPC box;
    private Texture2D image;

    private void OnEnable()
    {
        box = (AKLD_TimerRTPC)target;
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

        // Mostrar el dropdown para seleccionar el tipo de operación (suma o resta)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Operation Type", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        foreach (var enterRTPC in box.enterRTPC)
        {
            enterRTPC.operationType = (AKLD_TimerRTPC.OperationType)EditorGUILayout.EnumPopup("Operation", enterRTPC.operationType);
        }
        EditorGUI.indentLevel--;

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
