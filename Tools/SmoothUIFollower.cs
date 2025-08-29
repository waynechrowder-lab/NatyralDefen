using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Gameplay.Script
{
    public class SmoothUIFollower : MonoBehaviour
    {
        [Header("Follow Settings")] 
        public bool alwaysSetPosition = true;
        public float followSpeed = 5f;
        public float rotationSpeed = 5f;
        public float distanceFromTarget = 1.5f;
        public float heightOffset = 0f;
        public float positionDeadZone = 0.05f; 

        [Header("Facing Settings")]
        public bool alwaysFacePlayer = true;
        public float minAngleToRotate = 30f;
        public float angleFollowSpeed = 5f;

        private Transform targetTransform;
        private Canvas canvas;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private Transform temp;
        private Vector3 lastStablePosition;

        private void Start()
        {
            targetTransform = Camera.main.transform;
            temp = new GameObject().transform;
        }

        private void Update()
        {
            CalculateTargetPositionAndRotation();
            SmoothFollow();
        }

        private void CalculateTargetPositionAndRotation()
        {
            if (targetTransform == null) return;
            var rot = Quaternion.Euler(0, targetTransform.rotation.eulerAngles.y, 
                targetTransform.rotation.eulerAngles.z);
            temp.SetPositionAndRotation(targetTransform.position, rot);
            
            if (alwaysSetPosition)
            {
                targetPosition = targetTransform.position + temp.forward * distanceFromTarget;
                targetPosition.y = targetTransform.position.y + heightOffset;
        
                bool shouldUpdatePosition = Vector3.Distance(targetPosition, lastStablePosition) > positionDeadZone;
                if (!shouldUpdatePosition)
                    targetPosition = lastStablePosition;
                else
                    lastStablePosition = targetPosition;
            }

            if (alwaysFacePlayer)
            {
                Vector3 directionToTarget = targetTransform.position - transform.position;
                directionToTarget.y = 0;
            
                if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(-directionToTarget)) > minAngleToRotate)
                {
                    targetRotation = Quaternion.Euler(0, Quaternion.LookRotation(-directionToTarget).eulerAngles.y, 0);
                    // targetRotation = Quaternion.LookRotation(-directionToTarget);
                }
            }
            else
            {
                targetRotation = Quaternion.Euler(0, targetTransform.rotation.eulerAngles.y, 0);
                // targetRotation = targetTransform.rotation;
            }
        }

        private void SmoothFollow()
        {
            if (targetTransform == null) return;
            if (alwaysSetPosition)
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (targetTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(targetTransform.position, targetPosition);
                Gizmos.DrawSphere(targetPosition, 0.1f);
            }
        }
    }
}