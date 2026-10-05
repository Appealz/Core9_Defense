using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TowerPartData))]
public class TowerPartDataEditor : Editor
{
    private Type[] _attackDefinitionTypes;
    private string[] _attackDefinitionNames;
    private int _selectedAttackIndex;

    private Type[] _fireModeDefinitionTypes;
    private string[] _fireModeDefinitionNames;
    private int _selectedFireModeIndex;

    private void OnEnable()
    {
        InitializeAttackDefinitions();
        InitializeFireModeDefinitions();

        SyncSelectedAttackIndex();
        SyncSelectedFireModeIndex();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawAttackDefinition();
        DrawFireModeDefinition();

        DrawDefaultInspector();
    }

    private void InitializeAttackDefinitions()
    {
        TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<TowerAttackDefinition>();

        _attackDefinitionTypes = new Type[types.Count];
        _attackDefinitionNames = new string[types.Count + 1];

        _attackDefinitionNames[0] = "미설정";

        for (int i = 0; i < types.Count; i++)
        {
            _attackDefinitionTypes[i] = types[i];
            _attackDefinitionNames[i + 1] = types[i].Name;
        }
    }

    private void InitializeFireModeDefinitions()
    {
        TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<FireModeDefinition>();

        _fireModeDefinitionTypes = new Type[types.Count];
        _fireModeDefinitionNames = new string[types.Count + 1];

        _fireModeDefinitionNames[0] = "미설정";

        for (int i = 0; i < types.Count; i++)
        {
            _fireModeDefinitionTypes[i] = types[i];
            _fireModeDefinitionNames[i + 1] = types[i].Name;
        }
    }

