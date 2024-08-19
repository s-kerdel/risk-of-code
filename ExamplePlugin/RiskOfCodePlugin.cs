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
using RiskOfCodePlugin.Plugins;

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
        public const string PluginVersion = "1.0.4";
        private List<ICustomPlugin> _plugins = new List<ICustomPlugin>();

        public RiskOfCodePlugin()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            _plugins.Add(new ItemGamblePlugin(Logger));
            _plugins.Add(new StatisticPrinterPlugin(Logger));
            _plugins.Add(new LuckCloverPlugin(Logger));
            _plugins.Add(new EngineerTurretLimitPlugin(Logger));
        }

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            _plugins.ForEach(x => x.Awake());
        }

        // The Update() method is run on every frame of the game.
        private void Update()
        {
            foreach (var plugin in _plugins)
            {
                plugin.Update();
            }
        }
    }
}
