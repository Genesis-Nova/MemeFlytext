using System;
using Dalamud.Game.Gui.FlyText;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MemeFlytext
{
    public unsafe struct CastbarInfo
    {
        public AtkUnitBase* unitBase;
        public AtkImageNode* gauge;
        public AtkImageNode* bg;
    }

    public struct ScreenLogInfo
    {
        public uint actionId;
        public FlyTextKind kind;
        public uint sourceId;
        public uint targetId;
        public int value;

        public override bool Equals(object o)
        {
            return
                o is ScreenLogInfo other
                && other.actionId == actionId
                && other.kind == kind
                && other.sourceId == sourceId
                && other.targetId == targetId
                && other.value == value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(actionId, (int)kind, sourceId, targetId, value);
        }

        public override string ToString()
        {
            return
                $"actionId: {actionId} kind: {kind} ({(int)kind}) sourceId: {sourceId} (0x{sourceId:X}) targetId: {targetId} (0x{targetId:X}) value: {value}";
        }
    }
}