using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AKLD_EventMultiBox : MonoBehaviour
{
    public GameObject objectToCheck;  // GameObject cuya posición se verifica para determinar la activación del área
    public bool showPositionGizmo = true;  // Nueva variable para mostrar/ocultar gizmo de posición
    public bool showRotationGizmo = true;  // Nueva variable para mostrar/ocultar gizmo de rotación
    public bool showSizeGizmo = true;      // Nueva variable para mostrar/ocultar gizmo de tamaño

    [System.Serializable]
    public class AreaData
    {
        [Header("Box")]
        public Vector3 relativeCenter = Vector3.zero;
        public Vector3 size = new Vector3(1f, 1f, 1f);
        public Color gizmoColor = Color.yellow;
        public Quaternion rotation = Quaternion.identity; // Nueva propiedad de rotación

        [Header("Wwise On Enter")]
        public AK.Wwise.Event enterEvent = null;
        public bool DebugOn = false;
        public string message = "Inside the area";

        [Header("On Exit")]
        public bool stopEventOnExit = true;
        public bool OnExit = false;
        public AK.Wwise.Event eventOnExit = null;

        [HideInInspector]
        public bool areaActivated = false;
        [HideInInspector]
        public bool insideLastFrame = false;
        [HideInInspector]
        public bool exitedOnce = false;
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
            bool isInside = IsInsideArea(objectToCheck, area);

            if (isInside && !area.insideLastFrame && !area.exitedOnce)
            {
                if (area.DebugOn)
                    Debug.Log(area.message);

                if (area.enterEvent != null)
                    UpdateEvent(area.enterEvent);

                area.areaActivated = true;
                area.insideLastFrame = true;
            }
            else if (!isInside && area.insideLastFrame)
            {
                area.insideLastFrame = false;

                if (area.stopEventOnExit && area.areaActivated)
                {
                    StopEvent(area.enterEvent);
                }

                if (area.OnExit && area.areaActivated)
                {
                    EventOnExit(area.eventOnExit);
                }

                area.exitedOnce = true;
            }

            if (isInside && area.exitedOnce)
            {
                area.exitedOnce = false;
            }
        }
    }

    private bool IsInsideArea(GameObject objectToCheck, AreaData area)
    {
        Vector3 areaCenter = transform.position + area.relativeCenter;
        Vector3 halfSize = area.size * 0.5f;

        Vector3 localPos = Quaternion.Inverse(area.rotation) * (objectToCheck.transform.position - areaCenter);

        return (localPos.x > -halfSize.x && localPos.x < halfSize.x &&
                localPos.y > -halfSize.y && localPos.y < halfSize.y &&
                localPos.z > -halfSize.z && localPos.z < halfSize.z);
    }

    private void UpdateEvent(AK.Wwise.Event myEvent)
    {
        myEvent.Post(this.gameObject);
        Debug.Log("Posted entry event");
    }

    private void StopEvent(AK.Wwise.Event myEvent)
    {
        myEvent.Stop(this.gameObject);
        Debug.Log("Stopped event");
    }

    private void EventOnExit(AK.Wwise.Event myEvent)
    {
        if (myEvent != null)
        {
            myEvent.Post(this.gameObject);
            Debug.Log("Posted exit event");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AKLD_EventMultiBox))]
    public class AKLD_EventMultiBoxEditor : Editor
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

        public override void OnInspectorGUI()
        {
            // Update the serialized object
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

            // Mostrar propiedades excepto m_Script
            DrawPropertiesExcluding(serializedObject, "m_Script");

            // Mostrar controles de visibilidad de gizmos
            AKLD_EventMultiBox manager = (AKLD_EventMultiBox)target;
            manager.showPositionGizmo = EditorGUILayout.Toggle("Show Position Gizmo", manager.showPositionGizmo);
            manager.showRotationGizmo = EditorGUILayout.Toggle("Show Rotation Gizmo", manager.showRotationGizmo);
            manager.showSizeGizmo = EditorGUILayout.Toggle("Show Size Gizmo", manager.showSizeGizmo);

            // Apply changes
            serializedObject.ApplyModifiedProperties();

            // Botón para inicializar tamaños
            if (GUILayout.Button("Initialize Sizes") && manager != null)
            {
                InitializeSizes(manager);
            }
        }

        private void InitializeSizes(AKLD_EventMultiBox manager)
        {
            if (manager != null)
            {
                foreach (var area in manager.areas)
                {
                    area.size = new Vector3(1f, 1f, 1f); // Reinicializar tamaño
                }
            }
        }

        private void OnSceneGUI()
        {
            AKLD_EventMultiBox manager = target as AKLD_EventMultiBox;

            if (manager != null)
            {
                foreach (var area in manager.areas)
                {
                    DrawAreaGizmo(manager, area);
                }
            }
        }

        private void DrawAreaGizmo(AKLD_EventMultiBox manager, AKLD_EventMultiBox.AreaData area)
        {
            Vector3 areaGlobalCenter = manager.transform.position + area.relativeCenter;

            Handles.color = area.gizmoColor;

            // Dibujar un cubo sin relleno para representar el área con rotación
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(areaGlobalCenter, area.rotation, Vector3.one);
            using (new Handles.DrawingScope(rotationMatrix))
            {
                Handles.DrawWireCube(Vector3.zero, area.size);
            }

            if (manager.showPositionGizmo)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.PositionHandle(areaGlobalCenter, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Move Area");
                    area.relativeCenter += newPosition - areaGlobalCenter;
                }
            }

            if (manager.showRotationGizmo)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion newRotation = Handles.RotationHandle(area.rotation, areaGlobalCenter);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Rotate Area");
                    area.rotation = newRotation;
                }
            }

            if (manager.showSizeGizmo)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newSize = Handles.ScaleHandle(area.size, areaGlobalCenter, area.rotation, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Resize Area");
                    area.size = newSize;
                }
            }
        }
    }
#endif
}


