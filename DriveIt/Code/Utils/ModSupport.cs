using AlgernonCommons;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;

namespace DriveIt.Utils
{
    public class ModSupport
    {
        public static bool FoundUUI { get; private set; }

        internal static List<string> CheckModConflicts()
        {
            var conflictModNames = new List<string>();
            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (var assembly in plugin.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        //case "Assembly Name":
                        //    conflictModNames.Add("Friendly Name");
                        //    break;
                    }
                }
            }
            return conflictModNames;
        }
        internal static void Initialize()
        {
            try
            {
                Logging.Message("ModSupport: Start search for supported enabled mods");
                foreach (var plugin in PluginManager.instance.GetPluginsInfo())
                {
                    if (plugin.isEnabled)
                        foreach (var assembly in plugin.GetAssemblies())
                        {
                            switch (assembly.GetName().Name)
                            {
                                case "UnifiedUIMod":
                                    FoundUUI = true;
                                    Logging.KeyMessage("found UUI, version ", assembly.GetName().Version);
                                    break;
                            }
                        }
                }
            }

            catch (Exception e)
            {
                Logging.LogException(e, $"ModSupport: Failed to search mods");
            }
        }
    }
}

