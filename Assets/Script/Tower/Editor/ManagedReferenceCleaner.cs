using UnityEditor;
using UnityEngine;

public static class ManagedReferenceCleaner
{
    [MenuItem("Tools/Core9/Check Missing Managed References")]
    private static void CheckMissingReferences()
    {
        Object target = Selection.activeObject;

        if (target == null)
        {
            Debug.LogWarning("확인할 Asset을 선택하세요.");
            return;
        }

        if (!SerializationUtility.HasManagedReferencesWithMissingTypes(target))
        {
            Debug.Log($"Missing Managed Reference 없음: {target.name}");
            return;
        }

        ManagedReferenceMissingType[] missingTypes =
            SerializationUtility.GetManagedReferencesWithMissingTypes(target);

        for (int i = 0; i < missingTypes.Length; i++)
        {
            ManagedReferenceMissingType missing = missingTypes[i];

            Debug.Log(
                $"Missing Managed Reference 발견\n" +
                $"Asset: {target.name}\n" +
                $"Class: {missing.className}\n" +
                $"Namespace: {missing.namespaceName}\n" +
                $"Assembly: {missing.assemblyName}\n" +
                $"Reference ID: {missing.referenceId}");
        }
    }

    [MenuItem("Tools/Core9/Clear Missing Managed References")]
    private static void ClearMissingReferences()
    {
        Object target = Selection.activeObject;

        if (target == null)
        {
            Debug.LogWarning("정리할 Asset을 선택하세요.");
            return;
        }

        if (!SerializationUtility.HasManagedReferencesWithMissingTypes(target))
        {
            Debug.Log($"정리할 Missing Managed Reference 없음: {target.name}");
            return;
        }

        Undo.RecordObject(target, "Clear Missing Managed References");

        SerializationUtility.ClearAllManagedReferencesWithMissingTypes(target);

        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();

        Debug.Log($"Missing Managed Reference 정리 완료: {target.name}");
    }
}