using UnityEngine;
using AK.Wwise;

public class AKLD_Steps : MonoBehaviour
{
    [Header("Configuración de Velocidades")]
    public float velocidadMinima = 1f;
    public float velocidadMaxima = 10f;
    public float velocidadMinimaDetener = 0.2f;

    [Header("Configuración de Distancias")]
    public float distanciaMinimaPaso = 0.4f;
    public float distanciaMaximaPaso = 2f;

    [Header("Configuración de Raycast")]
    public float longitudRaycast = 0.5f;
    public bool isGrounded;

    [Header("Eventos de Wwise")]
    public AK.Wwise.RTPC velocidadRTPC;
    public AK.Wwise.Event eventoPaso;
    public AK.Wwise.Event stopWalkEvent;
    public AK.Wwise.Event saltoEvent;
    public AK.Wwise.Event fallEvent;

    [Header("Rigidbody")]
    public Rigidbody rigidbodyToMeasure; // Rigidbody seleccionable desde el inspector

    private Vector3 posicionAnterior;
    private float distanciaRecorrida;
    private bool sePosteoStopWalk = false;
    private bool saltoPosteado = false;
    private bool wasGroundedLastFrame = true;

    private void Start()
    {
        if (rigidbodyToMeasure == null)
        {
            rigidbodyToMeasure = GetComponent<Rigidbody>(); // Obtener el Rigidbody del GameObject actual si no se ha especificado uno en el inspector
        }

        posicionAnterior = transform.position;
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, longitudRaycast);

        if (!isGrounded && !saltoPosteado)
        {
            saltoEvent?.Post(gameObject);
            saltoPosteado = true;
            return;
        }

        if (isGrounded && saltoPosteado)
        {
            saltoPosteado = false;
        }

        if (!wasGroundedLastFrame && isGrounded)
        {
            fallEvent?.Post(gameObject);
        }

        wasGroundedLastFrame = isGrounded;

        if (!isGrounded)
            return;

        // Leer la velocidad del Rigidbody seleccionado usando la propiedad velocity
        float velocidadActual = rigidbodyToMeasure.velocity.magnitude;

        if (velocidadActual < velocidadMinimaDetener && !sePosteoStopWalk)
        {
            stopWalkEvent?.Post(gameObject);
            sePosteoStopWalk = true;
        }

        if (velocidadActual > velocidadMinimaDetener && sePosteoStopWalk)
        {
            sePosteoStopWalk = false;
        }

        // Calcular factorExponencial de manera logarítmica ascendente
        float factorExponencial = Mathf.Pow((velocidadActual - velocidadMinima) / (velocidadMaxima - velocidadMinima), 2f);
        factorExponencial = Mathf.Clamp01(factorExponencial);

        // Ajustar la distancia de paso en función de la velocidad actual y la curva logarítmica ascendente
        float distanciaEntrePasos = Mathf.Lerp(distanciaMinimaPaso, distanciaMaximaPaso, factorExponencial);

        if (velocidadRTPC != null)
        {
            float valorRTPC = velocidadActual / velocidadMaxima;
            velocidadRTPC.SetValue(gameObject, valorRTPC);
        }

        // Imprimir la velocidad actual del Rigidbody
        Debug.Log("Speed: " + velocidadActual);

        // Lógica para determinar el momento de postear eventos basado en la distancia recorrida
        float distanciaFrame = Vector3.Distance(transform.position, posicionAnterior);
        distanciaRecorrida += distanciaFrame;

        if (distanciaRecorrida >= distanciaEntrePasos)
        {
            // Imprimir la distancia recorrida entre pasos
            Debug.Log("Distancia entre pasos: " + distanciaRecorrida);

            eventoPaso?.Post(gameObject);
            distanciaRecorrida = 0f;
        }

        // Actualizar la posición anterior para el siguiente frame
        posicionAnterior = transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * longitudRaycast);
    }
}
