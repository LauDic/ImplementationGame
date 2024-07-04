using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AK.Wwise;

public class AKLD_EventBox : MonoBehaviour
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

    [HideInInspector]
    public bool areaActivated = false;

    private bool wasInsideLastFrame = false;

#if UNITY_EDITOR

    [CustomEditor(typeof(AKLD_EventBox))]
    public class AKLD_EventBoxEditor : Editor
    {

        private Texture2D image;
        private SerializedProperty m_Script; // Referencia al campo m_Script

        private void OnEnable()
        {
            // Obtener la referencia al campo m_Script
            m_Script = serializedObject.FindProperty("m_Script");

            // Cargar la imagen desde los recursos
            image = Resources.Load<Texture2D>("Titulo script 7");
        }

        private void OnSceneGUI()
        {
            AKLD_EventBox manager = target as AKLD_EventBox;

            if (manager != null)
            {
                DrawAreaGizmo(manager);
            }
        }

        // Actualizar el objeto serializado
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
            /*
            EditorGUILayout.PropertyField(serializedObject.FindProperty("objectToCheck"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("relativeCenter"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("size"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoColor"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enterEvents"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("debugOn"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("message"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stopEventsOnExit"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onExit"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventsOnExit"), true);
            EditorGUILayout.Space();
            */

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawAreaGizmo(AKLD_EventBox manager)
        {
            Vector3 areaGlobalCenter = manager.transform.position + manager.relativeCenter;

            Handles.color = manager.gizmoColor;
            Handles.DrawWireCube(areaGlobalCenter, manager.size);

            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(areaGlobalCenter, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(manager, "Move Areaa");
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

        if (isInside && !wasInsideLastFrame && !areaActivated)
        {
            if (debugOn)
                Debug.Log(message);

            foreach (var enterEvent in enterEvents)
            {
                if (enterEvent != null) UpdateEvent(enterEvent);
            }
            areaActivated = true;
        }
        else if (!isInside && wasInsideLastFrame)
        {
            if (stopEventsOnExit && areaActivated)
            {
                foreach (var enterEvent in enterEvents)
                {
                    if (enterEvent != null) StopEvent(enterEvent);
                }
            }

            if (onExit)
            {
                foreach (var exitEvent in eventsOnExit)
                {
                    if (exitEvent != null) EventOnExit(exitEvent);
                }
            }

            areaActivated = false;
        }

        wasInsideLastFrame = isInside;
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
