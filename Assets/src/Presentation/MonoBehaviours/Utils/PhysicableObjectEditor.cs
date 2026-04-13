// В папке Editor создайте файл PhysicableObjectEditor.cs
using UnityEditor;
using UnityEngine;

namespace OrbitalAuthority.Presentation.MonoBehaviours.Physics.Editor
{
    [CustomEditor(typeof(PhysicableObject))]
    public class PhysicableObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty _mass;
        private SerializedProperty _useCustomSpeed;
        private SerializedProperty _velocityX;
        private SerializedProperty _velocityY;
        
        private void OnEnable()
        {
            _mass = serializedObject.FindProperty("_mass");
            _useCustomSpeed = serializedObject.FindProperty("_useCustomSpeed");
            _velocityX = serializedObject.FindProperty("_velocityX");
            _velocityY = serializedObject.FindProperty("_velocityY");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_mass);
            EditorGUILayout.PropertyField(_useCustomSpeed);
            
            // Показываем поля скорости только если UseCustomSpeed включен
            if (_useCustomSpeed.boolValue)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Initial Velocity", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_velocityX, new GUIContent("Velocity X"));
                EditorGUILayout.PropertyField(_velocityY, new GUIContent("Velocity Y"));
                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}