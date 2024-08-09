using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiskOfCodePlugin
{
    public static class ItemHelper
    {
        private static Random _rng = new Random();
        private static Dictionary<ItemTier, List<ItemDef>> _allItems;

        public static ItemDef GetRandomItem(RoR2.ItemTier tier)
        {
            _allItems ??= ItemCatalog.allItems
                .Select(ItemCatalog.GetItemDef)
                .GroupBy(x => x.tier)
                .ToDictionary(x => x.Key, x => x.ToList());

            var i = _rng.Next(_allItems[tier].Count);
            return _allItems[tier][i];
        }

        public static ItemDef GetRandomItem()
        {
            var randomTier = _allItems.Keys.ElementAt(_rng.Next(_allItems.Count));

            return GetRandomItem(randomTier);
        }
    }
}
