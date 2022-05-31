using RoR2;
using UnityEngine;

namespace RPGMod.Questing
{
    public class QuestComponent
    {
        private readonly NetworkUser networkUser;
        private int progress;

        public int Objective { get; }
        public bool Complete { get; private set; }
        public QuestType QuestType { get; }

        public int Progress
        {
            get => progress;
            set
            {
                if (Complete)
                {
                    return;
                }

                progress = value;

                if (progress < Objective)
                {
                    return;
                }

                Complete = true;
                RemoveListener();
                Manager.CheckClientData(networkUser);
            }
        }

        private QuestComponent()
        {
            Complete = false;
            progress = 0;
        }

        public QuestComponent(QuestType questType, NetworkUser networkUser) : this()
        {
            QuestType = questType;
            this.networkUser = networkUser;
            Objective = GenerateObjective(questType);

            switch (questType)
            {
                case QuestType.killCommon:
                    Events.commonKilled.AddListener(Listener);

                    break;

                case QuestType.killElite:
                    Events.eliteKilled.AddListener(Listener);

                    break;

                case QuestType.killChampion:
                    Events.championKilled.AddListener(Listener);

                    break;

                case QuestType.killFlying:
                    Events.flyingKilled.AddListener(Listener);

                    break;

                case QuestType.killHaunted:
                    Events.hauntedKilled.AddListener(Listener);

                    break;

                case QuestType.killPoison:
                    Events.poisonKilled.AddListener(Listener);

                    break;

                case QuestType.collectGold:
                    Events.goldCollected.AddListener(Listener);

                    break;

                default:
                    Debug.LogError(questType);

                    break;
            }
        }

        public QuestComponent(QuestType questType, bool complete, int progress, int objective) : this()
        {
            QuestType = questType;
            Complete = complete;
            this.progress = progress;
            Objective = objective;
        }

        private int GenerateObjective(QuestType questType)
        {
            int objective;

            // TODO: Scale values for difficulty
            switch (questType)
            {
                case QuestType.killCommon:
                    // Common enemies start to be less frequent after a few stages so we limit them
                    var commonMultiplier = Mathf.Min(Run.instance.stageClearCount, 5);

                    objective = Random.Range(Config.Questing.killCommonMin, Config.Questing.killCommonMax) * Mathf.Max(1, commonMultiplier);

                    break;

                case QuestType.killElite:
                    objective = Random.Range(Config.Questing.killEliteMin, Config.Questing.killEliteMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case QuestType.killChampion:
                    objective = Random.Range(Config.Questing.killChampionMin, Config.Questing.killChampionMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killFlying:
                    objective = Random.Range(Config.Questing.killFlyingMin, Config.Questing.killFlyingMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case QuestType.killHaunted:
                    objective = Random.Range(Config.Questing.killHauntedMin, Config.Questing.killHauntedMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killPoison:
                    objective = Random.Range(Config.Questing.killPoisonMin, Config.Questing.killPoisonMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.collectGold:
                    objective = Run.instance.GetDifficultyScaledCost(Random.Range(Config.Questing.collectGoldMin, Config.Questing.collectGoldMax));

                    break;

                default:
                    objective = 1;
                    Debug.LogError(questType);
                    break;
            }
            return objective;
        }

        private void Listener(int value, NetworkUser networkUser)
        {
            if (this.networkUser == networkUser)
            {
                Progress += value;
            }
        }

        private void RemoveListener()
        {
            switch (QuestType)
            {
                case QuestType.killCommon:
                    Events.commonKilled.RemoveListener(Listener);

                    break;

                case QuestType.killElite:
                    Events.eliteKilled.RemoveListener(Listener);

                    break;

                case QuestType.killChampion:
                    Events.championKilled.RemoveListener(Listener);

                    break;

                case QuestType.killFlying:
                    Events.flyingKilled.RemoveListener(Listener);

                    break;

                case QuestType.killHaunted:
                    Events.hauntedKilled.RemoveListener(Listener);

                    break;

                case QuestType.killPoison:
                    Events.poisonKilled.RemoveListener(Listener);

                    break;

                case QuestType.collectGold:
                    Events.goldCollected.RemoveListener(Listener);

                    break;
            }
        }

        public static bool GetComplete(QuestComponent questComponent)
        {
            return questComponent.Complete;
        }
    }
}