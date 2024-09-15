using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMod.Questing
{
    public class Mission
    {
        private readonly NetworkUser _networkUser;
        private int _progress;

        public static readonly Dictionary<MissionType, Events.QuestEvent> EventsByMissionType = new()
        {
            { MissionType.KillAny, Events.AnyKilled },
            { MissionType.KillCommon, Events.CommonKilled },
            { MissionType.KillElite, Events.EliteKilled },
            { MissionType.KillChampion, Events.ChampionKilled },
            { MissionType.KillFlying, Events.FlyingKilled },
            { MissionType.CollectGold, Events.GoldCollected },
            { MissionType.KillSpecificName, Events.SpecificNameKilled },
            { MissionType.KillSpecificBuff, Events.SpecificBuffKilled }
        };

        public int Objective { get; }
        public bool IsCompleted { get; private set; }
        public MissionType MissionType { get; }
        public string MissionSpecification { get; }

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

        public Mission(MissionType missionType, string missionSpecification, NetworkUser networkUser) : this()
        {
            MissionType = missionType;
            MissionSpecification = missionSpecification;
            _networkUser = networkUser;
            Objective = GenerateObjective(missionType);

            EventsByMissionType[missionType].AddListener(Listener);
        }

        public Mission(MissionType missionType, string missionSpecification, bool isCompleted, int progress, int objective) : this()
        {
            MissionType = missionType;
            MissionSpecification = missionSpecification;
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

                    objective = Random.Range(ConfigValues.Questing.MissionKillCommonMin, ConfigValues.Questing.MissionKillCommonMax) * Mathf.Max(1, commonMultiplier);

                    break;

                case MissionType.KillAny:
                    objective = Random.Range(ConfigValues.Questing.MissionKillAnyMin, ConfigValues.Questing.MissionKillAnyMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.KillSpecificName:
                    objective = Random.Range(ConfigValues.Questing.KillSpecificNameMin, ConfigValues.Questing.KillSpecificNameMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillSpecificBuff:
                    objective = Random.Range(ConfigValues.Questing.MissionKillSpecificBuffMin, ConfigValues.Questing.MissionKillSpecificBuffMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillElite:
                    objective = Random.Range(ConfigValues.Questing.MissionKillEliteMin, ConfigValues.Questing.MissionKillEliteMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.KillChampion:
                    objective = Random.Range(ConfigValues.Questing.MissionKillChampionMin, ConfigValues.Questing.MissionKillChampionMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillFlying:
                    objective = Random.Range(ConfigValues.Questing.MissionKillFlyingMin, ConfigValues.Questing.MissionKillFlyingMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.CollectGold:
                    objective = Run.instance.GetDifficultyScaledCost(Random.Range(ConfigValues.Questing.MissionCollectGoldMin, ConfigValues.Questing.MissionCollectGoldMax));

                    break;

                default:
                    objective = 1;
                    Debug.LogError($"Unknown mission type: {missionType}");

                    break;
            }

            return objective;
        }

        public void Abort()
        {
            IsCompleted = true;
            RemoveListener();
        }

        private void Listener(int value, string specification, NetworkUser networkUser)
        {
            if (_networkUser == networkUser && MissionSpecification == specification)
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