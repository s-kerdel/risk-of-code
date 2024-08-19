using BepInEx.Logging;
using RiskOfCodePlugin.Helpers;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RiskOfCodePlugin.Plugins
{
    public class ItemGamblePlugin : ICustomPlugin
    {
        private ManualLogSource logger;

        public ItemGamblePlugin(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public void Awake()
        {
            On.RoR2.Run.AdvanceStage += Run_AdvanceStage;
        }

        public void Uninitialize()
        {
            On.RoR2.Run.AdvanceStage -= Run_AdvanceStage;
        }

        public void Update()
        {
            // nothing
        }

        private void Run_AdvanceStage(On.RoR2.Run.orig_AdvanceStage orig, Run self, SceneDef nextScene)
        {
            try
            {
                GambleAllPlayerItems();
            }
            catch (Exception e)
            {
                Chat.SimpleChatMessage exceptionMessage = new Chat.SimpleChatMessage
                {
                    baseToken = $"<style=cDeath>{e.Message}</style>",
                };
                Chat.SendBroadcastChat(exceptionMessage);
            }

            orig(self, nextScene);
        }

        private static void GambleAllPlayerItems()
        {
            // Gamble every player's inventory and balance the game while making things harder too.
            Chat.AddMessage($"<style=cEvent>Something strange is happening to your equipment...</style>");
            var players = new List<CharacterMaster>(PlayerCharacterMasterController.instances.Select(x => x.master));
            players.Shuffle();

            int totalPlayersItemCount = players.Select(p => p).Sum(c => c.inventory.itemStacks.Sum());

            Log.Info($"Total player item count: {totalPlayersItemCount}.");
            if (totalPlayersItemCount > 0)
            {
                var totalItemCountPerTier = players
                    .SelectMany(player => player.GetItemCountPerTier())
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(group => group.Key, group => group.Sum(kvp => kvp.Value));

                var itemCollectionPerTier = totalItemCountPerTier.Select(x => new
                {
                    Tier = x.Key,
                    RandomItems = Enumerable.Range(0, x.Value)
                    .Select(y => ItemHelper.GetRandomItem(x.Key))
                    .ToList()
                }).ToList();

                //var allItems = ItemCatalog.allItems.Select(x => new
                //{
                //    ItemIndex = x,
                //    ItemDef = ItemCatalog.GetItemDef(x)
                //});


                players.ForEach(x => x.inventory.CleanInventory());

                foreach (var tierCollection in itemCollectionPerTier)
                {
                    var chunks = tierCollection.RandomItems.SplitIntoChunks(players.Count);
                    chunks.Shuffle();

                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];
                        var chunk = chunks[i];

                        foreach (var item in chunk)
                        {
                            player.inventory.GiveItem(item);
                        }
                    }
                }
            }
        }
    }
}
