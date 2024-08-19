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
using BepInEx.Logging;
using static RiskOfCodePlugin.RiskOfCodePlugin;

namespace RiskOfCodePlugin.Plugins
{
    public class StatisticPrinterPlugin : ICustomPlugin
    {
        private ManualLogSource logger;

        public StatisticPrinterPlugin(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public void Awake()
        {

            SceneExitController.onBeginExit += SceneExitController_onBeginExit;

        }

        public void Uninitialize()
        {
            SceneExitController.onBeginExit -= SceneExitController_onBeginExit;
        }

        private void SceneExitController_onBeginExit(SceneExitController obj)
        {
            PrintPlayersDamageStats(source: PrintSource.Server);
        }

        // The Update() method is run on every frame of the game.
        public void Update()
        {
            // Test score output:
            if (Input.GetKeyDown(KeyCode.F3))
            {
                PrintPlayersDamageStats(source: PrintSource.Client);
            }
            //if (Input.GetKeyDown(KeyCode.F4))
            //{
            //    GambleAllPlayerItems();
            //}

            // // This is a bypass to skip the game and test the stage progression event using a hotkey.
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
    }
}
