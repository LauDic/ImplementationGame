using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AK.Wwise;

public class AKLD_TimerEvent : MonoBehaviour
{
    public Transform objectToCheck;

    [Header("Box")]
    public Vector3 relativeCenter = Vector3.zero;
    public Vector3 size = new Vector3(1f, 1f, 1f);
    public Color gizmoColor = Color.yellow;

    [Header("Wwise On Enter")]
    public List<AK.Wwise.Event> enterEvents = new List<AK.Wwise.Event>();
    public bool debugOn = false;
    public string message = "Inside the area";

    [Header("On Exit")]
    public bool stopEventsOnExit = true;
    public bool onExit = false;
    public List<AK.Wwise.Event> eventsOnExit = new List<AK.Wwise.Event>();

    [Header("Timer")]
    public float repeatInterval = 1f; // Intervalo de repetición en segundos

    [Header("Sound Settings")]
    public bool playSoundOnEnter = true; // Determina si se debe reproducir el sonido cuando se entra en el área por primera vez
    private bool wasInsideLastFrame = false; // Variable para rastrear si el objeto estaba dentro del área en el último cuadro
    private Coroutine eventCoroutine; // Referencia a la corutina para poder detenerla si es necesario.

    [Header("Repeat Settings")]
    public bool limitRepeatCount = false; // Determina si se debe limitar la cantidad de repeticiones
    public int repeatCount = 1; // Cantidad de repeticiones permitidas, incluida la primera

#if UNITY_EDITOR

    [CustomEditor(typeof(AKLD_TimerEvent))]
    public class AKLD_TimerEventEditor : Editor
    {
        private Texture2D image;
        private SerializedProperty m_Script; // Reference to the m_Script field

        private void OnEnable()
        {
            // Get reference to the m_Script field
            m_Script = serializedObject.FindProperty("m_Script");

            // Load the image from resources
            image = Resources.Load<Texture2D>("Titulo script 7");
        }

        private void OnSceneGUI()
        {
            AKLD_TimerEvent manager = target as AKLD_TimerEvent;

            if (manager != null)
            {
                DrawAreaGizmo(manager);
            }
        }

        // Update the serialized object
        public override void OnInspectorGUI()
        {
            // Update the serialized object
            serializedObject.Update();

            // Show the image in the Inspector (at the top)
            if (image != null)
            {
                GUILayout.Space(10f);
                Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(image.height));
                EditorGUI.DrawTextureTransparent(rect, image, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUILayout.HelpBox("Could not load the image. Make sure it's in the Resources folder.", MessageType.Warning);
            }

            // Show the default fields of the script except the m_Script field
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawAreaGizmo(AKLD_TimerEvent manager)
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


    private void Update()
    {
        if (objectToCheck == null)
        {
            return;
        }

        bool isInside = IsInsideArea(objectToCheck.position);

        if (isInside && !wasInsideLastFrame)
        {
            if (debugOn)
                Debug.Log(message);

            // Reproduce el sonido cada vez que se entra en el área
            if (playSoundOnEnter)
            {
                foreach (var enterEvent in enterEvents)
                {
                    if (enterEvent != null) UpdateEvent(enterEvent);
                }
            }

            // Comenzar la corutina solo si no está corriendo y no hemos alcanzado el límite de repeticiones
            if (eventCoroutine == null && (!limitRepeatCount || repeatCount > 0))
                eventCoroutine = StartCoroutine(TriggerEventsRepeatedly());
        }
        else if (!isInside && wasInsideLastFrame)
        {
            if (onExit)
            {
                foreach (var exitEvent in eventsOnExit)
                {
                    if (exitEvent != null) EventOnExit(exitEvent);
                }
            }

            if (stopEventsOnExit)
            {
                foreach (var enterEvent in enterEvents)
                {
                    if (enterEvent != null) StopEvent(enterEvent);
                }
            }

            // Detener la corutina si está corriendo
            if (eventCoroutine != null)
            {
                StopCoroutine(eventCoroutine);
                eventCoroutine = null;
            }
        }

        wasInsideLastFrame = isInside; // Update the variable to track the current state.
    }

    private IEnumerator TriggerEventsRepeatedly()
    {
        int count = 0;
        while (!limitRepeatCount || count < repeatCount)
        {
            yield return new WaitForSeconds(repeatInterval);

            foreach (var enterEvent in enterEvents)
            {
                if (enterEvent != null) UpdateEvent(enterEvent);
            }

            count++;
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

    private void UpdateEvent(AK.Wwise.Event myEvent)
    {
        myEvent.Post(gameObject);
    }

    private void StopEvent(AK.Wwise.Event myEvent)
    {
        myEvent.Stop(gameObject);
    }

    private void EventOnExit(AK.Wwise.Event myEvent)
    {
        myEvent.Post(gameObject);
    }
}
