using UnityEngine;

public class SmokeBomb : MonoBehaviour
{

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Enemy enemy))
        {
            Debug.Log("Smoked an Enemy!");
            enemy.SmokeBomb();
        }
        
    }
}
