/*#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomEditorBase), true)]
public class CustomScriptEditor : Editor
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
            EditorGUILayout.HelpBox("No se pudo cargar la imagen. Asegúrate de que está en la carpeta Resources.", MessageType.Warning);
        }

        // Mostrar los campos predeterminados del script excepto el campo m_Script
        DrawPropertiesExcluding(serializedObject, "m_Script");

        // Aplicar los cambios al objeto serializado
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
*/

 #if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomEditorBase), true)]
public class CustomScriptEditor : Editor
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
