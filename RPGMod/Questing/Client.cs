using System.Collections.Generic;
using RoR2;

namespace RPGMod.Questing
{
    internal static class Client
    {
        private static QuestData _questData;
        private static UI.Quest _questUI;
        public static QuestData QuestData
        {
            get => _questData;
            set
            {
                if (_questData?.Guid != value?.Guid)
                {
                    _questUI?.Destroy();
                }

                _questData = value;

                if (_questData == null)
                {
                    return;
                }

                if (_questUI == null && !((bool)_questData?.Complete))
                {
                    var localUser = LocalUserManager.GetFirstLocalUser();

                    if (localUser?.cameraRigController?.hud?.mainContainer != null)
                    {
                        _questUI = localUser.cameraRigController.hud.mainContainer.AddComponent<UI.Quest>();
                    }
                }
                else if (_questUI != null)
                {
                    _questUI.UpdateData(_questData);
                }
            }
        }

        public static List<Announcement> Announcements { get; set; } = new List<Announcement>();

        private static UI.Announcer _announcerUI;

        public static void Update()
        {
            if (Announcements.Count <= 0 || _announcerUI != null)
            {
                return;
            }

            var localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser?.cameraRigController?.hud?.mainContainer != null)
            {
                _announcerUI = localUser.cameraRigController.hud.mainContainer.AddComponent<UI.Announcer>();
                _announcerUI.SetMessage(Announcements[0].Message);
            }
            Announcements.RemoveAt(0);
        }

        public static void CleanUp()
        {
            _questUI?.Destroy();
            _announcerUI?.Destroy();
            Announcements.Clear();
            _questUI = null;
            _questData = null;
        }
    }
}