using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;

namespace RPGMod.Questing
{
    // Quest Message that gets sent to all clients
    public class ClientMessage : MessageBase
    {
        // Attributes
        public int Id;
        public bool Active;
        public bool AdvancingStage;
        public string Description;
        public string Target;
        public string IconPath;
        public static List<ClientMessage> Instances { get; set; } = new List<ClientMessage>();

        //Constructors
        public ClientMessage()
        {
            Description = "bad";
            IconPath = "custom";
            Active = false;
            AdvancingStage = false;
        }

        public ClientMessage(string target) : this()
        {
            Target = target;
        }

        public override void Deserialize(NetworkReader reader)
        {
            Id = reader.ReadInt32();
            Active = reader.ReadBoolean();
            AdvancingStage = reader.ReadBoolean();
            Description = reader.ReadString();
            Target = reader.ReadString();
            IconPath = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Id);
            writer.Write(Active);
            writer.Write(AdvancingStage);
            writer.Write(Description);
            writer.Write(Target);
            writer.Write(IconPath);
        }

        public void RegisterInstance()
        {
            Instances.Add(this);
        }

        // Sends the quest to all clients via the quest port.
        public void SendToAll()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            NetworkServer.SendToAll(Config.QuestPort, this);
        }
    }

    // Server message for each quest
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