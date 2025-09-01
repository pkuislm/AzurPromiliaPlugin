using AzurPromiliaPlugin;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Lens.Tools.RuntimeDebug;
using Lens.Tools.RuntimeDebug.Actions;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;
using UniverseLib.UI.Widgets.ScrollView;
using InputManager = UniverseLib.Input.InputManager;

namespace AzurPromiliaPlugin
{
    public class GMPanel : PanelBase
    {
        public override string Name => "GMPanel --Press F1 to display/hide";
        public override int MinWidth => 100;
        public override int MinHeight => 300;
        public override Vector2 DefaultAnchorMin => new(0.1f, 0.1f);
        public override Vector2 DefaultAnchorMax => new(0.7f, 0.7f);
        public override Vector2 DefaultPosition => new(0.1f, 0.1f);
        System.Timers.Timer debounceTimer = new()
        {
            Interval = 1000,
            AutoReset = false
        };
        GMButtonTab tab;
        bool needUpdate = false;

        public GMPanel(UIBase owner):base(owner) { }

        protected override void ConstructPanelContent()
        {
            tab = new GMButtonTab();
            tab.ConstructUI(ContentRoot);

            debounceTimer.Elapsed += (s, e) =>
            {
                needUpdate = true;
            };

            AzurPlugin.OnGMFunctionsChanged += () =>
            {
                debounceTimer.Stop();
                debounceTimer.Start();
            };
        }

        public override void Update()
        {
            if (needUpdate)
            {
                tab.Groups.Apply(AzurPlugin.GMAction.ToList());
                LayoutRebuilder.MarkLayoutRootForRebuild(this.Rect);
                needUpdate = false;
            }
            base.Update();
        }
    }


    public class GMButtonTab : UIModel
    {
        public override GameObject UIRoot => uiRoot;
        private GameObject uiRoot;
        public GMFunctionGroup Groups { get; set; }
     
        public override void ConstructUI(GameObject go)
        {
            uiRoot = UIFactory.CreateUIObject("root", go);
            UIFactory.SetLayoutElement(uiRoot, flexibleHeight: 9999, minWidth: 100, flexibleWidth:9999, minHeight:100);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(uiRoot, true, true, true, true, 5, 2, 2, 2, 2);

            var g = UIFactory.CreateScrollPool<GMFunctionGroupCell>(uiRoot, "GroupScrollPool", out GameObject scrollObj, out GameObject scrollContent);
            UIFactory.SetLayoutElement(scrollObj, flexibleWidth: 9999, flexibleHeight: 9999, minHeight: 200, minWidth:200);
            UIFactory.SetLayoutElement(scrollContent, flexibleHeight: 9999, minWidth: 200, flexibleWidth:9999, minHeight:200);
            Groups = new GMFunctionGroup(g);
        }
    }

