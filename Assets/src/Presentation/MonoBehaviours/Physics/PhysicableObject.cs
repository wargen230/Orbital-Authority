using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using Vector2 = OrbitalAuthority.Domain.Core.Math.Vectors.Vector2;
using UnityEngine;

namespace OrbitalAuthority.Presentation.MonoBehaviours.Physics
{
    public class PhysicableObject : MonoBehaviour, IPhysicableObject
    {
        [SerializeField] private double _mass;
        [SerializeField] private bool _useCustomSpeed;
        [SerializeField] double _velocityX;
        [SerializeField] double _velocityY;

        public double Mass => _mass;
        public bool UseCustomSpeed => _useCustomSpeed;
        public Vector2 InitialVelocity => new Vector2(_velocityX, _velocityY);
    }
}
