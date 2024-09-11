using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMod.Questing
{
    public class Mission
    {
        private readonly NetworkUser _networkUser;
        private int _progress;

        public static readonly Dictionary<MissionType, Events.QuestEvent> EventsByMissionType = new Dictionary<MissionType, Events.QuestEvent>
        {
            { MissionType.KillAny, Events.AnyKilled },
            { MissionType.KillAnyBuff, Events.AnyBuffKilled },
            { MissionType.KillCommon, Events.CommonKilled },
            { MissionType.KillElite, Events.EliteKilled },
            { MissionType.KillChampion, Events.ChampionKilled },
            { MissionType.KillFlying, Events.FlyingKilled },
            { MissionType.KillRed, Events.RedKilled },
            { MissionType.KillHaunted, Events.HauntedKilled },
            { MissionType.KillWhite, Events.WhiteKilled },
            { MissionType.KillPoison, Events.PoisonKilled },
            { MissionType.KillBlue, Events.BlueKilled },
            { MissionType.KillLunar, Events.LunarKilled },
            { MissionType.KillEarthDLC1, Events.EarthKilledDLC1 },
            { MissionType.KillVoidDLC1, Events.VoidKilledDLC1 },
            { MissionType.KillAurelioniteDLC2, Events.AurelioniteKilledDLC2 },
            { MissionType.KillBeadDLC2, Events.BeadKilledDLC2 },
            { MissionType.CollectGold, Events.GoldCollected }
        };

        public int Objective { get; }
        public bool IsCompleted { get; private set; }
        public MissionType MissionType { get; }

        public int Progress
        {
            get => _progress;
            private set
            {
                if (IsCompleted)
                {
                    return;
                }

                _progress = value;

                if (_progress < Objective)
                {
                    return;
                }

                Abort();
                Manager.CheckClientData(_networkUser);
            }
        }

        private Mission()
        {
            IsCompleted = false;
            _progress = 0;
        }

        public Mission(MissionType missionType, NetworkUser networkUser) : this()
        {
            MissionType = missionType;
            _networkUser = networkUser;
            Objective = GenerateObjective(missionType);

            EventsByMissionType[missionType].AddListener(Listener);
        }

        public Mission(MissionType missionType, bool isCompleted, int progress, int objective) : this()
        {
            MissionType = missionType;
            IsCompleted = isCompleted;
            _progress = progress;
            Objective = objective;
        }

        private int GenerateObjective(MissionType missionType)
        {
            int objective;

            // TODO: Scale values for difficulty
            switch (missionType)
            {
                case MissionType.KillCommon:
                    // Common enemies start to be less frequent after a few stages so we limit them
                    var commonMultiplier = Mathf.Min(Run.instance.stageClearCount, 5);

                    objective = Random.Range(Config.Questing.KillCommonMin, Config.Questing.KillCommonMax) * Mathf.Max(1, commonMultiplier);

                    break;

                case MissionType.KillAny:
                    objective = Random.Range(Config.Questing.KillAnyMin, Config.Questing.KillAnyMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.KillAnyBuff:
                    objective = Random.Range(Config.Questing.KillAnyBuffMin, Config.Questing.KillAnyBuffMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillElite:
                    objective = Random.Range(Config.Questing.KillEliteMin, Config.Questing.KillEliteMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.KillChampion:
                    objective = Random.Range(Config.Questing.KillChampionMin, Config.Questing.KillChampionMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillFlying:
                    objective = Random.Range(Config.Questing.KillFlyingMin, Config.Questing.KillFlyingMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.KillRed:
                    objective = Random.Range(Config.Questing.KillRedMin, Config.Questing.KillRedMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillHaunted:
                    objective = Random.Range(Config.Questing.KillHauntedMin, Config.Questing.KillHauntedMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillWhite:
                    objective = Random.Range(Config.Questing.KillWhiteMin, Config.Questing.KillWhiteMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillPoison:
                    objective = Random.Range(Config.Questing.KillPoisonMin, Config.Questing.KillPoisonMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillBlue:
                    objective = Random.Range(Config.Questing.KillBlueMin, Config.Questing.KillBlueMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillLunar:
                    objective = Random.Range(Config.Questing.KillLunarMin, Config.Questing.KillLunarMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillEarthDLC1:
                    objective = Random.Range(Config.Questing.KillEarthMin, Config.Questing.KillEarthMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillVoidDLC1:
                    objective = Random.Range(Config.Questing.KillVoidMin, Config.Questing.KillVoidMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillAurelioniteDLC2:
                    objective = Random.Range(Config.Questing.KillAurelioniteMin, Config.Questing.KillAurelioniteMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillBeadDLC2:
                    objective = Random.Range(Config.Questing.KillBeadMin, Config.Questing.KillBeadMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.CollectGold:
                    objective = Run.instance.GetDifficultyScaledCost(Random.Range(Config.Questing.CollectGoldMin, Config.Questing.CollectGoldMax));

                    break;

                default:
                    objective = 1;
                    Debug.LogError(missionType);

                    break;
            }

            return objective;
        }

        public void Abort()
        {
            IsCompleted = true;
            RemoveListener();
        }

        private void Listener(int value, NetworkUser networkUser)
        {
            if (_networkUser == networkUser)
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