    private void DrawAttackDefinition()
    {
        int newSelectedIndex = EditorGUILayout.Popup("Attack Definition", _selectedAttackIndex, _attackDefinitionNames);

        if (newSelectedIndex != _selectedAttackIndex)
        {
            _selectedAttackIndex = newSelectedIndex;

            SerializedProperty attackDefinition = serializedObject.FindProperty("_attackDefinition");

            if (_selectedAttackIndex == 0)
            {
                attackDefinition.managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Type selectedType = _attackDefinitionTypes[_selectedAttackIndex - 1];
                SerializedProperty attackDefinitions = serializedObject.FindProperty("_attackDefinitions");

                TowerAttackDefinition definition = FindAttackDefinition(attackDefinitions, selectedType);

                if (definition == null)
                {
                    definition = (TowerAttackDefinition)Activator.CreateInstance(selectedType);

                    int newIndex = attackDefinitions.arraySize;
                    attackDefinitions.arraySize++;

                    SerializedProperty newElement = attackDefinitions.GetArrayElementAtIndex(newIndex);
                    newElement.managedReferenceValue = definition;
                }

                attackDefinition.managedReferenceValue = definition;
                serializedObject.ApplyModifiedProperties();
            }
        }

        if (_selectedAttackIndex != 0 && GUILayout.Button("현재 공격 설정 초기화"))
        {
            Type selectedType = _attackDefinitionTypes[_selectedAttackIndex - 1];

            SerializedProperty attackDefinition = serializedObject.FindProperty("_attackDefinition");
            SerializedProperty attackDefinitions = serializedObject.FindProperty("_attackDefinitions");

            int definitionIndex = FindAttackDefinitionIndex(attackDefinitions, selectedType);

            if (definitionIndex >= 0)
            {
                TowerAttackDefinition newDefinition = (TowerAttackDefinition)Activator.CreateInstance(selectedType);

                SerializedProperty element = attackDefinitions.GetArrayElementAtIndex(definitionIndex);
                element.managedReferenceValue = newDefinition;

                attackDefinition.managedReferenceValue = newDefinition;

                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private void DrawFireModeDefinition()
    {
        int newSelectedIndex = EditorGUILayout.Popup("Fire Mode", _selectedFireModeIndex, _fireModeDefinitionNames);

        if (newSelectedIndex != _selectedFireModeIndex)
        {
            _selectedFireModeIndex = newSelectedIndex;

            SerializedProperty fireModeDefinition = serializedObject.FindProperty("_fireModeDefinition");

            if (_selectedFireModeIndex == 0)
            {
                fireModeDefinition.managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Type selectedType = _fireModeDefinitionTypes[_selectedFireModeIndex - 1];
                SerializedProperty fireModeDefinitions = serializedObject.FindProperty("_fireModeDefinitions");

                FireModeDefinition definition = FindFireModeDefinition(fireModeDefinitions, selectedType);

                if (definition == null)
                {
                    definition = (FireModeDefinition)Activator.CreateInstance(selectedType);

                    int newIndex = fireModeDefinitions.arraySize;
                    fireModeDefinitions.arraySize++;

                    SerializedProperty newElement = fireModeDefinitions.GetArrayElementAtIndex(newIndex);
                    newElement.managedReferenceValue = definition;
                }

                fireModeDefinition.managedReferenceValue = definition;
                serializedObject.ApplyModifiedProperties();
            }
        }

        if (_selectedFireModeIndex != 0 && GUILayout.Button("현재 발사 방식 설정 초기화"))
        {
            Type selectedType = _fireModeDefinitionTypes[_selectedFireModeIndex - 1];

            SerializedProperty fireModeDefinition = serializedObject.FindProperty("_fireModeDefinition");
            SerializedProperty fireModeDefinitions = serializedObject.FindProperty("_fireModeDefinitions");

            int definitionIndex = FindFireModeDefinitionIndex(fireModeDefinitions, selectedType);

            if (definitionIndex >= 0)
            {
                FireModeDefinition newDefinition = (FireModeDefinition)Activator.CreateInstance(selectedType);

                SerializedProperty element = fireModeDefinitions.GetArrayElementAtIndex(definitionIndex);
                element.managedReferenceValue = newDefinition;

                fireModeDefinition.managedReferenceValue = newDefinition;

                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private void SyncSelectedAttackIndex()
    {
        serializedObject.Update();

        SerializedProperty attackDefinition = serializedObject.FindProperty("_attackDefinition");
        TowerAttackDefinition currentDefinition = (TowerAttackDefinition)attackDefinition.managedReferenceValue;

        _selectedAttackIndex = 0;

        if (currentDefinition == null)
            return;

        Type currentType = currentDefinition.GetType();

        for (int i = 0; i < _attackDefinitionTypes.Length; i++)
        {
            if (_attackDefinitionTypes[i] == currentType)
            {
                _selectedAttackIndex = i + 1;
                return;
            }
        }
    }

    private void SyncSelectedFireModeIndex()
    {
        serializedObject.Update();

        SerializedProperty fireModeDefinition = serializedObject.FindProperty("_fireModeDefinition");
        FireModeDefinition currentDefinition = (FireModeDefinition)fireModeDefinition.managedReferenceValue;

        _selectedFireModeIndex = 0;

        if (currentDefinition == null)
            return;

        Type currentType = currentDefinition.GetType();

        for (int i = 0; i < _fireModeDefinitionTypes.Length; i++)
        {
            if (_fireModeDefinitionTypes[i] == currentType)
            {
                _selectedFireModeIndex = i + 1;
                return;
            }
        }
    }

    private TowerAttackDefinition FindAttackDefinition(SerializedProperty definitions, Type selectedType)
    {
        for (int i = 0; i < definitions.arraySize; i++)
        {
            SerializedProperty element = definitions.GetArrayElementAtIndex(i);
            TowerAttackDefinition definition = (TowerAttackDefinition)element.managedReferenceValue;

            if (definition != null && definition.GetType() == selectedType)
                return definition;
        }

        return null;
    }

    private int FindAttackDefinitionIndex(SerializedProperty definitions, Type selectedType)
    {
        for (int i = 0; i < definitions.arraySize; i++)
        {
            SerializedProperty element = definitions.GetArrayElementAtIndex(i);
            TowerAttackDefinition definition = (TowerAttackDefinition)element.managedReferenceValue;

            if (definition != null && definition.GetType() == selectedType)
                return i;
        }

        return -1;
    }

    private FireModeDefinition FindFireModeDefinition(SerializedProperty definitions, Type selectedType)
    {
        for (int i = 0; i < definitions.arraySize; i++)
        {
            SerializedProperty element = definitions.GetArrayElementAtIndex(i);
            FireModeDefinition definition = (FireModeDefinition)element.managedReferenceValue;

            if (definition != null && definition.GetType() == selectedType)
                return definition;
        }

        return null;
    }

    private int FindFireModeDefinitionIndex(SerializedProperty definitions, Type selectedType)
    {
        for (int i = 0; i < definitions.arraySize; i++)
        {
            SerializedProperty element = definitions.GetArrayElementAtIndex(i);
            FireModeDefinition definition = (FireModeDefinition)element.managedReferenceValue;

            if (definition != null && definition.GetType() == selectedType)
                return i;
        }

        return -1;
    }
}