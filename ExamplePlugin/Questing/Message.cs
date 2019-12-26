using UnityEngine.Networking;
using RoR2;
using System.Collections.Generic;

namespace RPGMod
{
    namespace Questing
    {
        // Quest Message that gets sent to all clients
        public class ClientMessage : MessageBase
        {
            // Attributes
            public int id;
            public bool active;
            public bool advancingStage;
            public string description;
            public string target;
            public string iconPath;
            public static List<Questing.ClientMessage> Instances { get; set; } = new List<Questing.ClientMessage>();

            //Constructors
            public ClientMessage()
            {
                description = "bad";
                iconPath = "custom";
                active = false;
                advancingStage = false;
            }

            public ClientMessage(string target) : this()
            {
                this.target = target;
            }

            // Methdods
            public override void Deserialize(NetworkReader reader)
            {
                id = reader.ReadInt32();
                active = reader.ReadBoolean();
                advancingStage = reader.ReadBoolean();
                description = reader.ReadString();
                target = reader.ReadString();
                iconPath = reader.ReadString();
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(id);
                writer.Write(active);
                writer.Write(advancingStage);
                writer.Write(description);
                writer.Write(target);
                writer.Write(iconPath);
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
                NetworkServer.SendToAll(Config.questPort, this);
            }
        }

        // Server message for each quest
        public class ServerMessage
        {
            // Attributes
            public PickupDef drop;
            public int objective;
            public int progress;
            public Type type;
            public bool awaitingClientMessage;
            public static List<ServerMessage> Instances { get; private set; } = new List<ServerMessage>();

            // Constructors
            public ServerMessage() {
                progress = 0;
                drop = Quest.GetQuestDrop();
                objective = 100;
                awaitingClientMessage = false;
            }

            public ServerMessage(Type type) : this() {
                this.type = type;
            }

            // Methods
            public void RegisterInstance() {
                Instances.Add(this);
            }
            
        }
    } // namespace Questing
} // namespace RPGMod