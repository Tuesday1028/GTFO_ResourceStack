using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace Hikaria.ResourceStack
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    internal class EntryPoint : BasePlugin
    {
        public override void Load()
        {
            Instance = this;
            m_Harmony = new Harmony("Hikaria.ResourceStack");
            m_Harmony.PatchAll();
            Logs.LogMessage("OK");
        }

        public static EntryPoint Instance;

        private Harmony m_Harmony;
    }
}
