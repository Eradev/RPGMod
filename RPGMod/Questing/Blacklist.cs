using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace RPGMod.Questing
{
    public static class Blacklist
    {
        private static ItemMask _cachedAvailableItems = [];
        private static ItemMask _blacklistedItems = [];

        // ReSharper disable InconsistentNaming
        private static readonly List<PickupIndex> _availableTier1DropList = [];
        private static readonly List<PickupIndex> _availableTier2DropList = [];
        private static readonly List<PickupIndex> _availableTier3DropList = [];
        // ReSharper enable InconsistentNaming

        public static List<PickupIndex> AvailableTier1DropList => Get(_availableTier1DropList);
        public static List<PickupIndex> AvailableTier2DropList => Get(_availableTier2DropList);
        public static List<PickupIndex> AvailableTier3DropList => Get(_availableTier3DropList);

        public static void ClearCache()
        {
            _cachedAvailableItems = [];
        }

        private static void LoadBlackListItems()
        {
            _blacklistedItems = [];
            foreach (var itemName in ConfigValues.Questing.RewardBlacklist)
            {
                var item = ItemCatalog.FindItemIndex(itemName);

                if (item == ItemIndex.None)
                {
                    continue;
                }

                _blacklistedItems.Add(item);
            }
        }

        private static void ValidateItemCache()
        {
            if (Run.instance == null)
            {
                ClearCache();

                return;
            }

            if (_cachedAvailableItems.Equals(Run.instance.availableItems))
            {
                return;
            }

            _cachedAvailableItems = Run.instance.availableItems;

            LoadBlackListItems();

            var pairs = new[]
            {
                (_availableTier1DropList, Run.instance.availableTier1DropList),
                (_availableTier2DropList, Run.instance.availableTier2DropList),
                (_availableTier3DropList, Run.instance.availableTier3DropList)
            };

            foreach (var (validItems, source) in pairs)
            {
                validItems.Clear();
                validItems.AddRange(source.Where(pickupIndex =>
                {
                    var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);

                    return pickupDef != null && !_blacklistedItems.Contains(pickupDef.itemIndex);
                }));
            }
        }

        private static List<PickupIndex> Get(List<PickupIndex> list)
        {
            ValidateItemCache();

            return list;
        }
    }
}