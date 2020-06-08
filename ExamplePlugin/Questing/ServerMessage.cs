using System.Collections.Generic;
using RoR2;

namespace RPGMod.Questing
{
    /// <summary>
    /// Server message for each quest
    /// </summary>
    public class ServerMessage
    {
        // Attributes
        public PickupDef Drop;
        public int Objective;
        public int Progress;
        public QuestType QuestType;
        public bool AwaitingClientMessage;
        public static List<ServerMessage> Instances { get; } = new List<ServerMessage>();

        public ServerMessage()
        {
            Progress = 0;
            Drop = Quest.GetQuestDrop();
            Objective = 100;
            AwaitingClientMessage = false;
        }

        public ServerMessage(QuestType questType) : this()
        {
            QuestType = questType;
        }

        public void RegisterInstance()
        {
            Instances.Add(this);
        }
    }
}