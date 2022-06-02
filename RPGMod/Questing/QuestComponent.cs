using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace RPGMod.Questing
{
    public class QuestComponent
    {
        private readonly NetworkUser networkUser;
        private int progress;

        public static readonly Dictionary<QuestType, Events.QuestEvent> QuestEventByQuestType = new Dictionary<QuestType, Events.QuestEvent>
        {
            { QuestType.killCommon, Events.CommonKilled },
            { QuestType.killElite, Events.EliteKilled },
            { QuestType.killChampion, Events.ChampionKilled },
            { QuestType.killRed, Events.RedKilled },
            { QuestType.killHaunted, Events.HauntedKilled },
            { QuestType.killWhite, Events.WhiteKilled },
            { QuestType.killPoison, Events.PoisonKilled },
            { QuestType.killBlue, Events.BlueKilled },
            { QuestType.killLunar, Events.LunarKilled },
            { QuestType.killEarthDLC1, Events.EarthKilledDLC1 },
            { QuestType.killVoidDLC1, Events.VoidKilledDLC1 },
            { QuestType.killFlying, Events.FlyingKilled },
            { QuestType.killByBackstab, Events.KilledByBackstab },
            { QuestType.collectGold, Events.GoldCollected }
        };

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

            QuestEventByQuestType[questType].AddListener(Listener);
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

                case QuestType.killRed:
                    objective = Random.Range(Config.Questing.killRedMin, Config.Questing.killRedMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killHaunted:
                    objective = Random.Range(Config.Questing.killHauntedMin, Config.Questing.killHauntedMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killWhite:
                    objective = Random.Range(Config.Questing.killWhiteMin, Config.Questing.killWhiteMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killPoison:
                    objective = Random.Range(Config.Questing.killPoisonMin, Config.Questing.killPoisonMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killBlue:
                    objective = Random.Range(Config.Questing.killBlueMin, Config.Questing.killBlueMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killLunar:
                    objective = Random.Range(Config.Questing.killLunarMin, Config.Questing.killLunarMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killEarthDLC1:
                    objective = Random.Range(Config.Questing.killEarthMin, Config.Questing.killEarthMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killVoidDLC1:
                    objective = Random.Range(Config.Questing.killVoidMin, Config.Questing.killVoidMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case QuestType.killByBackstab:
                    objective = Random.Range(Config.Questing.killByBackstabMin, Config.Questing.killByBackstabMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

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
            QuestEventByQuestType[QuestType].RemoveListener(Listener);
        }

        public static bool GetComplete(QuestComponent questComponent)
        {
            return questComponent.Complete;
        }
    }
}