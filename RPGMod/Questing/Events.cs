using RoR2;
using UnityEngine.Events;

namespace RPGMod.Questing
{
    public static class Events
    {
        public class QuestEvent : UnityEvent<int, string, NetworkUser> { }

        public static readonly QuestEvent AnyKilled = new QuestEvent();

        public static readonly QuestEvent CommonKilled = new QuestEvent();
        public static readonly QuestEvent EliteKilled = new QuestEvent();
        public static readonly QuestEvent ChampionKilled = new QuestEvent();

        public static readonly QuestEvent FlyingKilled = new QuestEvent();

        public static readonly QuestEvent GoldCollected = new QuestEvent();

        public static readonly QuestEvent SpecificBuffKilled = new QuestEvent();
        public static readonly QuestEvent SpecificNameKilled = new QuestEvent();
    }
}