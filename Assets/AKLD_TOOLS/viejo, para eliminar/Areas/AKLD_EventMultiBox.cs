using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AKLD_EventMultiBox : MonoBehaviour
{
    public Transform objectToCheck;  // Objeto cuya posición se verifica para determinar la activación del área

    [System.Serializable]
    public class AreaData
    {
        [Header("Box")]
        public Vector3 relativeCenter = Vector3.zero;
        public Vector3 size = new Vector3(1f, 1f, 1f);
        public Color gizmoColor = Color.yellow;

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
            bool isInside = IsInsideArea(objectToCheck.position, area);

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

                if (area.stopEventOnExit && area.enterEvent != null)
                {
                    StopEvent(area.enterEvent);
                }

                if (area.OnExit && area.areaActivated)
                {
                    EventOnExit(area.eventOnExit);
                    area.exitedOnce = true;
                }
            }

            if (isInside && area.exitedOnce)
            {
                area.exitedOnce = false;
            }
        }
    }

    private bool IsInsideArea(Vector3 position, AreaData area)
    {
        Vector3 areaCenter = transform.position + area.relativeCenter;
        Vector3 halfSize = area.size * 0.5f;

        return (position.x > areaCenter.x - halfSize.x && position.x < areaCenter.x + halfSize.x &&
                position.y > areaCenter.y - halfSize.y && position.y < areaCenter.y + halfSize.y &&
                position.z > areaCenter.z - halfSize.z && position.z < areaCenter.z + halfSize.z);
    }

    private void UpdateEvent(AK.Wwise.Event myEvent)
    {
        myEvent.Post(this.gameObject);
    }

    private void StopEvent(AK.Wwise.Event myEvent)
    {
        myEvent.Stop(this.gameObject);
    }

    private void EventOnExit(AK.Wwise.Event myEvent)
    {
        myEvent.Post(this.gameObject);
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

            // Apply changes
            serializedObject.ApplyModifiedProperties();

            // Botón para inicializar tamaños
            AKLD_EventMultiBox manager = target as AKLD_EventMultiBox;
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

            // Dibujar un cubo sin relleno para representar el área
            Handles.DrawWireCube(areaGlobalCenter, area.size);

            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(areaGlobalCenter, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(manager, "Move Area");
                area.relativeCenter += newPosition - areaGlobalCenter;
            }
        }

    }
#endif
}
