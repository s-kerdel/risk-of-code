using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiskOfCodePlugin
{
    public static class PlayerHelper
    {
        public static Dictionary<RoR2.ItemTier, int> GetItemCountPerTier(this CharacterMaster character)
        {
            var mapping = MapItems(character);

            var retval = new Dictionary<RoR2.ItemTier, int>();

            foreach (var kvp in mapping)
            {
                var itemDef = kvp.Key;
                var amount = kvp.Value;

                if (retval.TryGetValue(itemDef.tier, out var oldAmount))
                {
                    retval[itemDef.tier] = oldAmount + amount;
                }
                else
                {
                    retval.Add(itemDef.tier, amount);
                }
            }

            return retval;
        }

        private static Dictionary<ItemDef, int> MapItems(CharacterMaster character)
        {
            var dict = new Dictionary<ItemDef, int>();
            var stacks = character.inventory.itemStacks;

            for (int i = 0; i < stacks.Length; i++)
            {
                var stackSize = stacks[i];
                if (stackSize > 0)
                {
                    var itemDef = ItemCatalog.GetItemDef((ItemIndex)i);
                    dict.Add(itemDef, stackSize);
                }
            }

            return dict;
        }

        //private static Dictionary<ItemIndex, int> MapItems(List<ItemIndex> itemsOrder, int[] stacks)
        //{
        //    var dict = new Dictionary<ItemIndex, int>(itemsOrder.Count);
        //    for (int i = 0; i < itemsOrder.Count; i++)
        //    {
        //        dict.Add(itemsOrder[i], stacks[i]);
        //    }

        //    return dict;
        //}
    }
}
