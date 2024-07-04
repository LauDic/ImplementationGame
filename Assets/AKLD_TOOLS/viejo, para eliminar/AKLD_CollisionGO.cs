using UnityEngine;

public class AKLD_CollisionGO : CustomEditorBase
{
    // Nombre del evento que se enviará a Wwise
    public AK.Wwise.Event collisionEvent = null;

    // Nombre del RTPC que se utilizará para la velocidad del impacto
    public AK.Wwise.RTPC impactSpeedRTPC = null;

    // Valor máximo del RTPC para la velocidad del impacto
    public float maxImpactSpeed = 100.0f;

    // Etiquetas de los GameObjects que activarán la colisión
    public string[] collisionTags;

    // GameObjects específicos que activarán la colisión
    public GameObject[] collisionObjects;

    // Función llamada cuando ocurre una colisión
    void OnCollisionEnter(Collision collision)
    {
        // Verifica si el objeto que colisionó está en la lista de objetos específicos
        if (ArrayContainsGameObject(collision.gameObject, collisionObjects) || ArrayContainsTag(collision.gameObject.tag, collisionTags))
        {
            // Calcula la velocidad del impacto
            float impactSpeed = collision.relativeVelocity.magnitude;

            // Asegúrate de que la velocidad esté dentro del rango apropiado
            float clampedSpeed = Mathf.Clamp(impactSpeed, 0f, maxImpactSpeed);

            // Envía el valor del RTPC a Wwise
            if (impactSpeedRTPC != null)
            {
                impactSpeedRTPC.SetValue(gameObject, clampedSpeed);
            }
            else
            {
                Debug.LogWarning("El RTPC para la velocidad del impacto no está asignado.");
            }

            // Envía el evento a Wwise
            if (collisionEvent != null)
            {
                collisionEvent.Post(gameObject);
            }
            else
            {
                Debug.LogWarning("El evento de colisión no está asignado.");
            }

            Debug.Log("COLISIÓN DETECTADA CON VELOCIDAD: " + impactSpeed);
        }
    }

    // Función para verificar si un GameObject está en la lista de objetos específicos
    bool ArrayContainsGameObject(GameObject obj, GameObject[] array)
    {
        foreach (GameObject item in array)
        {
            if (item == obj)
            {
                return true;
            }
        }
        return false;
    }

    // Función para verificar si una etiqueta está en la lista de etiquetas
    bool ArrayContainsTag(string tag, string[] array)
    {
        foreach (string item in array)
        {
            if (item == tag)
            {
                return true;
            }
        }
        return false;
    }
}
