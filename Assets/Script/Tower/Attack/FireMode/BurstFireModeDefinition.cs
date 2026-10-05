using System;
using UnityEngine;

[Serializable]
public sealed class BurstFireModeDefinition : FireModeDefinition
{
    [SerializeField] private int _shotCount = 3;
    [SerializeField] private float _shotInterval = 0.08f;

    public override IFireMode CreateFireMode()
    {
        return new BurstFireMode(_shotCount, _shotInterval);
    }
}