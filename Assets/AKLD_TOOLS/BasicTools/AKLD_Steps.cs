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
    private bool floorCheck = false;

    private CharacterController _controller;
    private float _verticalVelocity;
    public bool IsFalling { get; private set; }

    public float smoothTime = 0.3f; // Tiempo de suavizado
    private float velocity = 0f; // Almacena la velocidad de cambio
    private float velocidadSuavizada = 0f; // La velocidad suavizada
    public float smoothCoeficiente = 0.1f; // Coeficiente de suavizado


    private void Start()
    {
        if (rigidbodyToMeasure == null)
        {
            rigidbodyToMeasure = GetComponent<Rigidbody>(); // Obtener el Rigidbody del GameObject actual si no se ha especificado uno en el inspector
        }

        posicionAnterior = transform.position;



        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            Debug.LogError("No se encontró el CharacterController en el objeto.");
        }
    }

    private void Update()
    {

        //si estamos yendo para abajo, no postea el sonido de salto.
        if (_controller != null)
        {
            _verticalVelocity = _controller.velocity.y;
            IsFalling = _verticalVelocity < 0;

        }


        isGrounded = Physics.Raycast(transform.position, Vector3.down, longitudRaycast);

        if (!isGrounded && !saltoPosteado && !floorCheck && !IsFalling)
        {
            saltoEvent?.Post(gameObject);
            saltoPosteado = true;
            return;
        }

        if (isGrounded && saltoPosteado)
        {
            saltoPosteado = false;
        }

        if (!wasGroundedLastFrame && isGrounded && floorCheck)
        {
            fallEvent?.Post(gameObject);
        }

        wasGroundedLastFrame = isGrounded;

        // Leer la velocidad del Rigidbody seleccionado usando la propiedad velocity
        float velocidadActual = rigidbodyToMeasure.velocity.magnitude;

        // Debug.Log("Speed: " + velocidadActual);

        if (!isGrounded)
           return;

        if (IsFalling)
           return;


        if (velocidadActual < velocidadMinimaDetener && !sePosteoStopWalk)
        {
            stopWalkEvent?.Post(gameObject);
            sePosteoStopWalk = true;
        }

        if (velocidadActual > velocidadMinimaDetener && sePosteoStopWalk)
        {
            sePosteoStopWalk = false;
        }

        /*
        // Calcular factorExponencial de manera logarítmica ascendente
        float factorExponencial = Mathf.Pow((velocidadActual - velocidadMinima) / (velocidadMaxima - velocidadMinima), 2f);
        factorExponencial = Mathf.Clamp01(factorExponencial);
        

        if (velocidadRTPC != null)
        {
            float valorRTPC = velocidadActual / velocidadMaxima;
            velocidadRTPC.SetValue(this.gameObject, velocidadActual);

        }
        */

        // Solo suavizar si la velocidad actual es mayor que la velocidad suavizada (solo ascendente)
        if (velocidadActual > velocidadSuavizada)
        {
            velocidadSuavizada = Mathf.Lerp(velocidadSuavizada, velocidadActual, smoothCoeficiente * Time.deltaTime);
        }
        else
        {
            velocidadSuavizada = velocidadActual; // Si la velocidad actual es menor o igual, no suavizar
        }

        // Calcular factorExponencial de manera logarítmica ascendente usando la velocidad suavizada
        float factorExponencial = Mathf.Pow((velocidadSuavizada - velocidadMinima) / (velocidadMaxima - velocidadMinima), 2f);
        factorExponencial = Mathf.Clamp01(factorExponencial);

        // Verificar si velocidadRTPC no es nulo y ajustar el valor RTPC
        if (velocidadRTPC != null)
        {
            // Calcular valor RTPC usando la velocidad suavizada
            float valorRTPC = velocidadSuavizada / velocidadMaxima;
            velocidadRTPC.SetValue(this.gameObject, valorRTPC);
        }

        // Imprimir la velocidad actual del Rigidbody
        Debug.Log("Speed-> " + velocidadActual);
    

        // Ajustar la distancia de paso en función de la velocidad actual y la curva logarítmica ascendente
        float distanciaEntrePasos = Mathf.Lerp(distanciaMinimaPaso, distanciaMaximaPaso, factorExponencial);


        // Lógica para determinar el momento de postear eventos basado en la distancia recorrida
        float distanciaFrame = Vector3.Distance(transform.position, posicionAnterior);
        distanciaRecorrida += distanciaFrame;

        if (distanciaRecorrida >= distanciaEntrePasos)
        {
            // Imprimir la distancia recorrida entre pasos
            //Debug.Log("Distancia entre pasos: " + distanciaRecorrida);

            eventoPaso?.Post(gameObject);
            distanciaRecorrida = 0f;
        }

        // Actualizar la posición anterior para el siguiente frame
        posicionAnterior = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Floor"))
        {
            floorCheck = true;
            //Debug.Log("Collider: Tocando el suelo");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Floor"))
        {
            floorCheck = true;
            //Debug.Log("Collider: OnTriggerStay");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Floor"))
        {
            floorCheck = false;
            //Debug.Log("Collider: Dejó de tocar el suelo ->" + floorCheck);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * longitudRaycast);
    }
}
