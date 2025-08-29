using UnityEngine;

namespace Gameplay.Script
{
    public class FollowCameraXZ : MonoBehaviour
    {
        public Transform cameraTransform; // Assign your Camera's Transform here in the Inspector
        public float distanceFromCamera = 3f; // Distance from the camera to the canvas
        public float fixedHeight = 5f; // The fixed Y-axis height

        void Update()
        {
            if (cameraTransform == null)
            {
                Debug.LogWarning("Camera Transform not assigned!");
                return;
            }

            // Calculate a vector pointing forward, but flattened on the Y-axis
            Vector3 flatForward = cameraTransform.forward;
            flatForward.y = 0;
            flatForward.Normalize(); // Normalize it in case camera is perfectly looking up or down

            // Calculate the new position based on the flat forward vector
            Vector3 newPosition = cameraTransform.position + flatForward * distanceFromCamera;
            newPosition.y = fixedHeight; // Set to the fixed height

            transform.position = newPosition;

            // Now, make the canvas always look at the camera but maintain its upright orientation
            Vector3 lookDirection = cameraTransform.position - transform.position;
            lookDirection.y = 0; // To prevent vertical rotation
            Quaternion rotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
            transform.rotation = rotation;
        }
    }
}