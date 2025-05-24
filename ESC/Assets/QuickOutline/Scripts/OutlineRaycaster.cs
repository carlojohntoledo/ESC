using UnityEngine;

public class OutlineRaycaster : MonoBehaviour
{
    public Camera cam; // Drag your main camera here
    public float maxDistance = 100f;

    private Outline currentOutline;

    void Update()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = cam.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            Outline newOutline = hit.collider.GetComponent<Outline>();

            if (newOutline != null)
            {
                if (currentOutline != null && currentOutline != newOutline)
                {
                    currentOutline.DisableOutline();
                }

                newOutline.EnableOutline();
                currentOutline = newOutline;
            }
            else
            {
                if (currentOutline != null)
                {
                    currentOutline.DisableOutline();
                    currentOutline = null;
                }
            }
        }
        else
        {
            if (currentOutline != null)
            {
                currentOutline.DisableOutline();
                currentOutline = null;
            }
        }
    }
}
