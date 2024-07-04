// Script creado por Lautaro Dichio
// Scriptable Object Template, para trabajar mas comodo. Instrucciones al final.

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


[CreateAssetMenu(menuName = "SO/Audio/Template", fileName = "New Template")]
public class AKLD_SOTemplate : ScriptableObject
{
    [System.Serializable]
    public class ComponentEvent
    {
        public string componentName;
        public AK.Wwise.Event eventComponent;
    }

    [System.Serializable]
    public class ComponentRTPC
    {
        public string componentName;
        public AK.Wwise.RTPC rtpcComponent;
    }

    [System.Serializable]
    public class ComponentSwitch
    {
        public string componentName;
        public AK.Wwise.Switch switchComponent;
    }

    [System.Serializable]
    public class ComponentState
    {
        public string componentName;
        public AK.Wwise.State stateComponent;
    }

    [Header("Event")]
    public List<ComponentEvent> eventComponents = new List<ComponentEvent>();

    [Header("Rtpc")]
    public List<ComponentRTPC> rtpcComponents = new List<ComponentRTPC>();

    [Header("Switch")]
    public List<ComponentSwitch> switchComponents = new List<ComponentSwitch>();

    [Header("State")]
    public List<ComponentState> stateComponents = new List<ComponentState>();

    // Método para buscar un componente por nombre
    private T GetComponentByName<T>(string componentName, List<T> componentList) where T : class
    {
        return componentList.Find(item => string.Equals((string)item.GetType().GetField("componentName").GetValue(item), componentName));
    }


    // Métodos para buscar componentes específicos por nombre
    public AK.Wwise.Event GetEventComponent(string componentName)
    {
        return GetComponentByName(componentName, eventComponents)?.eventComponent;
    }

    public AK.Wwise.RTPC GetRTPCComponent(string componentName)
    {
        return GetComponentByName(componentName, rtpcComponents)?.rtpcComponent;
    }

    public AK.Wwise.Switch GetSwitchComponent(string componentName)
    {
        return GetComponentByName(componentName, switchComponents)?.switchComponent;
    }

    public AK.Wwise.State GetStateComponent(string componentName)
    {
        return GetComponentByName(componentName, stateComponents)?.stateComponent;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AKLD_SOTemplate))]
public class AKLD_SOTemplateEditor : Editor
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
            EditorGUILayout.HelpBox("No se pudo cargar la imagen. Asegúrate de que esté en la carpeta Resources.", MessageType.Warning);
        }

        // Mostrar los campos predeterminados del script excepto el campo m_Script
        DrawPropertiesExcluding(serializedObject, "m_Script");

        // Aplicar los cambios al objeto serializado
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
/*
       Como llamar? 
1. Declarar un template en un script cualquiera y ponerle un nombre, ej sOTemplate. 
2. Asignarlo manualmente el Scriptable Object.
3. Configurar segun como queramos. 
4. Hacer la llamada:
        SOTemplate.GetEventComponent("Nombre").Post(this.gameObject);
        SOTemplate.GetSwitchComponent("Nombre").SetValue(this.gameObject);
        SOTemplate.GetRTPCComponent("Nombre").SetValue(this.gameObject, valorDeseado);
        SOTemplate.GetStateComponent("Nombre").SetValue();
 */