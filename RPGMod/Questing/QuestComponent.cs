using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Events;

namespace RPGMod
{
namespace Questing
{
public class QuestComponent {
    private int progress;
    public int objective { get; private set; }
    public bool complete { get; private set; }
    private NetworkUser networkUser;
    public QuestType questType { get; private set; }
    public int Progress { get { return progress; } set { if (!complete) { progress = value; if (progress >= objective) { complete = true; RemoveListener(); Manager.CheckClientData(networkUser); } } } }

    private QuestComponent() {
        complete = false;
        progress = 0;
    }
    public QuestComponent(QuestType questType, NetworkUser networkUser) : this() {
        this.questType = questType;
        this.networkUser = networkUser;
        objective = GenerateObjective(questType);
        switch (questType) {
            case QuestType.killCommon:
                Events.commonKilled.AddListener(Listener);
                break;
            case QuestType.killElite:
                Events.eliteKilled.AddListener(Listener);
                break;
            case QuestType.collectGold:
                Events.goldCollected.AddListener(Listener);
                break;
            default:
                Debug.LogError(questType);
                break;
        }
    }
    public QuestComponent(QuestType questType, bool complete, int progress, int objective) : this() {
        this.questType = questType;
        this.complete = complete;
        this.progress = progress;
        this.objective = objective;
    }
    private int GenerateObjective(QuestType questType) {
        int objective;
        // TODO: Scale values for difficulty
        switch (questType) {
            case QuestType.killCommon:
                objective = UnityEngine.Random.Range(Config.Questing.killCommonMin, Config.Questing.killCommonMax);
                break;
            case QuestType.killElite:
                objective = UnityEngine.Random.Range(Config.Questing.killEliteMin, Config.Questing.killEliteMax);
                break;
            case QuestType.collectGold:
                objective = Run.instance.GetDifficultyScaledCost(UnityEngine.Random.Range(Config.Questing.collectGoldMin, Config.Questing.collectGoldMax));
                break;
            case QuestType.attemptShrineChance:
                objective = UnityEngine.Random.Range(Config.Questing.attemptShrineChanceMin, Config.Questing.attemptShrineChanceMax);;
                break;
            default:
                objective = 1;
                Debug.LogError(questType);
                break;
        }
        return objective;
    }
    void Listener(int value, NetworkUser networkUser) {
        if (this.networkUser == networkUser) {
            Progress += value;
        }
    }
    private void RemoveListener() {
        switch (questType) {
            case QuestType.killCommon:
                Events.commonKilled.RemoveListener(Listener);
                break;
            case QuestType.killElite:
                Events.eliteKilled.RemoveListener(Listener);
                break;
            case QuestType.collectGold:
                Events.goldCollected.RemoveListener(Listener);
                break;
            case QuestType.attemptShrineChance:
                Events.chanceShrineAttempted.RemoveListener(Listener);
                break;
        }
    }
    public static bool getComplete(QuestComponent questComponent)
    {
        return questComponent.complete;
    }
}

} // namespace Questing
} // namespace RPGMod