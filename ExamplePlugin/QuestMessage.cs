using UnityEngine.Networking;

namespace RPGMod
{
    // Quest Message that gets sent to all clients
    public class QuestMessage : MessageBase
    {
        public int questID;
        public bool questInitialised;
        public string questDescription;
        public string questTarget;
        public string questTargetName;

        public override void Deserialize(NetworkReader reader)
        {
            questID = reader.ReadInt32();
            questInitialised = reader.ReadBoolean();
            questDescription = reader.ReadString();
            questTarget = reader.ReadString();
            questTargetName = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(questID);
            writer.Write(questInitialised);
            writer.Write(questDescription);
            writer.Write(questTarget);
            writer.Write(questTargetName);
        }
    }

    // All server side data
    public struct QuestServerData
    {
        public RoR2.PickupIndex drop;
        public int objective;
        public int progress;
        public int type;
    }
}