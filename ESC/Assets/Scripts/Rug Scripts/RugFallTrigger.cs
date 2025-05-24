using UnityEngine;

public class RugFallTrigger : MonoBehaviour
{
    public Cloth rugCloth;
    private bool hasFallen = false;

    void OnTriggerEnter(Collider other)
    {
        if (!hasFallen && other.CompareTag("Player"))
        {
            // Unpin the cloth by setting maxDistance on all vertices
            var constraints = rugCloth.coefficients;
            for (int i = 0; i < constraints.Length; i++)
            {
                constraints[i].maxDistance = 0.5f; // or any non-zero value
            }
            rugCloth.coefficients = constraints;

            hasFallen = true;
        }
    }
}
