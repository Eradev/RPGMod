using UnityEngine.Networking;
using RoR2;

namespace RPGMod
{
    namespace Questing
    {
        // Quest Message that gets sent to all clients
        public class ClientMessage : MessageBase
        {
            public ClientMessage() {
                description = "bad";
                iconPath = "custom";
                active = false;
            }

            public ClientMessage(string target) : this() {
                this.target = target;
            }

            public int id;
            public bool active;
            public string description;
            public string target;
            public string iconPath;

            public override void Deserialize(NetworkReader reader)
            {
                id = reader.ReadInt32();
                active = reader.ReadBoolean();
                description = reader.ReadString();
                target = reader.ReadString();
                iconPath = reader.ReadString();
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(id);
                writer.Write(active);
                writer.Write(description);
                writer.Write(target);
                writer.Write(iconPath);
            }
        }

        // All server side data
        public class ServerMessage
        {
            public ServerMessage() {
                progress = 0;
                drop = Quest.GetQuestDrop();
                objective = MainDefs.random.Next(Config.questObjectiveMin, Quest.GetObjectiveLimit());
                active = true;
            }

            public ServerMessage(Type type) : this() {
                this.type = type;
            }

            public PickupDef drop;
            public int objective;
            public int progress;
            public Type type;
            public bool active;
        }
    } // namespace Questing
} // namespace RPGMod