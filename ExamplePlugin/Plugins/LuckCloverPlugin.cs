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

namespace RiskOfCodePlugin.Plugins
{
    public class LuckCloverPlugin : ICustomPlugin
    {
        private ManualLogSource logger;

        public LuckCloverPlugin(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public void Awake()
        {
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        public void Uninitialize()
        {
            Run.onRunStartGlobal -= Run_onRunStartGlobal;
        }

        public void Update()
        {
            // nothing
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            Chat.AddMessage($"<style=cEvent>Luck is granted to stay on your side...</style>");

            foreach (CharacterMaster characterMaster in PlayerCharacterMasterController.instances.Select(p => p.master))
            {
                characterMaster.inventory.GiveItem(RoR2Content.Items.Clover);
            }
        }
    }
}
