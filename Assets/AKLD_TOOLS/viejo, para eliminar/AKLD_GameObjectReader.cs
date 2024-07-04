using UnityEngine;
using System.Collections.Generic;

public enum Direction { Adelante, Atrás, Arriba, Abajo, Izquierda, Derecha }

public class AKLD_GameObjectReader : CustomEditorBase
{
    // Dirección seleccionable desde la lista
    public Direction selectedDirection = Direction.Adelante;

    // Distancia máxima para detectar Game Objects
    public float maxDistance = 10.0f;

    // Lista de chequeadores para Game Objects
    public List<GameObjectChecker> checkers = new List<GameObjectChecker>();

    // Activar o desactivar el debug.log del nombre del Game Object
    public bool debugObjectName = false;

    // Mostrar el raycast solo en el editor
    public bool showRaycast = true;

    // Si la cámara afecta la dirección del rayo para adelante, atrás, izquierda y derecha
    public bool cameraAffectsDirection = false;

    // Referencia a la cámara
    public GameObject cameraObject;

    private GameObject GO;
    private Camera cam;

    // Para almacenar el último objeto detectado
    private GameObject lastHitObject;

    void Start()
    {
        GO = this.gameObject;
        cam = cameraObject != null ? cameraObject.GetComponent<Camera>() : Camera.main;
    }

    void Update()
    {
        // Obtener la posición de inicio para la lectura
        Vector3 origin = transform.position;

        // Definir la dirección del rayo en función de la dirección seleccionada
        Vector3 rayDirection = Vector3.zero;
        switch (selectedDirection)
        {
            case Direction.Adelante:
                rayDirection = GetForwardDirection();
                break;
            case Direction.Atrás:
                rayDirection = GetBackwardDirection();
                break;
            case Direction.Arriba:
                rayDirection = GetUpDirection();
                break;
            case Direction.Abajo:
                rayDirection = GetDownDirection();
                break;
            case Direction.Izquierda:
                rayDirection = GetLeftDirection();
                break;
            case Direction.Derecha:
                rayDirection = GetRightDirection();
                break;
        }

        // Rayo para detectar Game Objects
        Ray ray = new Ray(origin, rayDirection);

        // Visualizar el raycast solo en el editor
        if (showRaycast)
        {
            Debug.DrawRay(origin, rayDirection * maxDistance, Color.red);
        }

        // Variable para almacenar la información de la colisión
        RaycastHit hit;

        // Realizar la detección de Game Objects
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Verificar si el objeto golpeado es un Game Object
            GameObject hitObject = hit.collider.gameObject;

            // Debug.log del nombre del Game Object si está activado
            if (debugObjectName)
            {
                Debug.Log("GameObject Name: " + hitObject.name);
            }

            // Si el objeto detectado cambió, detener el evento anterior y restablecer el evento enviado
            if (lastHitObject != null && lastHitObject != hitObject)
            {
                foreach (var checker in checkers)
                {
                    if (checker.IsValidGameObject(lastHitObject))
                    {
                        checker.StopWwiseEvent(GO, lastHitObject);
                        checker.ResetEventSent();
                        break; // Salir del bucle una vez que se haya detenido el evento
                    }
                }
            }

            // Verificar si el Game Object coincide con algún chequeador
            foreach (var checker in checkers)
            {
                if (checker.IsValidGameObject(hitObject) && !checker.eventSent)
                {
                    // Enviar el evento de Wwise asociado al chequeador válido
                    checker.SendWwiseEvent(GO, hitObject);
                    checker.eventSent = true;
                    break; // Salir del bucle una vez que se haya enviado el evento
                }
            }

            // Almacenar el objeto detectado para la próxima comparación
            lastHitObject = hitObject;
        }
        else
        {
            // Si no hay objeto detectado, detener el evento anterior y restablecer el evento enviado
            if (lastHitObject != null)
            {
                foreach (var checker in checkers)
                {
                    if (checker.IsValidGameObject(lastHitObject))
                    {
                        checker.StopWwiseEvent(GO, lastHitObject);
                        checker.ResetEventSent();
                        break; // Salir del bucle una vez que se haya detenido el evento
                    }
                }

                // Limpiar el último objeto detectado
                lastHitObject = null;
            }
        }
    }

    // Función para obtener la dirección hacia adelante
    private Vector3 GetForwardDirection()
    {
        if (cameraAffectsDirection && cam != null)
        {
            return cam.transform.forward;
        }
        else
        {
            return transform.forward;
        }
    }

    // Función para obtener la dirección hacia atrás
    private Vector3 GetBackwardDirection()
    {
        if (cameraAffectsDirection && cam != null)
        {
            return -cam.transform.forward;
        }
        else
        {
            return -transform.forward;
        }
    }

    // Función para obtener la dirección hacia arriba
    private Vector3 GetUpDirection()
    {
        return Vector3.up;
    }

    // Función para obtener la dirección hacia abajo
    private Vector3 GetDownDirection()
    {
        return Vector3.down;
    }

    // Función para obtener la dirección hacia la izquierda
    private Vector3 GetLeftDirection()
    {
        if (cameraAffectsDirection && cam != null)
        {
            return -cam.transform.right;
        }
        else
        {
            return -transform.right;
        }
    }

    // Función para obtener la dirección hacia la derecha
    private Vector3 GetRightDirection()
    {
        if (cameraAffectsDirection && cam != null)
        {
            return cam.transform.right;
        }
        else
        {
            return transform.right;
        }
    }
}

[System.Serializable]
public class GameObjectChecker
{
    // Nombre del Game Object que se espera detectar
    public GameObject expectedGameObject;
    public bool postOnOther;

    // Evento de Wwise a enviar si se encuentra el Game Object correcto
    public AK.Wwise.Event eventToSend;

    // Evento de Wwise para detener si se aleja del Game Object correcto
    public AK.Wwise.Event eventToStop;

    // Para rastrear si el evento ya se envió
    public bool eventSent = false;

    // Función para verificar si el Game Object coincide con el nombre esperado
    public bool IsValidGameObject(GameObject gameObject)
    {
        return gameObject == expectedGameObject;
    }

    // Función para enviar el evento de Wwise asociado al chequeador válido
    public void SendWwiseEvent(GameObject _GO, GameObject other)
    {
        if (postOnOther)
            eventToSend.Post(other);
        else
            eventToSend.Post(_GO);
    }

    // Función para detener el evento de Wwise asociado al chequeador
    public void StopWwiseEvent(GameObject _GO, GameObject other)
    {
        if (postOnOther)
            eventToStop.Post(other);
        else
            eventToStop.Post(_GO);
    }

    // Función para restablecer el estado de evento enviado
    public void ResetEventSent()
    {
        eventSent = false;
    }
}
