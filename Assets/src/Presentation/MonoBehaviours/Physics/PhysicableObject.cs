using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using Vector3 = OrbitalAuthority.Domain.Core.Math.Vectors.Vector3;
using UVector3 = UnityEngine.Vector3;
using UnityEngine;

namespace OrbitalAuthority.Presentation.MonoBehaviours.Physics
{
    public class PhysicableObject : MonoBehaviour, IPhysicableObject
    {
        [SerializeField] private double _mass;
        [SerializeField] private bool _useCustomSpeed;
        [SerializeField] private double _velocity;
        [SerializeField] private double _polarAngle = 0;
        [SerializeField] private double _azimuthalAngle = 0;

        public double Mass => _mass;
        public bool UseCustomSpeed => _useCustomSpeed;
        public double InitialVelocity => _velocity;
        public double InitialPolarAngle => _polarAngle;
        public double InitialAzimuthalAngle => _azimuthalAngle;
        public Vector3 Position {get; set;}
        
        #if UNITY_EDITOR
        [SerializeField] private float _visualizationScale = 1000000f; 
        
        private void OnDrawGizmosSelected()
        {
            if (!_useCustomSpeed) return;
            
            UVector3 initialPoint = transform.position;
            
            float polarRad = (float)_polarAngle * Mathf.Deg2Rad;
            float azimuthalRad = (float)_azimuthalAngle * Mathf.Deg2Rad;
            float speed = (float)_velocity;
            
            UVector3 direction = new UVector3(
                Mathf.Cos(polarRad) * Mathf.Cos(azimuthalRad),
                Mathf.Sin(azimuthalRad),
                Mathf.Sin(polarRad) * Mathf.Cos(azimuthalRad)
            ).normalized;
            
            float visualLength = 5f;
            UVector3 targetPoint = initialPoint + direction * visualLength;
            
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawLine(initialPoint, targetPoint);
            
            UVector3 arrowBase = targetPoint - direction * 0.5f;
            
            UnityEditor.Handles.DrawLine(targetPoint, arrowBase + UVector3.up * 0.2f);
            UnityEditor.Handles.DrawLine(targetPoint, arrowBase - UVector3.up * 0.2f);
            UnityEditor.Handles.DrawLine(targetPoint, arrowBase + UVector3.right * 0.2f);
            UnityEditor.Handles.DrawLine(targetPoint, arrowBase - UVector3.right * 0.2f);
            
            UnityEditor.Handles.Label(
                targetPoint + UVector3.up * 0.5f,
                $"Speed: {speed:E2} m/s\nPolar: {_polarAngle:F1}°\nAzimuth: {_azimuthalAngle:F1}°"
            );
            
            float realDistance = speed;
            if (realDistance > 0.001f && realDistance < 100f)
            {
                UVector3 realEndPoint = initialPoint + direction * realDistance;
                if (UVector3.Distance(initialPoint, realEndPoint) < 100f)
                {
                    UnityEditor.Handles.color = new Color(1, 1, 0, 0.5f);
                    UnityEditor.Handles.DrawDottedLine(initialPoint, realEndPoint, 2f);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!_useCustomSpeed) return;
            
            UVector3 initialPoint = transform.position;
            
            float polarRad = (float)_polarAngle * Mathf.Deg2Rad;
            float azimuthalRad = (float)_azimuthalAngle * Mathf.Deg2Rad;
            
            UVector3 direction = new UVector3(
                Mathf.Sin(polarRad) * Mathf.Cos(azimuthalRad),
                Mathf.Sin(polarRad) * Mathf.Sin(azimuthalRad),
                Mathf.Cos(polarRad)
            ).normalized;
            
            float visualLength = 5f;
            UVector3 targetPoint = initialPoint + direction * visualLength;
            
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawLine(initialPoint, targetPoint);
        }
        #endif
    }
}