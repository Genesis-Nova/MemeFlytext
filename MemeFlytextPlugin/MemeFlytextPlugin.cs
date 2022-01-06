using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace MemeFlytext
{
    public unsafe class MemeFlytextPlugin : IDalamudPlugin
    {
        private const int TargetInfoGaugeBgNodeIndex = 41;
        private const int TargetInfoGaugeNodeIndex = 43;

        private const int TargetInfoSplitGaugeBgNodeIndex = 2;
        private const int TargetInfoSplitGaugeNodeIndex = 4;

        private const int FocusTargetInfoGaugeBgNodeIndex = 13;
        private const int FocusTargetInfoGaugeNodeIndex = 15;

        public string Name => "Meme Flytext";
        private const string CommandName = "/pmeme";

        private readonly Configuration _configuration;
        private readonly PluginUI _ui;
        public bool Desquish = false;
        public bool Zero = false;
        public bool Crazy = false;
        private readonly GameGui _gameGui;
        private readonly DalamudPluginInterface _pi;
        private readonly CommandManager _cmdMgr;
        private readonly FlyTextGui _ftGui;
        private readonly ObjectTable _objectTable;
        private readonly ClientState _clientState;
        private readonly TargetManager _targetManager;

        private delegate void AddScreenLogDelegate(
            FFXIVClientStructs.FFXIV.Client.Game.Character.Character* target,
            FFXIVClientStructs.FFXIV.Client.Game.Character.Character* source,
            FlyTextKind logKind,
            int option,
            int actionKind,
            int actionId,
            int val1,
            int val2,
            int val3,
            int val4);
        private readonly Hook<AddScreenLogDelegate> _addScreenLogHook;

        private delegate IntPtr WriteFlyTextDataDelegate(IntPtr a1, NumberArrayData* numberArray, uint numberArrayIndex, IntPtr a4, int a5, int* ftData, uint a7, uint a8);
        private readonly Hook<WriteFlyTextDataDelegate> _writeFlyTextHook;

        private Dictionary<uint, DamageType> _actionToDamageTypeDict;
        private readonly List<ScreenLogInfo> _actions;

        public MemeFlytextPlugin(
            [RequiredVersion("1.0")] GameGui gameGui,
            [RequiredVersion("1.0")] FlyTextGui ftGui,
            [RequiredVersion("1.0")] DalamudPluginInterface pi,
            [RequiredVersion("1.0")] CommandManager cmdMgr,
            [RequiredVersion("1.0")] DataManager dataMgr,
            [RequiredVersion("1.0")] ObjectTable objectTable,
            [RequiredVersion("1.0")] ClientState clientState,
            [RequiredVersion("1.0")] TargetManager targetManager,
            [RequiredVersion("1.0")] SigScanner scanner)
        {
            _gameGui = gameGui;
            _ftGui = ftGui;
            _pi = pi;
            _cmdMgr = cmdMgr;
            _objectTable = objectTable;
            _clientState = clientState;
            _targetManager = targetManager;

            _actionToDamageTypeDict = new Dictionary<uint, DamageType>();
            _actions = new List<ScreenLogInfo>();

            _configuration = pi.GetPluginConfig() as Configuration ?? new Configuration();
            _configuration.Initialize(pi, this);
            _ui = new PluginUI(_configuration, this);

            cmdMgr.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Display the Meme Flytext configuration interface."
            });

            try
            {
                var writeFtPtr = scanner.ScanText("E8 ?? ?? ?? ?? 83 F8 01 75 45");
                _writeFlyTextHook = new Hook<WriteFlyTextDataDelegate>(writeFtPtr, (WriteFlyTextDataDelegate)WriteFlyTextDataDetour);

                var addScreenLogPtr = scanner.ScanText("E8 ?? ?? ?? ?? BB ?? ?? ?? ?? EB 37");
                _addScreenLogHook = new Hook<AddScreenLogDelegate>(addScreenLogPtr, (AddScreenLogDelegate)AddScreenLogDetour);

                ftGui.FlyTextCreated += OnFlyTextCreated;
            }
            catch (Exception ex)
            {
                PluginLog.Information($"Encountered an error loading MemeFlytext: {ex.Message}");
                PluginLog.Information("Plugin will not be loaded.");

                _writeFlyTextHook?.Disable();
                _writeFlyTextHook?.Dispose();
                _addScreenLogHook?.Disable();
                _addScreenLogHook?.Dispose();
                cmdMgr.RemoveHandler(CommandName);

                throw;
            }

            _writeFlyTextHook.Enable();
            _addScreenLogHook.Enable();

            pi.UiBuilder.Draw += DrawUI;
            pi.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            _writeFlyTextHook?.Disable();
            _writeFlyTextHook?.Dispose();
            _addScreenLogHook?.Disable();
            _addScreenLogHook?.Dispose();

            _ftGui.FlyTextCreated -= OnFlyTextCreated;

            _actionToDamageTypeDict.Clear();
            _actionToDamageTypeDict = null;

            _ui.Dispose();
            _cmdMgr.RemoveHandler(CommandName);
            _pi.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            _ui.SettingsVisible = true;
        }

        private void DrawUI()
        {
            _ui.Draw();
        }

        private void DrawConfigUI()
        {
            _ui.SettingsVisible = true;
        }

        private List<uint> FindCharaPets()
        {
            var results = new List<uint>();
            var charaId = GetCharacterActorId();
            foreach (var obj in _objectTable)
            {
                if (obj is not BattleNpc npc) continue;

                var actPtr = npc.Address;
                if (actPtr == IntPtr.Zero) continue;

                if (npc.OwnerId == charaId)
                    results.Add(npc.ObjectId);
            }

            return results;
        }

        private uint GetCharacterActorId()
        {
            return _clientState?.LocalPlayer?.ObjectId ?? 0;
        }

        private SeString GetActorName(uint id)
        {
            foreach (var obj in _objectTable)
                if (obj != null)
                    if (id == obj.ObjectId)
                        return obj.Name;
            return "";
        }

        private void AddScreenLogDetour(
            FFXIVClientStructs.FFXIV.Client.Game.Character.Character* target,
            FFXIVClientStructs.FFXIV.Client.Game.Character.Character* source,
            FlyTextKind logKind,
            int option,
            int actionKind,
            int actionId,
            int val1,
            int val2,
            int val3,
            int val4)
        {
            try
            {
                var targetId = target->GameObject.ObjectID;
                var sourceId = source->GameObject.ObjectID;

                var action = new ScreenLogInfo
                {
                    actionId = (uint)actionId,
                    kind = logKind,
                    sourceId = sourceId,
                    targetId = targetId,
                    value = val1,
                };

                _actions.Add(action);
                _addScreenLogHook.Original(target, source, logKind, option, actionKind, actionId, val1, val2, val3, val4);
            }
            catch (Exception e)
            {
                PluginLog.Error(e, "An error occurred in meme flytext.");
            }
        }

        private IntPtr WriteFlyTextDataDetour(IntPtr a1, NumberArrayData* numberArray, uint numberArrayIndex, IntPtr a4, int a5, int* ftData, uint a7, uint a8)
        {
            var result = _writeFlyTextHook.Original(a1, numberArray, numberArrayIndex, a4, a5, ftData, a7, a8);

            if (numberArray == null || ftData == null) return result;
            if ((uint)numberArray->IntArray[numberArrayIndex + 5] != 0xFF0061E3) return result;

            try
            {
            }

            catch (Exception e)
            {
                PluginLog.Error(e, "An error occurred in meme flytext.");
            }
            return result;
        }

        private void OnFlyTextCreated(
            ref FlyTextKind kind,
            ref int val1,
            ref int val2,
            ref SeString text1,
            ref SeString text2,
            ref uint color,
            ref uint icon,
            ref float yOffset,
            ref bool handled)
        {
            try
            {
                if (Desquish)
                    val1 = MakeDamageDesquish(val1);

                if (Zero)
                    val1 = MakeDamageZero(val1);

                if (Crazy)
                    val1 = MakeDamageCrazy(val1);

                var ftKind = kind;
                var ftVal1 = val1;
                var charaId = GetCharacterActorId();
                var petIds = FindCharaPets();
                var action = _actions.FirstOrDefault(x => x.kind == ftKind && x.value == ftVal1 && TargetCheck(x, charaId, petIds));

                if (!_actions.Remove(action))
                {
                    return;
                }
            }
            catch (Exception e)
            {
                PluginLog.Information($"{e.Message} {e.StackTrace}");
            }
        }

        public int MakeDamageDesquish(int originalDamage)
        {
            var newDamage = originalDamage * 5;
            return newDamage;
        }

        public int MakeDamageZero(int originalDamage)
        {
            var newDamage = originalDamage * 0;
            return newDamage;
        }

        public int MakeDamageCrazy(int originalDamage)
        {
            var rand = new Random();
            var newDamage = rand.Next(int.MinValue, int.MaxValue);
            return newDamage;
        }

        private bool TargetCheck(ScreenLogInfo screenLogInfo, uint charaId, List<uint> petIds)
        {
            return screenLogInfo.sourceId == charaId || screenLogInfo.targetId == charaId || petIds.Contains(screenLogInfo.sourceId);
        }

        private SeString GetNewText(uint sourceId, SeString originalText)
        {
            SeString name = GetActorName(sourceId);
            var newPayloads = new List<Payload>();

            if (name.Payloads.Count == 0) return originalText;

            switch (_clientState.ClientLanguage)
            {
                case ClientLanguage.Japanese:
                    newPayloads.AddRange(name.Payloads);
                    newPayloads.Add(new TextPayload("から"));
                    break;
                case ClientLanguage.English:
                    newPayloads.Add(new TextPayload("from "));
                    newPayloads.AddRange(name.Payloads);
                    break;
                case ClientLanguage.German:
                    newPayloads.Add(new TextPayload("von "));
                    newPayloads.AddRange(name.Payloads);
                    break;
                case ClientLanguage.French:
                    newPayloads.Add(new TextPayload("de "));
                    newPayloads.AddRange(name.Payloads);
                    break;
                default:
                    newPayloads.Add(new TextPayload(">"));
                    newPayloads.AddRange(name.Payloads);
                    break;
            }

            if (originalText.Payloads.Count > 0)
                newPayloads.AddRange(originalText.Payloads);

            return new SeString(newPayloads);
        }

        private SeString GetName(uint id)
        {
            return _objectTable.SearchById(id)?.Name ?? SeString.Empty;
        }
    }
}