using UnityEngine;
using System;

public sealed class TowerAttackDependencies
{
    public ProjectileManager ProjectileManager { get; }
    public IImpactEffect ImpactEffect { get; }
    public LaserManager LaserManager { get; }

    public TowerAttackDependencies(ProjectileManager projectileManager, IImpactEffect impactEffect, LaserManager laserManager)
    {
        ProjectileManager = projectileManager ?? throw new ArgumentNullException(nameof(projectileManager));
        ImpactEffect = impactEffect ?? throw new ArgumentNullException(nameof(impactEffect));
        LaserManager = laserManager ?? throw new ArgumentNullException(nameof(laserManager));
    }
}