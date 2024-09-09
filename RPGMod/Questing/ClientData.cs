using RoR2;

namespace RPGMod.Questing
{
    public class ClientData
    {
        public QuestData QuestData { get; private set; }

        public readonly NetworkUser NetworkUser;
        public int QuestsCompleted;

        public ClientData(NetworkUser networkUser)
        {
            QuestsCompleted = 0;
            NetworkUser = networkUser;
            NewQuest();
        }

        public void NewQuest()
        {
            if (Server.AllowedTypes.Count <= 0)
            {
                return;
            }

            QuestData = new QuestData(NetworkUser, QuestsCompleted, QuestData?.Guid ?? 0);
        }
    }
}