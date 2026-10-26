#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class TowerCombatDebugPanel : MonoBehaviour
{
    private const int WindowId = 0xC09D;
    private const int FirstSlotNumber = 1;
    private const int LastSlotNumber = 9;

    private readonly TowerStatType[] _statTypes =
        (TowerStatType[])Enum.GetValues(typeof(TowerStatType));

    private CoreManager _coreManager;
    private TowerStatModifiers _globalStatModifiers;
    private RewardSelectionManager _rewardSelectionManager;
    private IReadOnlyList<RewardData> _rewardDataList;

    private Rect _windowRect = new Rect(12f, 12f, 440f, 690f);
    private Vector2 _scrollPosition;
    private int _selectedSlotNumber = 5;
    private int _selectedRewardIndex;
    private bool _isVisible;
    private bool _isApplyingReward;

    public void Initialize(
        CoreManager coreManager,
        TowerStatModifiers globalStatModifiers,
        RewardSelectionManager rewardSelectionManager,
        IReadOnlyList<RewardData> rewardDataList)
    {
        _coreManager = coreManager;
        _globalStatModifiers = globalStatModifiers;
        _rewardSelectionManager = rewardSelectionManager;
        _rewardDataList = rewardDataList;

        Debug.Log("[Combat Debug] Press F8 to open the tower/reward verification panel.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
            _isVisible = !_isVisible;
    }

    private void OnGUI()
    {
        if (!_isVisible || _coreManager == null)
            return;

        _windowRect.width = Mathf.Min(_windowRect.width, Screen.width - 24f);
        _windowRect.height = Mathf.Min(690f, Screen.height - 24f);
        _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, "Core9 Combat Debug (F8)");
    }

    private void DrawWindow(int windowId)
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        DrawSlotSelector();
        DrawSelectedTower();
        DrawGlobalModifiers();
        DrawCombatControls();
        DrawRewardControls();
        DrawTowerComparison();

        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0f, 0f, _windowRect.width, 24f));
    }

    private void DrawSlotSelector()
    {
        GUILayout.Label("Tower selection (CoreSlot)");
        GUILayout.BeginHorizontal();

        for (int slotNumber = FirstSlotNumber; slotNumber <= LastSlotNumber; slotNumber++)
        {
            string label = slotNumber == _selectedSlotNumber ? $"[{slotNumber}]" : slotNumber.ToString();

            if (GUILayout.Button(label, GUILayout.MinWidth(34f)))
                _selectedSlotNumber = slotNumber;
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
    }

    private void DrawSelectedTower()
    {
        GUILayout.Label("Selected tower runtime stats");

        if (!TryGetSelectedTower(out Tower tower))
        {
            GUILayout.Label($"CoreSlot {_selectedSlotNumber}: empty");
            GUILayout.Space(6f);
            return;
        }

        TowerStats stats = tower.Stats;
        TowerHealth health = tower.Health;

        GUILayout.Label($"Tower: {tower.name}");
        GUILayout.Label($"CoreSlot: {_selectedSlotNumber}");
        GUILayout.Label($"AttackDamage: {stats.BaseAttackDamage:0.###} -> {stats.AttackDamage:0.###}");
        GUILayout.Label($"MaxHp: {stats.BaseMaxHp:0.###} -> {stats.MaxHp:0.###}");
        GUILayout.Label($"CurrentHp: {health.CurrentHp:0.###} / {health.MaxHp:0.###}");
        GUILayout.Label($"AttackSpeed: {stats.BaseAttackSpeed:0.###}/s -> {stats.AttackSpeed:0.###}/s");
        GUILayout.Label($"Actual attack interval: {stats.AttackInterval:0.###} sec");

        string criticalOverride = stats.HasDebugCriticalChanceOverride ? " (debug override)" : string.Empty;
        GUILayout.Label($"CriticalChance: {stats.EffectiveCriticalChance:P1}{criticalOverride}");
        GUILayout.Label($"CriticalDamageMultiplier: x{stats.CriticalDamageMultiplier:0.###}");
        GUILayout.Space(6f);
    }

    private void DrawGlobalModifiers()
    {
        GUILayout.Label("Global modifiers (live accumulated values)");

        foreach (TowerStatType statType in _statTypes)
        {
            float flat = _globalStatModifiers.GetFlat(statType);
            float percent = _globalStatModifiers.GetPercent(statType);
            GUILayout.Label($"{statType}: Flat +{flat:0.###}, Percent +{percent:P1}");
        }

        GUILayout.Space(6f);
    }

    private void DrawCombatControls()
    {
        bool traceEnabled = GUILayout.Toggle(
            TowerCombatDebugTrace.Enabled,
            "Log actual attacks and enemy HP changes"
        );

        TowerCombatDebugTrace.Enabled = traceEnabled;

        GUILayout.BeginHorizontal();
        GUI.enabled = TryGetSelectedTower(out Tower tower);

        if (GUILayout.Button("Force Crit 0%"))
            tower.Stats.SetDebugCriticalChanceOverride(0f);

        if (GUILayout.Button("Force Crit 100%"))
            tower.Stats.SetDebugCriticalChanceOverride(1f);

        if (GUILayout.Button("Clear Crit Override"))
            tower.Stats.SetDebugCriticalChanceOverride(null);

        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
    }

    private void DrawRewardControls()
    {
        GUILayout.Label("Apply RewardData through real reward logic");

        if (_rewardDataList == null || _rewardDataList.Count == 0)
        {
            GUILayout.Label("No RewardData configured in GameStartData.");
            GUILayout.Space(6f);
            return;
        }

        _selectedRewardIndex = Mathf.Clamp(_selectedRewardIndex, 0, _rewardDataList.Count - 1);
        RewardData rewardData = _rewardDataList[_selectedRewardIndex];
        string rewardName = rewardData == null ? "<null>" : rewardData.RewardName;

        GUILayout.Label($"{_selectedRewardIndex + 1}/{_rewardDataList.Count}: {rewardName}");

        if (rewardData != null)
            GUILayout.Label($"Category: {rewardData.Category} | {rewardData.Description}");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Previous"))
            _selectedRewardIndex = WrapIndex(_selectedRewardIndex - 1, _rewardDataList.Count);

        GUI.enabled = !_isApplyingReward && rewardData != null;

        if (GUILayout.Button(_isApplyingReward ? "Applying..." : "Apply selected reward"))
            ApplySelectedRewardAsync(rewardData).Forget();

        GUI.enabled = true;

        if (GUILayout.Button("Next"))
            _selectedRewardIndex = WrapIndex(_selectedRewardIndex + 1, _rewardDataList.Count);

        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
    }

    private void DrawTowerComparison()
    {
        GUILayout.Label("Current/new tower comparison");

        for (int slotNumber = FirstSlotNumber; slotNumber <= LastSlotNumber; slotNumber++)
        {
            if (!_coreManager.TryGetTower(slotNumber, out Tower tower))
                continue;

            TowerStats stats = tower.Stats;
            GUILayout.Label(
                $"Slot {slotNumber} {tower.name}: DMG {stats.AttackDamage:0.###}, " +
                $"HP {tower.Health.CurrentHp:0.###}/{tower.Health.MaxHp:0.###}, " +
                $"SPD {stats.AttackSpeed:0.###}/s, CRIT {stats.EffectiveCriticalChance:P1}"
            );
        }

        if (GUILayout.Button("Write tower comparison to Console"))
            LogTowerComparison();
    }

    private async UniTaskVoid ApplySelectedRewardAsync(RewardData rewardData)
    {
        _isApplyingReward = true;

        try
        {
            await _rewardSelectionManager.ApplyForDebugAsync(rewardData);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            _isApplyingReward = false;
        }
    }

    private void LogTowerComparison()
    {
        for (int slotNumber = FirstSlotNumber; slotNumber <= LastSlotNumber; slotNumber++)
        {
            if (!_coreManager.TryGetTower(slotNumber, out Tower tower))
                continue;

            TowerStats stats = tower.Stats;
            Debug.Log(
                $"[Combat Debug] Slot={slotNumber}, Tower={tower.name}, " +
                $"Damage={stats.BaseAttackDamage:0.###}->{stats.AttackDamage:0.###}, " +
                $"MaxHp={stats.BaseMaxHp:0.###}->{stats.MaxHp:0.###}, " +
                $"CurrentHp={tower.Health.CurrentHp:0.###}, " +
                $"Speed={stats.BaseAttackSpeed:0.###}->{stats.AttackSpeed:0.###}, " +
                $"Interval={stats.AttackInterval:0.###}, " +
                $"Crit={stats.EffectiveCriticalChance:P1}, CritMultiplier={stats.CriticalDamageMultiplier:0.###}"
            );
        }
    }

    private bool TryGetSelectedTower(out Tower tower)
    {
        return _coreManager.TryGetTower(_selectedSlotNumber, out tower);
    }

    private static int WrapIndex(int index, int count)
    {
        return (index % count + count) % count;
    }
}
#endif
