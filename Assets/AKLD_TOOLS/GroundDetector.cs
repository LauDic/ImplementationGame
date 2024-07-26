using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    public float rayDistance = 1.0f; // Distancia del raycast
    public Color rayColor = Color.red; // Color del raycast en la vista de la escena
    public bool isGrounded = false; // Variable para verificar si está en el suelo

    private bool previousGroundedState = false;

    private void Update()
    {
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
    }
}
