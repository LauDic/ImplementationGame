//Script created by Lautaro Dichio.
using UnityEditor;
using UnityEngine;

public class AKLD_CheckTerrainSwitch : CustomEditorBase
{
    // Variable to store the last detected tag
    private string lastDetectedTag = "";

    [SerializeField]
    private AK.Wwise.Switch metalSwitch;
    [SerializeField]
    private AK.Wwise.Switch grassSwitch;
    [SerializeField]
    private AK.Wwise.Switch woodSwitch;
    [SerializeField]
    private AK.Wwise.Switch dustSwitch;
    [SerializeField]
    private AK.Wwise.Switch waterSwitch;
    [SerializeField]
    private AK.Wwise.Switch sandSwitch;
    [SerializeField]
    private AK.Wwise.Switch rockSwitch;

    [SerializeField]
    private float raycastDistance = 2f;

    public GameObject gameobjectSwitch;

    [Header("For Debugging")]
    public bool tagChecking = false;

    void Update()
    {
        // Get the player's position
        Vector3 playerPosition = transform.position;

        // Cast a ray from the player's position downward
        Ray ray = new Ray(playerPosition, Vector3.down);

        // Perform the raycast
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // Check if the current tag is different from the last detected tag
            if (hit.collider.tag == lastDetectedTag)
            {
                return;
            }
            // Update the last detected tag
            lastDetectedTag = hit.collider.tag;

            // Change the Wwise switch based on the tag of the impacted object
            SetCurrentTerrain();
        }
        else
        {
            // No collision, reset the last detected tag
            lastDetectedTag = "";
        }
    }

    private void SetCurrentTerrain()
    {
        switch (lastDetectedTag)
        {
            case "Metal":
                metalSwitch.SetValue(gameobjectSwitch);
                Debug.Log("Switch changed to Metal");
                break;
            case "Grass":
                grassSwitch.SetValue(gameobjectSwitch);
                Debug.Log("Switch changed to Grass");
                break;
            case "Wood":
                woodSwitch.SetValue(gameobjectSwitch);
                Debug.Log("Switch changed to Wood");
                break;
            case "Dust":
                dustSwitch.SetValue(gameobjectSwitch);
                Debug.Log("Switch changed to Dust");
                break;
            case "Water":
                waterSwitch.SetValue(gameobjectSwitch);
                Debug.Log("Switch changed to water");
                break;
            case "Sand":
                sandSwitch.SetValue(gameobjectSwitch);
                Debug.Log("Switch changed to Sand");
                break;
            case "Rock":
                rockSwitch.SetValue(gameobjectSwitch);
                Debug.Log("Switch changed to Rock");
                break;
            default:
                // Optional: Show a warning message if the tag doesn't match any of the above cases. It's currently inactive.
                if (tagChecking) { Debug.LogWarning($"Unrecognized tag: {lastDetectedTag}"); }
                break;
        }
    }
}