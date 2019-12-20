using UnityEngine.Networking;

namespace RPGMod
{
    namespace Questing
    {
        // Quest Message that gets sent to all clients
        public class ClientMessage : MessageBase
        {
            public int id;
            public bool initialised;
            public string description;
            public string target;
            public string iconPath;

            public override void Deserialize(NetworkReader reader)
            {
                id = reader.ReadInt32();
                initialised = reader.ReadBoolean();
                description = reader.ReadString();
                target = reader.ReadString();
                iconPath = reader.ReadString();
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(id);
                writer.Write(initialised);
                writer.Write(description);
                writer.Write(target);
                writer.Write(iconPath);
            }
        }

        // All server side data
        public struct ServerMessage
        {
            public RoR2.PickupDef drop;
            public int objective;
            public int progress;
            public int type;
        }
    } // namespace Questing
} // namespace RPGMod