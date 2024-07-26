using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AKLDSteps : MonoBehaviour
{
    [Header("Eventos de Wwise")]
    public AK.Wwise.RTPC velocidadRTPC;


    public AK.Wwise.Event eventoPaso;

    public AK.Wwise.Event stopWalkEvent;

    public AK.Wwise.Event saltoEvent;

    public AK.Wwise.Event fallEvent;



    //variables de la velocidad.
    private Vector3 previousPosition;
    private float horizontalSpeed;
    private float upwardSpeed;
    private float verticalSpeed;

    private Vector3 lastVelocity;
    private float velocityThreshold = 0.01f; // Umbral para considerar el movimiento


    //demas
    [Header("Configuraciones de Velocidad")]
    [Tooltip("Velocidad mínima hacia arriba para imprimir 'SALTO'.")]
    public float upwardVelocityThreshold = 5.0f; // Umbral de velocidad hacia arriba
    private bool hasPrintedSalto = false;

    //ray
    public float rayDistance = 1.0f; // Distancia del raycast
    public Color rayColor = Color.red; // Color del raycast en la vista de la escena
    public bool isGrounded = false; // Variable para verificar si está en el suelo
    private bool previousGroundedState = false;

    void Start()
    {
        // Inicializa la posición anterior con la posición actual
        previousPosition = transform.position;
        lastVelocity = Vector3.zero;
    }

    void Update()
    {
        //rayo
        // Solo ejecutar la lógica cuando el juego esté en modo de juego
        if (Application.isPlaying)
        {
            // Mostrar el raycast en la vista de la escena
            Debug.DrawRay(transform.position, Vector3.down * rayDistance, rayColor);

            // Lanzar raycasts y obtener todos los objetos tocados
            RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, rayDistance);

            // Verificar todos los objetos tocados
            bool foundFloor = false;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Floor"))
                {
                    foundFloor = true;
                    Debug.Log("Raycast hit: " + hit.collider.name);
                    break; // Salir del bucle una vez que encontramos el primer "Floor"
                }
            }

            // Actualizar isGrounded basado en si se encontró un objeto con el tag "Floor"
            isGrounded = foundFloor;

            // Imprimir mensajes si el estado de isGrounded ha cambiado
            if (isGrounded != previousGroundedState)
            {
                if (isGrounded)
                {
                    Debug.Log("isGrounded = true");
                }
                else
                {
                    Debug.Log("isGrounded = false");
                }
                previousGroundedState = isGrounded;
            }
        }





        // Calcula la diferencia de posición
        Vector3 deltaPosition = transform.position - previousPosition;

        // Guarda la posición actual para el próximo frame
        previousPosition = transform.position;

        // Calcula el tiempo de cuadro
        float deltaTime = Time.deltaTime;

        // Evita dividir por cero en caso de deltaTime muy pequeño
        if (deltaTime <= 0)
        {
            return;
        }

        // Calcula la velocidad instantánea
        Vector3 currentVelocity = deltaPosition / deltaTime;

        // Verifica si la velocidad es significativamente diferente de cero
        if (currentVelocity.magnitude > velocityThreshold)
        {
            lastVelocity = currentVelocity;

            // Velocidad en el plano horizontal (X y Z)
            Vector3 horizontalDelta = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            horizontalSpeed = horizontalDelta.magnitude;

            // Velocidad hacia arriba (solo cuando currentVelocity.y es positivo)
            upwardSpeed = currentVelocity.y > 0 ? currentVelocity.y : 0;

            // Velocidad en el eje vertical (Y)
            verticalSpeed = Mathf.Abs(currentVelocity.y);



        }
        else
        {
            // Si el objeto está quieto, establece las velocidades en cero
            horizontalSpeed = 0;
            upwardSpeed = 0;
            verticalSpeed = 0;

            hasPrintedSalto = false;
        }


        if (upwardSpeed > upwardVelocityThreshold && !hasPrintedSalto && !isGrounded)
        {
            //Debug.Log("Jump");

            saltoEvent.Post(gameObject);
            hasPrintedSalto = true;
        }
        else if (upwardSpeed == 0) hasPrintedSalto = false;


        // Opcional: Imprime las velocidades en la consola para verificar
        //Debug.Log("Velocidad horizontal: " + horizontalSpeed);
        //Debug.Log("Velocidad hacia arriba: " + upwardSpeed);
        //Debug.Log("Velocidad vertical: " + verticalSpeed);
    }
}

