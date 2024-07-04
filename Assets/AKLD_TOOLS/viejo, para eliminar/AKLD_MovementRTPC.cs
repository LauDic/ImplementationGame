using UnityEditor;
using UnityEngine;

public class AKLD_MovementRTPC : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AK.Wwise.RTPC RTPCVelocity; // Nombre del RTPC en Wwise
    public Vector3 velocidad; // Variable para mostrar en el Hierarchy como Vector3
    private Vector3 previousPosition;

    private void Start()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody not assigned. Please assign a Rigidbody to the script.");
            rb = GetComponent<Rigidbody>();
        }

        // Inicializa el RTPC a un valor predeterminado si es necesario
        RTPCVelocity.SetValue(this.gameObject, 0.0f);
        previousPosition = rb.position;
    }

    private void Update()
    {
        if (rb != null)
        {
            Vector3 currentPosition = rb.position;
            Vector3 currentVelocity;

            // Si el Rigidbody es cinemático, calcular la velocidad manualmente
            if (rb.isKinematic)
            {
                currentVelocity = (currentPosition - previousPosition) / Time.deltaTime;
            }
            else
            {
                // Obtener la velocidad actual del Rigidbody si no es cinemático
                currentVelocity = rb.velocity;
            }

            // Actualizar el RTPC en Wwise con la magnitud de la velocidad (un solo número)
            RTPCVelocity.SetValue(this.gameObject, currentVelocity.magnitude);

            // Actualizar la variable de visualización para el Hierarchy
            velocidad = currentVelocity;

            // Actualizar la posición anterior
            previousPosition = currentPosition;
        }
    }

    // Mostrar la variable velocidad en el Hierarchy
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Velocidad: " + velocidad.ToString("F2"));
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AKLD_MovementRTPC))]
    public class AKLD_MovementRTPCEditor : Editor
    {
        private Texture2D image;
        private SerializedProperty m_Script;

        private void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            image = Resources.Load<Texture2D>("Titulo script 7");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Mostrar la imagen en el Inspector (arriba de todo) si está disponible
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

            // Mostrar todos los campos del script excepto el campo m_Script
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}

