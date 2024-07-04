using System.Collections.Generic;
using UnityEngine;

public class AKLD_LightToRTPC : MonoBehaviour
{
    // Listas para almacenar luces de cada tipo
    public List<Light> lucesPuntuales = new List<Light>();
    public List<Light> lucesDirecionales = new List<Light>();
    public List<Light> lucesFoco = new List<Light>();
    public List<Light> lucesArea = new List<Light>();

    // RTPC de Wwise para controlar la iluminación
    public AK.Wwise.RTPC rtpc;

    // Método para actualizar las listas de luces por tipo y enviar el RTPC
    void ActualizarListasDeLucesYRTPC()
    {
        // Limpiar las listas antes de actualizarlas
        lucesPuntuales.Clear();
        lucesDirecionales.Clear();
        lucesFoco.Clear();
        lucesArea.Clear();

        // Encontrar todas las luces en la escena
        Light[] todasLasLuces = FindObjectsOfType<Light>();

        // Iterar sobre todas las luces y clasificarlas por tipo
        foreach (Light luz in todasLasLuces)
        {
            switch (luz.type)
            {
                case LightType.Point:
                    lucesPuntuales.Add(luz);
                    break;
                case LightType.Directional:
                    lucesDirecionales.Add(luz);
                    break;
                case LightType.Spot:
                    lucesFoco.Add(luz);
                    break;
                case LightType.Area:
                    lucesArea.Add(luz);
                    break;
                default:
                    break;
            }
        }

        // Calcular la intensidad total de todas las luces
        float intensidadTotal = 0f;
        foreach (Light luz in todasLasLuces)
        {
            // Lanzar un Raycast desde el objeto hacia la luz para detectar obstrucciones
            RaycastHit hit;
            if (Physics.Raycast(transform.position, (luz.transform.position - transform.position).normalized, out hit))
            {
                // Si el raycast golpea un objeto, restar su intensidad como sombra
                if (hit.collider.gameObject != luz.gameObject)
                {
                    intensidadTotal += luz.intensity * (1f - hit.distance / Vector3.Distance(transform.position, luz.transform.position));
                }
            }
            else
            {
                // Si no hay obstrucciones, sumar la intensidad de la luz
                intensidadTotal += luz.intensity;
            }

            // Dibujar el raycast en la escena para depuración
            Debug.DrawRay(transform.position, (luz.transform.position - transform.position).normalized * 10f, Color.green);
        }

        // Enviar la intensidad total como RTPC a Wwise
        rtpc.SetGlobalValue(intensidadTotal);
    }

    // Método que se llama cada fotograma
    void Update()
    {
        // Actualizar las listas de luces por tipo y enviar el RTPC
        ActualizarListasDeLucesYRTPC();
    }
}
