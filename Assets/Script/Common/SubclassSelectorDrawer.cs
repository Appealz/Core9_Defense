using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    private const float ResetButtonWidth = 52f;
    private static readonly Dictionary<Type, Type[]> _typeCache = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.LabelField(position, label.text, "SubclassSelector는 SerializeReference에만 사용할 수 있습니다.");
            return;
        }

        Type[] types = GetTypes(fieldInfo.FieldType);
        string[] names = GetNames(types);
        int selectedIndex = GetSelectedIndex(property, types);

        Rect lineRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginProperty(lineRect, label, property);

        Rect contentRect = EditorGUI.PrefixLabel(lineRect, label);
        Rect popupRect = new(contentRect.x, contentRect.y, contentRect.width - ResetButtonWidth - 4f, contentRect.height);
        Rect resetRect = new(popupRect.xMax + 4f, contentRect.y, ResetButtonWidth, contentRect.height);

        int newSelectedIndex = EditorGUI.Popup(popupRect, selectedIndex, names);

        if (newSelectedIndex != selectedIndex)
        {
            property.managedReferenceValue = newSelectedIndex == 0 ? null : GetOrCreateReference(property, types[newSelectedIndex - 1]);
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.BeginDisabledGroup(selectedIndex == 0);

        if (GUI.Button(resetRect, "초기화"))
        {
            Type selectedType = types[selectedIndex - 1];
            object newReference = Activator.CreateInstance(selectedType);

            ReplaceCachedReference(property, selectedType, newReference);

            property.managedReferenceValue = newReference;
            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.EndDisabledGroup();
        EditorGUI.EndProperty();

        if (property.managedReferenceValue != null)
            DrawChildren(position, property);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.propertyType != SerializedPropertyType.ManagedReference || property.managedReferenceValue == null)
            return height;

        SerializedProperty child = property.Copy();
        SerializedProperty end = child.GetEndProperty();
        bool enterChildren = true;

        while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
        {
            enterChildren = false;
            height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(child, true);
        }

        return height;
    }

    private Type[] GetTypes(Type baseType)
    {
        if (_typeCache.TryGetValue(baseType, out Type[] cachedTypes))
            return cachedTypes;

        TypeCache.TypeCollection derivedTypes = TypeCache.GetTypesDerivedFrom(baseType);
        List<Type> types = new();

        foreach (Type type in derivedTypes)
        {
            if (!type.IsAbstract && !type.IsGenericType)
                types.Add(type);
        }

        types.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        Type[] result = types.ToArray();
        _typeCache.Add(baseType, result);

        return result;
    }

    private string[] GetNames(Type[] types)
    {
        string[] names = new string[types.Length + 1];
        names[0] = "미설정";

        for (int i = 0; i < types.Length; i++)
            names[i + 1] = types[i].Name;

        return names;
    }

    private int GetSelectedIndex(SerializedProperty property, Type[] types)
    {
        object currentReference = property.managedReferenceValue;

        if (currentReference == null)
            return 0;

        Type currentType = currentReference.GetType();

        for (int i = 0; i < types.Length; i++)
        {
            if (types[i] == currentType)
                return i + 1;
        }

        return 0;
    }

    private object GetOrCreateReference(SerializedProperty property, Type selectedType)
    {
        SerializedProperty cache = GetCacheProperty(property);

        if (cache == null || !cache.isArray)
            return Activator.CreateInstance(selectedType);

        for (int i = 0; i < cache.arraySize; i++)
        {
            object reference = cache.GetArrayElementAtIndex(i).managedReferenceValue;

            if (reference != null && reference.GetType() == selectedType)
                return reference;
        }

        object newReference = Activator.CreateInstance(selectedType);

        int newIndex = cache.arraySize;
        cache.arraySize++;
        cache.GetArrayElementAtIndex(newIndex).managedReferenceValue = newReference;

        return newReference;
    }

    private void ReplaceCachedReference(SerializedProperty property, Type selectedType, object newReference)
    {
        SerializedProperty cache = GetCacheProperty(property);

        if (cache == null || !cache.isArray)
            return;

        for (int i = 0; i < cache.arraySize; i++)
        {
            SerializedProperty element = cache.GetArrayElementAtIndex(i);
            object reference = element.managedReferenceValue;

            if (reference != null && reference.GetType() == selectedType)
            {
                element.managedReferenceValue = newReference;
                return;
            }
        }

        int newIndex = cache.arraySize;
        cache.arraySize++;
        cache.GetArrayElementAtIndex(newIndex).managedReferenceValue = newReference;
    }

    private SerializedProperty GetCacheProperty(SerializedProperty property)
    {
        SubclassSelectorAttribute selector = (SubclassSelectorAttribute)attribute;

        if (string.IsNullOrEmpty(selector.CacheFieldName))
            return null;

        int lastDotIndex = property.propertyPath.LastIndexOf('.');
        string cachePath = lastDotIndex < 0
            ? selector.CacheFieldName
            : property.propertyPath.Substring(0, lastDotIndex + 1) + selector.CacheFieldName;

        return property.serializedObject.FindProperty(cachePath);
    }

    private void DrawChildren(Rect position, SerializedProperty property)
    {
        SerializedProperty child = property.Copy();
        SerializedProperty end = child.GetEndProperty();
        bool enterChildren = true;
        float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        EditorGUI.indentLevel++;

        while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
        {
            enterChildren = false;

            float height = EditorGUI.GetPropertyHeight(child, true);
            Rect childRect = new(position.x, y, position.width, height);

            EditorGUI.PropertyField(childRect, child, true);
            y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        EditorGUI.indentLevel--;
    }
}