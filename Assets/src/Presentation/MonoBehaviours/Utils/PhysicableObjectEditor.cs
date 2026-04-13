using UnityEditor;
using UnityEngine;

namespace OrbitalAuthority.Presentation.MonoBehaviours.Physics.Editor
{
    [CustomEditor(typeof(PhysicableObject))]
    public class PhysicableObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty _mass;
        private SerializedProperty _useCustomSpeed;
        private SerializedProperty _velocity;
        private SerializedProperty _polarAngle;
        private SerializedProperty _azimuthalAngle;

        private void OnEnable()
        {
            _mass = serializedObject.FindProperty("_mass");
            _useCustomSpeed = serializedObject.FindProperty("_useCustomSpeed");
            _velocity = serializedObject.FindProperty("_velocity");
            _polarAngle = serializedObject.FindProperty("_polarAngle");
            _azimuthalAngle = serializedObject.FindProperty("_azimuthalAngle");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_mass);
            EditorGUILayout.PropertyField(_useCustomSpeed);
            
            if (_useCustomSpeed.boolValue)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Initial Velocity", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_velocity, new GUIContent("Velocity"));
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(_polarAngle, new GUIContent("Polar Angle (degrees)"));
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(_azimuthalAngle, new GUIContent("Azimuthal Angle (degrees)"));
                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}