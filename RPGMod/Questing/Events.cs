using RoR2;
using UnityEngine.Events;

namespace RPGMod.Questing
{
    public static class Events
    {
        public class QuestEvent : UnityEvent<int, string, NetworkUser> { }

        public static readonly QuestEvent AnyKilled = new();

        public static readonly QuestEvent CommonKilled = new();
        public static readonly QuestEvent EliteKilled = new();
        public static readonly QuestEvent ChampionKilled = new();

        public static readonly QuestEvent FlyingKilled = new();

        public static readonly QuestEvent GoldCollected = new();

        public static readonly QuestEvent SpecificBuffKilled = new();
        public static readonly QuestEvent SpecificNameKilled = new();
    }
}