using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using Vector2 = OrbitalAuthority.Domain.Core.Math.Vectors.Vector2;
using UnityEngine;

namespace OrbitalAuthority.Presentation.MonoBehaviours.Physics
{
    public class PhysicableObject : MonoBehaviour, IPhysicableObject
    {
        [SerializeField] private double _mass;
        [SerializeField] private bool _useCustomSpeed;
        [SerializeField] private double _velocity;
        [SerializeField] private double _polarAngle;
        [SerializeField] private double _azimuthalAngle;

        public double Mass => _mass;
        public bool UseCustomSpeed => _useCustomSpeed;
        public double InitialVelocity => _velocity;
        public double InitialPolarAngle => _polarAngle;
        public double InitialAzimuthalAngle => _azimuthalAngle;
        
        #if UNITY_EDITOR
        [SerializeField] private float _visualizationScale = 1000000f; 
        
        private void OnDrawGizmosSelected()
        {
            if (!_useCustomSpeed) return;
            
            Vector3 initialPoint = transform.position;
            
            float polarRad = (float)_polarAngle * Mathf.Deg2Rad;
            float azimuthalRad = (float)_azimuthalAngle * Mathf.Deg2Rad;
            float speed = (float)_velocity;
            
            Vector3 direction = new Vector3(
                Mathf.Sin(polarRad) * Mathf.Cos(azimuthalRad),
                Mathf.Sin(polarRad) * Mathf.Sin(azimuthalRad),
                Mathf.Cos(polarRad)
            ).normalized;
            
            float visualLength = 5f;
            Vector3 targetPoint = initialPoint + direction * visualLength;
            
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawLine(initialPoint, targetPoint);
            
            Vector3 arrowBase = targetPoint - direction * 0.5f;
            
            UnityEditor.Handles.DrawLine(targetPoint, arrowBase + Vector3.up * 0.2f);
            UnityEditor.Handles.DrawLine(targetPoint, arrowBase - Vector3.up * 0.2f);
            UnityEditor.Handles.DrawLine(targetPoint, arrowBase + Vector3.right * 0.2f);
            UnityEditor.Handles.DrawLine(targetPoint, arrowBase - Vector3.right * 0.2f);
            
            UnityEditor.Handles.Label(
                targetPoint + Vector3.up * 0.5f,
                $"Speed: {speed:E2} m/s\nPolar: {_polarAngle:F1}°\nAzimuth: {_azimuthalAngle:F1}°"
            );
            
            float realDistance = speed;
            if (realDistance > 0.001f && realDistance < 100f)
            {
                Vector3 realEndPoint = initialPoint + direction * realDistance;
                if (Vector3.Distance(initialPoint, realEndPoint) < 100f)
                {
                    UnityEditor.Handles.color = new Color(1, 1, 0, 0.5f);
                    UnityEditor.Handles.DrawDottedLine(initialPoint, realEndPoint, 2f);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!_useCustomSpeed) return;
            
            Vector3 initialPoint = transform.position;
            
            float polarRad = (float)_polarAngle * Mathf.Deg2Rad;
            float azimuthalRad = (float)_azimuthalAngle * Mathf.Deg2Rad;
            
            Vector3 direction = new Vector3(
                Mathf.Sin(polarRad) * Mathf.Cos(azimuthalRad),
                Mathf.Sin(polarRad) * Mathf.Sin(azimuthalRad),
                Mathf.Cos(polarRad)
            ).normalized;
            
            float visualLength = 5f;
            Vector3 targetPoint = initialPoint + direction * visualLength;
            
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawLine(initialPoint, targetPoint);
        }
        #endif
    }
}