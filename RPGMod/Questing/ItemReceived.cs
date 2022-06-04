using System.Linq;
using RoR2;
using UnityEngine.Networking;

namespace RPGMod.Questing
{
    public class ItemReceived : MessageBase
    {
        public ItemIndex ItemIndex { get; private set; }

        private PickupIndex PickupIndex => PickupCatalog.FindPickupIndex(ItemIndex);

        public ItemReceived()
        {
            ItemIndex = ItemIndex.None;
        }

        public ItemReceived(ItemIndex itemIndexIndex)
        {
            ItemIndex = itemIndexIndex;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(ItemIndex);
        }

        public override void Deserialize(NetworkReader reader)
        {
            ItemIndex = reader.ReadItemIndex();
        }

        public static void Handler(NetworkMessage networkMessage)
        {
            var message = networkMessage.ReadMessage<ItemReceived>();

            if (message.ItemIndex == ItemIndex.None)
            {
                return;
            }

            var localPlayer = PlayerCharacterMasterController.instances.FirstOrDefault(x => x.networkUser.isLocalPlayer);

            if (localPlayer == null)
            {
                return;
            }

            localPlayer.networkUser.localUser?.userProfile.DiscoverPickup(message.PickupIndex);

            // ReSharper disable once InvertIf
            if (localPlayer.master.inventory.GetItemCount(message.ItemIndex) <= 1)
            {
                CharacterMasterNotificationQueue.PushPickupNotification(localPlayer.master, message.PickupIndex);
            }
        }
    }
}
