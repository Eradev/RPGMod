using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace RPGMod.Questing
{
    public class Mission
    {
        private readonly NetworkUser networkUser;
        private int progress;

        public static readonly Dictionary<MissionType, Events.QuestEvent> EventsByMissionType = new Dictionary<MissionType, Events.QuestEvent>
        {
            { MissionType.killCommon, Events.CommonKilled },
            { MissionType.killElite, Events.EliteKilled },
            { MissionType.killChampion, Events.ChampionKilled },
            { MissionType.killRed, Events.RedKilled },
            { MissionType.killHaunted, Events.HauntedKilled },
            { MissionType.killWhite, Events.WhiteKilled },
            { MissionType.killPoison, Events.PoisonKilled },
            { MissionType.killBlue, Events.BlueKilled },
            { MissionType.killLunar, Events.LunarKilled },
            { MissionType.killEarthDLC1, Events.EarthKilledDLC1 },
            { MissionType.killVoidDLC1, Events.VoidKilledDLC1 },
            { MissionType.killFlying, Events.FlyingKilled },
            { MissionType.killByBackstab, Events.KilledByBackstab },
            { MissionType.collectGold, Events.GoldCollected }
        };

        public int Objective { get; }
        public bool IsCompleted { get; private set; }
        public MissionType MissionType { get; }

        public int Progress
        {
            get => progress;
            set
            {
                if (IsCompleted)
                {
                    return;
                }

                progress = value;

                if (progress < Objective)
                {
                    return;
                }

                IsCompleted = true;
                RemoveListener();
                Manager.CheckClientData(networkUser);
            }
        }

        private Mission()
        {
            IsCompleted = false;
            progress = 0;
        }

        public Mission(MissionType missionType, NetworkUser networkUser) : this()
        {
            MissionType = missionType;
            this.networkUser = networkUser;
            Objective = GenerateObjective(missionType);

            EventsByMissionType[missionType].AddListener(Listener);
        }

        public Mission(MissionType missionType, bool isCompleted, int progress, int objective) : this()
        {
            MissionType = missionType;
            IsCompleted = isCompleted;
            this.progress = progress;
            Objective = objective;
        }

        private int GenerateObjective(MissionType missionType)
        {
            int objective;

            // TODO: Scale values for difficulty
            switch (missionType)
            {
                case MissionType.killCommon:
                    // Common enemies start to be less frequent after a few stages so we limit them
                    var commonMultiplier = Mathf.Min(Run.instance.stageClearCount, 5);

                    objective = Random.Range(Config.Questing.killCommonMin, Config.Questing.killCommonMax) * Mathf.Max(1, commonMultiplier);

                    break;

                case MissionType.killElite:
                    objective = Random.Range(Config.Questing.killEliteMin, Config.Questing.killEliteMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.killChampion:
                    objective = Random.Range(Config.Questing.killChampionMin, Config.Questing.killChampionMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killFlying:
                    objective = Random.Range(Config.Questing.killFlyingMin, Config.Questing.killFlyingMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.killRed:
                    objective = Random.Range(Config.Questing.killRedMin, Config.Questing.killRedMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killHaunted:
                    objective = Random.Range(Config.Questing.killHauntedMin, Config.Questing.killHauntedMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killWhite:
                    objective = Random.Range(Config.Questing.killWhiteMin, Config.Questing.killWhiteMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killPoison:
                    objective = Random.Range(Config.Questing.killPoisonMin, Config.Questing.killPoisonMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killBlue:
                    objective = Random.Range(Config.Questing.killBlueMin, Config.Questing.killBlueMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killLunar:
                    objective = Random.Range(Config.Questing.killLunarMin, Config.Questing.killLunarMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killEarthDLC1:
                    objective = Random.Range(Config.Questing.killEarthMin, Config.Questing.killEarthMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killVoidDLC1:
                    objective = Random.Range(Config.Questing.killVoidMin, Config.Questing.killVoidMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.killByBackstab:
                    objective = Random.Range(Config.Questing.killByBackstabMin, Config.Questing.killByBackstabMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.collectGold:
                    objective = Run.instance.GetDifficultyScaledCost(Random.Range(Config.Questing.collectGoldMin, Config.Questing.collectGoldMax));

                    break;

                default:
                    objective = 1;
                    Debug.LogError(missionType);

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
            EventsByMissionType[MissionType].RemoveListener(Listener);
        }
    }
}