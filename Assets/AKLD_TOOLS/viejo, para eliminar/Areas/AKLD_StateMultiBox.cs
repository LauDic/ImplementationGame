using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AKLD_StateMultiBox : MonoBehaviour
{
    public Transform objectToCheck;  // Objeto cuya posici�n se verifica para determinar la activaci�n del �rea

    [System.Serializable]
    public class AreaData
    {
        // Configuraci�n de la zona en forma de caja
        [Header("Box")]
        public Vector3 relativeCenter = Vector3.zero;  // Centro relativo de la zona con respecto al objeto principal
        public Vector3 size = new Vector3(1f, 1f, 1f);  // Tama�o de la zona en cada dimensi�n (x, y, z)
        public Color gizmoColor = Color.yellow;  // Color del gizmo en el editor

        // Configuraci�n del evento Wwise
        [Header("Wwise Al Entrar")]
        public AK.Wwise.State enterState = null;  // Estado de Wwise asociado con la zona
        public bool DebugOn = false;
        public string message = "Dentro del �rea";  // Mensaje para mostrar cuando se activa la zona

        [Header("Al Salir")]
        public bool sendExitStateOnce = true;  // Indica si enviar el estado de salida una vez
        public AK.Wwise.State stateOnExit = null;
        [HideInInspector]
        public bool hasEntered = false; // Indica si el objeto ha entrado en el �rea

        // Constructor predeterminado para serializaci�n
        public AreaData() { }

        // M�todo de inicializaci�n
        public void Initialize()
        {
            // Inicializar el tama�o con valores m�nimos de 1 en cada dimensi�n
            size = new Vector3(Mathf.Max(size.x, 1f), Mathf.Max(size.y, 1f), Mathf.Max(size.z, 1f));
        }
    }

    // Lista de datos de �reas
    public List<AreaData> areas = new List<AreaData>();

    private void Start()
    {
        foreach (var area in areas)
        {
            area.Initialize();  // Inicializar cada �rea
        }
    }

    private void Update()
    {
        if (objectToCheck == null)
        {
            return;
        }
        foreach (var area in areas)
        {
            // Verificar si el objeto est� dentro de la zona y la zona no est� activada
            if (IsInsideArea(objectToCheck.position, area) && !area.hasEntered)
            {
                if (area.DebugOn)
                    Debug.Log(area.message);  // Mostrar el mensaje de la zona

                // Activar el evento si est� configurado
                if (area.enterState != null)
                {
                    UpdateState(area.enterState);
                }

                area.hasEntered = true;  // Marcar la zona como ingresada
            }

            // Si el objeto no est� dentro de la zona
            else if (!IsInsideArea(objectToCheck.position, area))
            {
                if (area.sendExitStateOnce && area.hasEntered)
                {
                    if (area.stateOnExit != null)
                    {
                        StateOnExit(area.stateOnExit);
                    }
                    area.hasEntered = false; // Restablecer el estado ingresado
                }
            }
        }
    }

    // Determina si una posici�n est� dentro de una zona
    private bool IsInsideArea(Vector3 position, AreaData area)
    {
        Vector3 areaCenter = transform.position + area.relativeCenter;  // Centro global de la zona
        Vector3 minBound = areaCenter - area.size * 0.5f;
        Vector3 maxBound = areaCenter + area.size * 0.5f;

        return position.x > minBound.x && position.x < maxBound.x &&
               position.y > minBound.y && position.y < maxBound.y &&
               position.z > minBound.z && position.z < maxBound.z;
    }

    private void UpdateState(AK.Wwise.State myState)
    {
        myState.SetValue();
    }

    private void StateOnExit(AK.Wwise.State myState)
    {
        myState.SetValue();
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(AKLD_StateMultiBox))]
public class AKLD_StateMultiBoxEditor : Editor
{
    private Texture2D image;
    private SerializedProperty m_Script; // Referencia al campo m_Script

    private void OnEnable()
    {
        // Obtener la referencia al campo m_Script
        m_Script = serializedObject.FindProperty("m_Script");

        // Cargar la imagen desde los recursos
        image = Resources.Load<Texture2D>("Titulo script 7");

        // Crear una caja por defecto si la lista de �reas est� vac�a
        if (((AKLD_StateMultiBox)target).areas.Count == 0)
        {
            ((AKLD_StateMultiBox)target).areas.Add(new AKLD_StateMultiBox.AreaData());
        }
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
            EditorGUILayout.HelpBox("No se pudo cargar la imagen. Aseg�rate de que est� en la carpeta Resources.", MessageType.Warning);
        }

        // Mostrar propiedades excepto m_Script
        DrawPropertiesExcluding(serializedObject, "m_Script");

        // Apply changes
        serializedObject.ApplyModifiedProperties();

        // Bot�n para inicializar tama�os
        AKLD_StateMultiBox manager = target as AKLD_StateMultiBox;
        if (GUILayout.Button("Initialize Sizes") && manager != null)
        {
            InitializeSizes(manager);
        }
    }

    private void InitializeSizes(AKLD_StateMultiBox manager)
    {
        if (manager != null)
        {
            foreach (var area in manager.areas)
            {
                // Reinicializar tama�o con un m�nimo de 1 en cada dimensi�n
                area.size = Vector3.one;
            }
        }
    }

    private void OnSceneGUI()
    {
        AKLD_StateMultiBox manager = target as AKLD_StateMultiBox;

        if (manager != null)
        {
            foreach (var area in manager.areas)
            {
                DrawAreaGizmo(manager, area);
            }
        }
    }

    private void DrawAreaGizmo(AKLD_StateMultiBox manager, AKLD_StateMultiBox.AreaData area)
    {
        // Calcular la posici�n global del �rea
        Vector3 areaGlobalCenter = manager.transform.position + area.relativeCenter;

        // Dibujar un cubo sin relleno para representar el �rea
        Handles.color = area.gizmoColor;
        Handles.DrawWireCube(areaGlobalCenter, area.size);

        // Permitir que el �rea se mueva en el editor
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(areaGlobalCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            // Registrar el cambio de posici�n del �rea
            Undo.RecordObject(manager, "Move Area");
            area.relativeCenter += newPosition - areaGlobalCenter;
        }
    }
}
#endif