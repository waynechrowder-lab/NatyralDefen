using UnityEngine;

namespace Gameplay.Script.MiniWorldGameplay
{

    public interface IPhysicsController
    {
        void ApplyStabilization(Rigidbody rb);
        void ApplyAlignmentTorque(Rigidbody rb);
    }
    
    public class PhysicsController : MonoBehaviour, IPhysicsController
    {
        public float alignmentForce = 200f;
        
        public void ApplyStabilization(Rigidbody rb)
        {
            // 减少水平滑动
            Vector3 horizontalVelocity = rb.velocity;
            horizontalVelocity.y = 0;
            rb.AddForce(-horizontalVelocity * 5f, ForceMode.Acceleration);
        
            // 当接近直立时增加稳定性
            // if (Vector3.Angle(transform.up, Vector3.up) < 30f)
            // {
            //     rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
            // }
        }
        
        public void ApplyAlignmentTorque(Rigidbody rb)
        {
            Vector3 currentUp = transform.up;
            Vector3 targetUp = Vector3.up;
        
            Vector3 rotationAxis = Vector3.Cross(currentUp, targetUp);
            float rotationAngle = Vector3.Angle(currentUp, targetUp);
            
            // Vector3 currentForward = transform.forward;
            // currentForward.y = 0;
            //
            // if (currentForward.magnitude > 0.01f)
            // {
            //     currentForward.Normalize();
            //     Vector3 targetForward = Vector3.forward; // 或根据需求设置其他方向
            //     Vector3 forwardRotationAxis = Vector3.Cross(currentForward, targetForward);
            //     float forwardRotationAngle = Vector3.Angle(currentForward, targetForward);
            //
            //     // 合并两个旋转轴
            //     rotationAxis += forwardRotationAxis * 0.5f; // 权重可调
            //     rotationAngle += forwardRotationAngle * 0.5f;
            // }
            
            if (rotationAngle > 1f)
            {
                float dampingFactor = 1.3f; // 阻尼系数(0-1)
                Vector3 angularVelocity = rb.angularVelocity;
                Vector3 desiredTorque = rotationAxis * (rotationAngle * alignmentForce * Time.fixedDeltaTime);
                Vector3 dampingTorque = -angularVelocity * dampingFactor;
        
                // 应用组合扭矩
                rb.AddTorque(desiredTorque + dampingTorque, ForceMode.VelocityChange);
        
                // 限制最大角速度
                if (rb.angularVelocity.magnitude > 5f)
                {
                    rb.angularVelocity = rb.angularVelocity.normalized * 5f;
                }
                // // 动态阻尼 - 角度越大阻尼越小
                // float dampingFactor = Mathf.Lerp(2f, 0.5f, rotationAngle / 90f);
                // Vector3 angularVelocity = rb.angularVelocity;
                // // 更平滑的扭矩计算
                // float torqueFactor = Mathf.Clamp(rotationAngle / 90f, 0.1f, 1f);
                // Vector3 desiredTorque = rotationAxis * (torqueFactor * alignmentForce * Time.fixedDeltaTime);
                // Vector3 dampingTorque = -angularVelocity * dampingFactor;
                //
                // rb.AddTorque(desiredTorque + dampingTorque, ForceMode.VelocityChange);
                //
                // // 更合理的角速度限制
                // float maxAngularSpeed = Mathf.Lerp(5f, 10f, rotationAngle / 90f);
                // if (rb.angularVelocity.magnitude > maxAngularSpeed)
                // {
                //     rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
                // }
            }
        }
    }
}