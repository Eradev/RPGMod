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

                    objective = Random.Range(Config.Questing.KillCommonMin, Config.Questing.KillCommonMax) * Mathf.Max(1, commonMultiplier);

                    break;

                case MissionType.KillAny:
                    objective = Random.Range(Config.Questing.KillAnyMin, Config.Questing.KillAnyMax) * Mathf.Max(1, Run.instance.stageClearCount / 2);

                    break;

                case MissionType.KillSpecificName:
                    objective = Random.Range(Config.Questing.KillSpecificNameMin, Config.Questing.KillSpecificNameMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

                    break;

                case MissionType.KillSpecificBuff:
                    objective = Random.Range(Config.Questing.KillSpecificBuffMin, Config.Questing.KillSpecificBuffMax) * Mathf.Max(1, Run.instance.stageClearCount / 3);

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

                case MissionType.CollectGold:
                    objective = Run.instance.GetDifficultyScaledCost(Random.Range(Config.Questing.CollectGoldMin, Config.Questing.CollectGoldMax));

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