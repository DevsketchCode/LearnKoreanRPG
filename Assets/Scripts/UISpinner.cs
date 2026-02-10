using UnityEngine;

public class UISpinner : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 200f;

    void Update()
    {
        // Rotates the object on the Z-axis (standard for 2D UI rotation)
        // Multiplied by Time.deltaTime to make it smooth regardless of frame rate
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
}