
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyConfig))]
public class EnemyConfigEditor : Editor
{
    private Type[] _movementDefinitionTypes;
    private string[] _movementDefinitionNames;
    private int _movementSelectedIndex;

    private Type[] _attackDefinitionTypes;
    private string[] _attackDefinitionNames;
    private int _attackSelectedIndex;

    private Type[] _deathDefinitionTypes;
    private string[] _deathDefinitionNames;
    private int _deathSelectedIndex;

    private void OnEnable()
    {
        CreateTypeList<EnemyMovementDefinition>(out _movementDefinitionTypes, out _movementDefinitionNames);
        CreateTypeList<EnemyAttackDefinition>(out _attackDefinitionTypes, out _attackDefinitionNames);
        CreateTypeList<EnemyDeathBehaviorDefinition>(out _deathDefinitionTypes, out _deathDefinitionNames);

        _movementSelectedIndex = FindSelectedIndex("_movementDefinition", _movementDefinitionTypes);
        _attackSelectedIndex = FindSelectedIndex("_attackDefinition", _attackDefinitionTypes);
        _deathSelectedIndex = FindSelectedIndex("_deathBehaviorDefinition", _deathDefinitionTypes);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_Script", "_movementDefinition", "_attackDefinition", "_deathBehaviorDefinition");

        SerializedProperty movementDefinition = serializedObject.FindProperty("_movementDefinition");
        SerializedProperty attackDefinition = serializedObject.FindProperty("_attackDefinition");
        SerializedProperty deathDefinition = serializedObject.FindProperty("_deathBehaviorDefinition");

        int newMovementIndex = EditorGUILayout.Popup("Movement Definition", _movementSelectedIndex, _movementDefinitionNames);

        if (newMovementIndex != _movementSelectedIndex)
        {
            _movementSelectedIndex = newMovementIndex;
            movementDefinition.managedReferenceValue = _movementSelectedIndex == 0
                ? null
                : Activator.CreateInstance(_movementDefinitionTypes[_movementSelectedIndex - 1]);
        }

        DrawDefinitionFields(movementDefinition);

        int newAttackIndex = EditorGUILayout.Popup("Attack Definition", _attackSelectedIndex, _attackDefinitionNames);

        if (newAttackIndex != _attackSelectedIndex)
        {
            _attackSelectedIndex = newAttackIndex;
            attackDefinition.managedReferenceValue = _attackSelectedIndex == 0
                ? null
                : Activator.CreateInstance(_attackDefinitionTypes[_attackSelectedIndex - 1]);
        }

        DrawDefinitionFields(attackDefinition);

        int newDeathIndex = EditorGUILayout.Popup("Death Behavior", _deathSelectedIndex, _deathDefinitionNames);

        if (newDeathIndex != _deathSelectedIndex)
        {
            _deathSelectedIndex = newDeathIndex;
            deathDefinition.managedReferenceValue = _deathSelectedIndex == 0
                ? null
                : Activator.CreateInstance(_deathDefinitionTypes[_deathSelectedIndex - 1]);
        }

        DrawDefinitionFields(deathDefinition);

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateTypeList<T>(out Type[] definitionTypes, out string[] definitionNames)
    {
        TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<T>();
        List<Type> validTypes = new();

        foreach (Type type in types)
        {
            if (!type.IsAbstract && !type.IsGenericType)
                validTypes.Add(type);
        }

        definitionTypes = validTypes.ToArray();
        definitionNames = new string[definitionTypes.Length + 1];
        definitionNames[0] = "None";

        for (int i = 0; i < definitionTypes.Length; i++)
        {
            string name = definitionTypes[i].Name.Replace("Definition", "");
            definitionNames[i + 1] = ObjectNames.NicifyVariableName(name);
        }
    }

    private int FindSelectedIndex(string propertyName, Type[] definitionTypes)
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property.managedReferenceValue == null)
            return 0;

        Type currentType = property.managedReferenceValue.GetType();

        for (int i = 0; i < definitionTypes.Length; i++)
        {
            if (definitionTypes[i] == currentType)
                return i + 1;
        }

        return 0;
    }

    private void DrawDefinitionFields(SerializedProperty property)
    {
        if (property.managedReferenceValue == null)
            return;

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();

        bool enterChildren = true;

        EditorGUI.indentLevel++;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            EditorGUILayout.PropertyField(iterator, true);
            enterChildren = false;
        }

        EditorGUI.indentLevel--;
    }
}
