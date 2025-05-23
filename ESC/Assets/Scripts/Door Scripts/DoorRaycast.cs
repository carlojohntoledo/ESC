using UnityEngine;

public class DoorRaycast : MonoBehaviour
{
    public float interactionDistance = 3f;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of screen
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                DoorInteraction door = hit.collider.GetComponent<DoorInteraction>();
                if (door != null)
                {
                    door.ToggleDoor();
                }
            }
        }
    }
}
