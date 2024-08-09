using BepInEx;
using R2API;
using RoR2;
using RoR2.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace RiskOfCodePlugin
{
    // This is an example plugin that can be put in
    // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    // It's a small plugin that adds a relatively simple item to the game,
    // and gives you that item whenever you press F2.

    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
    // You don't need this if you're not using R2API in your plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(ItemAPI.PluginGUID)]

    // Require R2API ContentManager dependency for loading new skill information.
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]

    // This one is because we use a .language file for language tokens
    // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class RiskOfCodePlugin : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Vertex";
        public const string PluginName = "RiskOfCode";
        public const string PluginVersion = "1.0.3";

        // Initialise and instatiate randomizer.
        private System.Random random = new System.Random();

        // Enum for source to differentiate between server and client printing
        public enum PrintSource
        {
            Server,
            Client
        }


        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            // Keep track when the stage is ended to print stats at the end of the round.
            SceneExitController.onBeginExit += SceneExitController_onBeginExit;

            // Hook into the character body initialization
            // On.RoR2.CharacterBody.Start += CharacterBody_Start;
            // On.RoR2.MasterSummon.Perform += MasterSummon_Perform;

            Run.onRunStartGlobal += Run_onRunStartGlobal;
            On.RoR2.Run.AdvanceStage += Run_AdvanceStage;

            PatchEngineerTurretsCapacity();
        }

        private static void PatchEngineerTurretsCapacity()
        {
            // Patch the amount of deployable turrets the engineer can have depending on magazines.
            IL.RoR2.CharacterMaster.GetDeployableSameSlotLimit += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext([
                    (Instruction x) => ILPatternMatchingExt.MatchLdcI4(x, 3)
                ]);
                c.Remove();
                c.Emit(OpCodes.Ldarg_0);
                Func<CharacterMaster, int> func = (CharacterMaster b) => 2 + b.inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid);
                c.EmitDelegate<Func<CharacterMaster, int>>(func);
            };
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
            var players = PlayerCharacterMasterController.instances.Select(x => x.master).ToList();

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

        private void Run_onRunStartGlobal(Run obj)
        {
            Chat.AddMessage($"<style=cEvent>Luck is granted to stay on your side...</style>");

            foreach (CharacterMaster characterMaster in PlayerCharacterMasterController.instances.Select(p => p.master))
            {
                characterMaster.inventory.GiveItem(RoR2Content.Items.Clover);
            }
        }

        //private CharacterMaster MasterSummon_Perform(On.RoR2.MasterSummon.orig_Perform orig, MasterSummon self)
        //{
        //    Chat.AddMessage("Setting MasterSummon ignore limit to true.");
        //    self.ignoreTeamMemberLimit = true;
        //    TeamIndex teamIndex = TeamComponent.GetObjectTeam(self.summonerBodyObject);
        //    TeamDef teamDef = TeamCatalog.GetTeamDef(teamIndex);
        //    if (teamDef == null)
        //    {
        //        return orig(self);
        //    }
        //    teamDef.softCharacterLimit = 99;
        //    Chat.AddMessage($"Current MasterSummon spawn count: {TeamComponent.GetTeamMembers(teamIndex).Count}, soft limit: {teamDef.softCharacterLimit}.");
        //    return orig(self);
        //}

        //private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        //{
        //    // Call the original method first
        //    orig(self);

        //    // Check if this is the Engineer body
        //    if (self.bodyIndex == BodyCatalog.FindBodyIndex("EngiBody"))
        //    {
        //        SkillLocator skillLocator = self.GetComponent<SkillLocator>();
        //        if (skillLocator != null)
        //        {
        //            GenericSkill specialSkill = skillLocator.special;
        //            if (specialSkill != null)
        //            {
        //                var skillDef = specialSkill.skillDef;
        //                if (skillDef != null)
        //                {
        //                    // Modify the skill parameters directly
        //                    skillDef.baseMaxStock = 10;
        //                    skillDef.baseRechargeInterval = 2.5f;

        //                    // Log the changes to ensure they are applied
        //                    Log.Info("Engineer turret skill modified successfully.");
        //                }
        //            }
        //        }
        //    }
        //}

        private void PrintPlayersDamageStats(PrintSource source = PrintSource.Server)
        {
            var playerStatsList = new List<(string playerName, ulong totalDamage, ulong maximumHit)>();

            foreach (var playerController in PlayerCharacterMasterController.instances)
            {
                CharacterMaster master = playerController.master;
                if (master != null)
                {
                    PlayerStatsComponent statsComponent = master.playerStatsComponent;
                    if (statsComponent != null)
                    {
                        StatSheet statSheet = statsComponent.currentStats;
                        // Use ternary operator to determine player name or fallback to character name
                        string characterName = Language.GetString(master.bodyPrefab.GetComponent<CharacterBody>().baseNameToken) ?? "Unknown";
                        string playerName = playerController.GetDisplayName() ?? characterName;

                        ulong damageMaxHit = statSheet.GetStatValueULong(StatDef.highestDamageDealt);
                        ulong damageMinions = statSheet.GetStatValueULong(StatDef.totalMinionDamageDealt);
                        ulong damageDealt = statSheet.GetStatValueULong(StatDef.totalDamageDealt);
                        ulong damageTotal = damageDealt + damageMinions;

                        playerStatsList.Add((playerName, damageTotal, damageMaxHit));
                    }
                }
            }

            var sortedPlayerStats = playerStatsList.OrderByDescending
                (stat => stat.totalDamage).ToList();

            StringBuilder statisticsMessage = new StringBuilder();
            foreach (var playerStats in sortedPlayerStats)
            {
                statisticsMessage.AppendLine($"{playerStats.playerName}'s damage: <style=cIsDamage>{playerStats.totalDamage}</style>, max hit: <style=cDeath>{playerStats.maximumHit}</style>");
            }

            Chat.SimpleChatMessage simpleMessage = new Chat.SimpleChatMessage
            {
                baseToken = $"<style=cEvent>{statisticsMessage.ToString().TrimEnd('\r', '\n')}</style>",
            };

            switch (source)
            {
                case PrintSource.Server:
                    Chat.SendBroadcastChat(simpleMessage);
                    break;
                case PrintSource.Client:
                    Chat.AddMessage(simpleMessage);
                    break;
                default:
                    break;
            }
        }

        private void SceneExitController_onBeginExit(SceneExitController obj)
        {
            PrintPlayersDamageStats(source: PrintSource.Server);
        }


        // The Update() method is run on every frame of the game.
        private void Update()
        {
            // Test score output:
            if (Input.GetKeyDown(KeyCode.F3))
            {
                PrintPlayersDamageStats(source: PrintSource.Client);
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                GambleAllPlayerItems();
            }

            // This is a bypass to skip the game and test the stage progression event using a hotkey.
            //if (Input.GetKeyDown(KeyCode.F4))
            //{
            //    Chat.AddMessage("Force progress to trigger the next stage...");
            //    SceneExitController sceneExitController = FindObjectOfType<SceneExitController>();
            //    if (sceneExitController != null)
            //    {
            //        Chat.AddMessage("SceneExitController instance loaded!");
            //        sceneExitController.SetState(SceneExitController.ExitState.ExtractExp);
            //    }
            //}
        }
    }
}
