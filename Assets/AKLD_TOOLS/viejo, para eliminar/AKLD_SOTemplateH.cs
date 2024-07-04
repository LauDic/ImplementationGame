using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Audio/Player", fileName = "New Player Sheet")]
public class AKLD_SOTemplateH : ScriptableObject
{
    [Header("Movement")]

    public AK.Wwise.Event footsteps = null;
    public AK.Wwise.Event jump = null;
    public AK.Wwise.Event fall = null;
    public AK.Wwise.Event dash = null;
    public AK.Wwise.Event run = null;


    [Header("Attack - Defense")]

    public AK.Wwise.Event basicAttack = null;
    public AK.Wwise.Event chargedAttack = null;
    public AK.Wwise.Event swordAttack = null;
    public AK.Wwise.Event blockEvent = null;

    [Header("Others")]

    public AK.Wwise.Event interact = null;
    public AK.Wwise.Event pickUp = null;
    public AK.Wwise.Event open = null;

    [Header("Rtpc")]

    public AK.Wwise.RTPC test1 = null;



    [Header("state")]

    public AK.Wwise.Switch Switch = null; 


    [Header("State")]

    public AK.Wwise.State State = null;

}
#if UNITY_EDITOR

[CustomEditor(typeof(AKLD_SOTemplateH))]
public class AKLD_SOTemplateHEditor : Editor
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