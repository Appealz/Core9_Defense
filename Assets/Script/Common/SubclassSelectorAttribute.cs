using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class SubclassSelectorAttribute : PropertyAttribute
{
    public string CacheFieldName { get; }

    public SubclassSelectorAttribute(string cacheFieldName = null)
    {
        CacheFieldName = cacheFieldName;
    }
}