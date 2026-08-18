using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TowerPartData))]
public class TowerPartDataEditor : Editor
{
    private Type[] _attackDefinitionTypes;
    private string[] _attackDefinitionNames;
    private int _selectedIndex;

    // AttackDefinition 타입 목록과 Popup 이름 목록을 생성하고 현재 선택값을 동기화.
    private void OnEnable()
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

        SyncSelectedIndex();
    }

    // TowerPartData의 Inspector UI 구성 및 입력 처리.
    public override void OnInspectorGUI()
    {
        // Popup을 표시하고 현재 선택된 인덱스를 반환받음.
        int newSelectedIndex = EditorGUILayout.Popup("Attack Definition", _selectedIndex, _attackDefinitionNames);

        // 기존 인덱스와 선택된 인덱스가 다른경우(Popup의 선택값이 변경된 경우)
        if (newSelectedIndex != _selectedIndex)
        {
            _selectedIndex = newSelectedIndex;

            SerializedProperty attackDefinition = serializedObject.FindProperty("_attackDefinition");

            if (_selectedIndex == 0)
            {
                attackDefinition.managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Type selectedType = _attackDefinitionTypes[_selectedIndex - 1];
                SerializedProperty attackDefinitions = serializedObject.FindProperty("_attackDefinitions");

                TowerAttackDefinition definition = FindDefinition(attackDefinitions, selectedType);

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

        // 현재 공격 설정 초기화 버튼 클릭(현재 AttackDefinition을 새 객체로 교체하여 설정 초기화)
        if (_selectedIndex != 0 && GUILayout.Button("현재 공격 설정 초기화"))
        {
            Type selectedType = _attackDefinitionTypes[_selectedIndex - 1];

            SerializedProperty attackDefinition = serializedObject.FindProperty("_attackDefinition");
            SerializedProperty attackDefinitions = serializedObject.FindProperty("_attackDefinitions");

            int definitionIndex = FindDefinitionIndex(attackDefinitions, selectedType);

            if (definitionIndex >= 0)
            {
                TowerAttackDefinition newDefinition = (TowerAttackDefinition)Activator.CreateInstance(selectedType);

                SerializedProperty element = attackDefinitions.GetArrayElementAtIndex(definitionIndex);
                element.managedReferenceValue = newDefinition;

                attackDefinition.managedReferenceValue = newDefinition;

                serializedObject.ApplyModifiedProperties();
            }
        }

        DrawDefaultInspector();
    }

    // 현재 _attackDefinition의 실제 타입과 Popup 선택 인덱스를 동기화.
    private void SyncSelectedIndex()
    {
        serializedObject.Update();

        SerializedProperty attackDefinition = serializedObject.FindProperty("_attackDefinition");
        TowerAttackDefinition currentDefinition = (TowerAttackDefinition)attackDefinition.managedReferenceValue;

        _selectedIndex = 0;

        if (currentDefinition == null)
            return;

        Type currentType = currentDefinition.GetType();

        for (int i = 0; i < _attackDefinitionTypes.Length; i++)
        {
            if (_attackDefinitionTypes[i] == currentType)
            {
                _selectedIndex = i + 1;
                return;
            }
        }
    }

    // 보관된 Definition 중 selectedType과 같은 타입의 객체를 찾아 반환.
    private TowerAttackDefinition FindDefinition(SerializedProperty attackDefinitions, Type selectedType)
    {
        for (int i = 0; i < attackDefinitions.arraySize; i++)
        {
            SerializedProperty element = attackDefinitions.GetArrayElementAtIndex(i);
            TowerAttackDefinition definition = (TowerAttackDefinition)element.managedReferenceValue;

            if (definition != null && definition.GetType() == selectedType)
                return definition;
        }

        return null;
    }

    // 보관된 Definition 중 selectedType과 같은 타입의 객체 인덱스를 반환.
    private int FindDefinitionIndex(SerializedProperty attackDefinitions, Type selectedType)
    {
        for (int i = 0; i < attackDefinitions.arraySize; i++)
        {
            SerializedProperty element = attackDefinitions.GetArrayElementAtIndex(i);
            TowerAttackDefinition definition = (TowerAttackDefinition)element.managedReferenceValue;

            if (definition != null && definition.GetType() == selectedType)
                return i;
        }

        return -1;
    }
}