    public class GMFunctionInfo
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Action<string> Callback { get; set; }
    }

    [BepInPlugin("org.pkuism.plugins.azurpromiliaplugin", "AzurPromilia Plugin", "1.0.0.0")]
    public class AzurPlugin : BasePlugin
    {
        public static ManualLogSource PluginLog { get; private set; } = new ManualLogSource("AzurPlugin");
        public static UIBase UiBase { get; private set; }
        public static Harmony Harmony { get; private set; } = new Harmony("org.pkuism.plugins.azurpromiliaplugin");
        public static GMPanel MyPanel { get; private set; }
        public static Dictionary<string, List<GMFunctionInfo>> GMAction = new();
        public static event Action OnGMFunctionsChanged;
        static Regex UrlPattern = new(@"https?://(api-grp(.+)?\.manjuu\.com|l\d-cb-version-all-p\d-cn\.manjuu\.com)");
        static string ServerUrl = "http://localhost:5000";

        public static void AddFunction(string key, GMFunctionInfo func)
        {
            if (!GMAction.ContainsKey(key))
                GMAction[key] = new List<GMFunctionInfo>();
            GMAction[key].Add(func);
            OnGMFunctionsChanged?.Invoke();
        }

        public static void RemoveFunction(string key, GMFunctionInfo func)
        {
            if (GMAction.TryGetValue(key, out var list) && list.Remove(func))
                OnGMFunctionsChanged?.Invoke();
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(Debug), "isDebugBuild", MethodType.Getter)]
        public static void IsUnityDebugBuild(ref bool __result)
        {
            __result = true;
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(RuntimeDebugSystem), "IsSystemEnabled", MethodType.Getter)]
        public static void IsSystemEnabled(ref bool __result)
        {
            __result = true;
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(RuntimeDebugSystem), "IsVisible", MethodType.Getter)]
        public static void IsVisible(ref bool __result)
        {
            __result = true;
        }

        //[HarmonyPostfix, HarmonyPatch(typeof(InfoManager), "ApiUrl", MethodType.Getter)]
        public static void ApiUrl(ref string __result)
        {
            __result = UrlPattern.Replace(__result, ServerUrl);
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(UnityWebRequest), "url", MethodType.Setter)]
        public static void WebReqUrl(ref string value)
        {
            value = UrlPattern.Replace(value, ServerUrl);
        }

        public static void UrlRedirector(ref string url)
        {
            url = UrlPattern.Replace(url, ServerUrl);
        }

        public static void InvokeAction(BaseDebugAction act, string arg = "")
        {
            try
            {
                var type = act.GetActualType();
                if (type == typeof(DebugActionInput))
                {
                    /*ai.WithInputQuery(new Lens.Tools.RuntimeDebug.DebugInput.InputQuery()
                    {

                    });*/
                    act.Cast<DebugActionInput>()._ResolveAction_b__8_0(arg);
                }
                else if(type == typeof(DebugActionButton))
                {
                    act.__ctor_b__13_0();
                }
                act.ResolveAction();
            }
            catch (Exception ex)
            {
                PluginLog.LogError(ex.Message);
                PluginLog.LogError(ex.StackTrace);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RuntimeDebugActionHelper), "RegisterButton")]
        public static void RegisterButton(BaseDebugAction __result, string groupName, string actionName, Action action, bool autoClosePanel = true, string desc = null, string shortcut = null)
        {
            PluginLog.LogInfo($"RegisterButton called: {groupName}.{actionName}");
            AddFunction(groupName, new GMFunctionInfo()
            {
                Type = 0,
                Name = actionName,
                Description = desc,
                Callback = (arg) => { InvokeAction(__result); }
            });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RuntimeDebugActionHelper), "RegisterInput")]
        public static void RegisterInput(BaseDebugAction __result, string groupName, string actionName, Action<string> action, bool autoClosePanel = true, string desc = null, string shortcut = null)
        {
            PluginLog.LogInfo($"RegisterInput called: {groupName}.{actionName}");
            AddFunction(groupName, new GMFunctionInfo()
            {
                Type = 1,
                Name = actionName,
                Description = desc,
                Callback = (arg) => { InvokeAction(__result, arg); }
            });
        }

        public override void Load()
        {
            Harmony.PatchAll(typeof(AzurPlugin));
            //var redirector = typeof(AzurPlugin).GetMethod("UrlRedirector");
            //if (redirector != null) 
            //{
            //    Harmony.Patch(typeof(HttpManager).GetMethod("Post"), new HarmonyMethod(redirector));
            //    Harmony.Patch(typeof(HttpManager).GetMethod("PostForText"), new HarmonyMethod(redirector));
            //    Harmony.Patch(typeof(HttpManager).GetMethod("PostForData"), new HarmonyMethod(redirector));
            //    Harmony.Patch(typeof(HttpManager).GetMethod("PostJsonForText"), new HarmonyMethod(redirector));
            //    Harmony.Patch(typeof(HttpManager).GetMethod("Get"), new HarmonyMethod(redirector));
            //    Harmony.Patch(typeof(HttpManager).GetMethod("GetData"), new HarmonyMethod(redirector));
            //    Harmony.Patch(typeof(HttpManager).GetMethod("GetJson"), new HarmonyMethod(redirector));
            //    Harmony.Patch(typeof(HttpManager).GetMethod("GetText"), new HarmonyMethod(redirector));
            //    Harmony.Patch(typeof(HttpManager).GetMethod("GetTexture"), new HarmonyMethod(redirector));
            //}

            BepInEx.Logging.Logger.Sources.Add(PluginLog);

            Universe.Init(1f, OnInitialized, LogHandler, new()
            {
                Disable_EventSystem_Override = false,
                Force_Unlock_Mouse = true,
                Disable_Setup_Force_ReLoad_ManagedAssemblies = false,
                Unhollowed_Modules_Folder = Path.Combine(Paths.BepInExRootPath, "interop")
            });

            PluginLog.LogInfo($"Plugin is loaded!");
        }

        public override bool Unload()
        {
            BepInEx.Logging.Logger.Sources.Remove(PluginLog);
            return true;
        }

        void UiUpdate()
        {
            if (InputManager.GetKeyDown(KeyCode.F1))
            {
                MyPanel.Toggle();
            }
        }

        void OnInitialized()
        {
            UiBase = UniversalUI.RegisterUI("org.pkuism.plugins.azurpromiliaplugin", UiUpdate);
            MyPanel = new(UiBase);
        }

        void LogHandler(string message, LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    PluginLog.LogWarning(message);
                    break;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    PluginLog.LogError(message);
                    break;
                default:
                    PluginLog.LogMessage(message);
                    break;
            }
        }
    }
}