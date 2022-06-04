using UnityEngine.Networking;

namespace RPGMod.Questing
{
    public class Announcement : MessageBase
    {
        public string Message { get; private set; }

        public Announcement()
        {
            Message = null;
        }

        public Announcement(string message)
        {
            Message = message;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Message);
        }

        public override void Deserialize(NetworkReader reader)
        {
            Message = reader.ReadString();
        }

        public static void Handler(NetworkMessage networkMessage)
        {
            Client.Announcements.Add(networkMessage.ReadMessage<Announcement>());
        }
    }
}