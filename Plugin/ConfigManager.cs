using System;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using static DrakiaXYZ.BrainTest.VersionChecker.TarkovVersion;

#nullable enable

namespace RoamingBots.Interop
{
    public static class ConfigManager
    {
        private static ConfigEntry<T> BindProp<T>(ConfigFile Config, PropertyInfo Prop) where T : IComparable
        {
            return Config.Bind<T>(Prop.GetCustomAttribute<AutoBindConfig<T>>().Definition, Prop.GetCustomAttribute<AutoBindConfig<T>>().DefaultValue, Prop.GetCustomAttribute<AutoBindConfig<T>>()?.Description);
        }

        private static ConfigEntry<bool> BindBool(ConfigFile Config, PropertyInfo Prop)
        {
            return Config.Bind<bool>(Prop.GetCustomAttribute<AutoBindBoolConfig>()?.Definition, Prop.GetCustomAttribute<AutoBindBoolConfig>().DefaultValue, Prop.GetCustomAttribute<AutoBindBoolConfig>()?.Description);
        }

        public static void RegisterBinds(RoamingBotsPlugin Target, ConfigFile Config, ManualLogSource? Logging)
        {
            // loop through all properties of the target.
            foreach (var prop in typeof(RoamingBotsPlugin).GetProperties())
            {
                bool bPrintBind = true;
                if (prop.GetCustomAttribute<AutoBindConfig<float>>() != null)
                {
                    prop.SetValue(Target, BindProp<float>(Config, prop));
                }
                else if (prop.GetCustomAttribute<AutoBindConfig<int>>() != null)
                {
                    prop.SetValue(Target, BindProp<int>(Config, prop));
                }
                else if (prop.GetCustomAttribute<AutoBindBoolConfig>() != null)
                {
                    prop.SetValue(Target, BindBool(Config, prop));
                }
                else
                {
                    bPrintBind = false;
                }

                if (bPrintBind)
                {
                    Logging?.LogDebug($"Binding {prop.Name}...");
                }
            }
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class AutoBindConfig<T> : System.Attribute where T : IComparable
    {
        public ConfigDefinition Definition;
        public ConfigDescription Description;
        public T DefaultValue;
        public bool Advanced = false;
        public int Order = 0;
        public bool ShowRangeAsPercent = false;
        public AutoBindConfig(string section, string key, string desc, T DefValue, T MinVlaue, T MaxValue)
        {
            Definition = new ConfigDefinition(section, key);
            Description = new ConfigDescription(desc,
            new AcceptableValueRange<T>(MinVlaue, MaxValue), new ConfigurationManagerAttributes()
            {
                Category = section,
                DispName = key,
                Order = Order,
                IsAdvanced = Advanced,
                ShowRangeAsPercent = ShowRangeAsPercent
            });
            DefaultValue = DefValue;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected AutoBindConfig() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class AutoBindBoolConfig : AutoBindConfig<bool>
    {
        public AutoBindBoolConfig(string section, string key, string desc, bool DefValue)
        {
            Definition = new ConfigDefinition(section, key);
            Description = new ConfigDescription(desc, null, new ConfigurationManagerAttributes()
            {
                Category = section,
                DispName = key,
                Order = Order,
                IsAdvanced = Advanced,
                ShowRangeAsPercent = ShowRangeAsPercent
            });
            DefaultValue = DefValue;
        }
    }
}