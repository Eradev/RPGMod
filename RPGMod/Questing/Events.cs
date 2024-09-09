using RoR2;
using UnityEngine.Events;

namespace RPGMod.Questing
{
    public static class Events
    {
        public class QuestEvent : UnityEvent<int, NetworkUser> { }

        public static readonly QuestEvent AnyKilled = new QuestEvent();
        public static readonly QuestEvent AnyBuffKilled = new QuestEvent();

        public static readonly QuestEvent CommonKilled = new QuestEvent();
        public static readonly QuestEvent EliteKilled = new QuestEvent();
        public static readonly QuestEvent ChampionKilled = new QuestEvent();

        public static readonly QuestEvent FlyingKilled = new QuestEvent();

        public static readonly QuestEvent RedKilled = new QuestEvent();
        public static readonly QuestEvent HauntedKilled = new QuestEvent();
        public static readonly QuestEvent WhiteKilled = new QuestEvent();
        public static readonly QuestEvent PoisonKilled = new QuestEvent();
        public static readonly QuestEvent BlueKilled = new QuestEvent();
        public static readonly QuestEvent LunarKilled = new QuestEvent();

        public static readonly QuestEvent EarthKilledDLC1 = new QuestEvent();
        public static readonly QuestEvent VoidKilledDLC1 = new QuestEvent();

        public static readonly QuestEvent AurelioniteKilledDLC2 = new QuestEvent();
        public static readonly QuestEvent BeadKilledDLC2 = new QuestEvent();

        public static readonly QuestEvent GoldCollected = new QuestEvent();
    }
}