using System.Collections.Generic;
using UnityEngine.Networking;

namespace RPGMod.Questing
{
    /// <summary>
    /// Quest Message that gets sent to all clients
    /// </summary>
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
}