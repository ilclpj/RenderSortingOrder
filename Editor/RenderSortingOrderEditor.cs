using LF.Components;
using UnityEditor;
using UnityEngine;

namespace LF.LFEditor
{
    [CustomEditor(typeof(RenderSortingOrder))]
    public class RenderSortingOrderEditor : Editor
    {
        private SerializedProperty m_BaseOrder;
        private SerializedProperty m_CurOrder;
        private SerializedProperty m_OrderDelta;
        private SerializedProperty m_OverrideSorting;

        private void OnEnable()
        {
            m_BaseOrder = serializedObject.FindProperty("baseOrder");
            m_CurOrder = serializedObject.FindProperty("curOrder");
            m_OrderDelta = serializedObject.FindProperty("orderDelta");
            m_OverrideSorting = serializedObject.FindProperty("overrideSorting");
        }

        private void _SetForbiddenEdit(SerializedProperty property)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(property);
            GUI.enabled = true;
        }

        private void _SetShow(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
            serializedObject.ApplyModifiedProperties();
        }

        private void _SetForbiddenEditOnPlay(SerializedProperty property, bool showOnNonPlay)
        {
            if (Application.isPlaying)
                _SetForbiddenEdit(property);
            else if (showOnNonPlay)
                _SetShow(property);
        }

        private bool _ModifyProperty(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
            var result = serializedObject.ApplyModifiedProperties();

            return Application.isPlaying && result;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _SetForbiddenEditOnPlay(m_CurOrder, false);
            _SetForbiddenEditOnPlay(m_OverrideSorting, true);

            if (Application.isPlaying && _ModifyProperty(m_BaseOrder))
            {
                var component = (RenderSortingOrder) target;
                component.SetBaseOrder(component.baseOrder);
                component.RefreshAllOrder();
                return;
            }

            if (_ModifyProperty(m_OrderDelta))
            {
                var component = (RenderSortingOrder) target;
                component.RefreshAllOrder();
            }
        }
    }
}