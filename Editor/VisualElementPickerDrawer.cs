using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Empress.UI {

    [CustomPropertyDrawer(typeof(VisualElementPickerAttribute))]
    public class VisualElementPickerDrawer : PropertyDrawer {

        const float ICON_WIDTH = 24;
        const float ICON_HEIGHT = 16;
        const float ICON_SPACING = 2;
        bool m_TargetPanelFound;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var target = property.serializedObject.targetObject as MonoBehaviour;
            var errorBoxHeight = EditorGUIUtility.singleLineHeight * 2;
            var fieldRect = new Rect(position.x, position.y, position.width - ICON_WIDTH - ICON_SPACING, EditorGUIUtility.singleLineHeight);
            var buttonRect = new Rect(position.x + position.width - ICON_WIDTH, position.y + (EditorGUIUtility.singleLineHeight - ICON_HEIGHT) / 2, ICON_WIDTH, ICON_HEIGHT);
            var backgroundColor = GUI.backgroundColor;
            var showErrorBox = false;
            var disablePicker = false;

            target.TryGetComponent<UIDocument>(out var uiDocument);

            // Check for errors
            if (property.propertyType != SerializedPropertyType.String) {
                var error = "This property drawer can only be used with string properties. Please ensure the property is of type string.";
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, errorBoxHeight), error, MessageType.Error);
                showErrorBox = true;
                disablePicker = true;
            }
            else if (!uiDocument) {
                var error = "No UI Document found in this component. Ensure that a UI Document component is attached and properly configured.";
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, errorBoxHeight), error, MessageType.Error);
                showErrorBox = true;
                disablePicker = true;
            }
            else if (!m_TargetPanelFound) {
                var error = $"VisualElement '{property.stringValue}' not found. Check name and existence.";
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, errorBoxHeight), error, MessageType.Error);
                showErrorBox = true;
                GUI.backgroundColor = new Color32(200, 75, 75, 255);
            }

            // Adjust fieldRect if error box is shown
            if (showErrorBox) {
                fieldRect.y += errorBoxHeight + EditorGUIUtility.standardVerticalSpacing;
                buttonRect.y += errorBoxHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // Draw the property field
            EditorGUI.PropertyField(fieldRect, property, label);

            // Restore color
            GUI.backgroundColor = backgroundColor;

            // Draw the button next to the property field
            GUI.enabled = !disablePicker;
            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("d_Search Icon"))) {
                VisualElementPickerWindow.Show(
                    fieldRect,
                    (VisualElementPickerAttribute)attribute,
                    uiDocument.rootVisualElement,
                    selectedName => {
                        // Update property with selected element name
                        property.stringValue = selectedName;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                );
            }
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var target = property.serializedObject.targetObject as MonoBehaviour;
            var baseHeight = EditorGUIUtility.singleLineHeight;
            var errorBoxHeight = EditorGUIUtility.singleLineHeight * 2;

            target.TryGetComponent<UIDocument>(out var uiDocument);
            m_TargetPanelFound = uiDocument && uiDocument.rootVisualElement.Q(property.stringValue) != null;

            // Calculate height based on the presence of errors
            if (property.propertyType != SerializedPropertyType.String || !uiDocument || !m_TargetPanelFound)
                return baseHeight + errorBoxHeight + EditorGUIUtility.standardVerticalSpacing;

            return baseHeight;
        }
    }
}
