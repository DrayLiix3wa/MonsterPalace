using UnityEngine;

public class SuitcaseStack : MonoBehaviour
{
    [SerializeField] private float levitationHeight = 1.0f;
    [SerializeField] private float oscillationAmplitude = 0.1f;
    [SerializeField] private float oscillationSpeed = 1.0f;
    [SerializeField] private float rotationSpeed = 5.0f;

    private float initialY;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // Initialiser la position
        initialPosition = transform.position;
        initialY = initialPosition.y;

        // D�finir la hauteur de l�vitation
        transform.position = new Vector3(initialPosition.x, levitationHeight, initialPosition.z);

        // Rotation al�atoire sur l'axe Y pour chaque enfant
        foreach (Transform child in transform)
        {
            float randomYRotation = Random.Range(0f, 360f);
            child.rotation = Quaternion.Euler(0f, randomYRotation, 0f);
        }

        // Enregistrer la rotation initiale
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Oscillation al�atoire et l�g�re sur Y
        float newY = levitationHeight + Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmplitude;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        // L�g�re rotation lente sur Y
        transform.rotation = initialRotation * Quaternion.Euler(0f, Time.time * rotationSpeed, 0f);
    }
}