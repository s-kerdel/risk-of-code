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
    internal class EngineerTurretLimitPlugin : ICustomPlugin
    {
        private ManualLogSource logger;

        public EngineerTurretLimitPlugin(ManualLogSource logger)
        {
            this.logger = logger;
        }

        private static void PatchEngineerTurretsCapacity()
        {
            // Patch the amount of deployable turrets the engineer can have depending on magazines.
            IL.RoR2.CharacterMaster.GetDeployableSameSlotLimit += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext([
                    (x) => x.MatchLdcI4(3)
                ]);
                c.Remove();
                c.Emit(OpCodes.Ldarg_0);
                Func<CharacterMaster, int> func = (b) => 2 + b.inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid);
                c.EmitDelegate(func);
            };
        }

        public void Uninitialize()
        {
        }

        public void Update()
        {
        }

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            PatchEngineerTurretsCapacity();
        }
    }
}