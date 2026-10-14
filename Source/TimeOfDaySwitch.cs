using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Merthsoft.TimerSwitches;

[StaticConstructorOnStartup]
public class TimeOfDaySwitch : Building_PowerSwitch
{
    private static Texture2D icon;
    private Graphic offGraphic;
    private bool requireOccupancy;
    private bool roomOccupied;
    private int ticksUntilOccupancyCheck;
    private string occupancyStatusKey = "Merthsoft_TimeOfDaySwitches_NoRoom";

    public bool RequireOccupancy => requireOccupancy;

    public static Texture2D OffColor => TimeAssignmentDefOf.Anything.ColorTexture;

    public static Texture2D OnColor => TimeAssignmentDefOf.Sleep.ColorTexture;

    protected bool[] states = Enumerable.Repeat(true, GenDate.HoursPerDay).ToArray();
    protected bool isTurnedOn = true;
    protected bool? previousState;

    public override bool TransmitsPowerNow => Spawned && isTurnedOn && GetState(GenLocalDate.HourOfDay(this)) && (!requireOccupancy || roomOccupied);

    public override Graphic Graphic => TransmitsPowerNow ? DefaultGraphic : offGraphic ??=
        GraphicDatabase.Get(def.graphicData.graphicClass, def.graphicData.texPath + "_Off",
            def.graphicData.shaderType.Shader, def.graphicData.drawSize, DrawColor, DrawColorTwo);

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        RefreshOccupancy();
        previousState = TransmitsPowerNow;
        DirtyGraphic();

        LongEventHandler.ExecuteWhenFinished(() => icon = ContentFinder<Texture2D>.Get("UI/Commands/SetTime", true));
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        var baseGizmos = base.GetGizmos().ToList();
        for (var i = 0; i < baseGizmos.Count; i++)
            yield return baseGizmos[i];

        yield return new Command_Action {
            defaultLabel = "Merthsoft_TimeOfDaySwitches_SetTimes".Translate(),
            defaultDesc = "Merthsoft_TimeOfDaySwitches_SetTimes_Description".Translate(),
            hotKey = KeyBindingDefOf.Misc2,
            activateSound = SoundDef.Named("Click"),
            action = designateTimes,
            groupKey = 313123001,
            icon = icon,
        };

        yield return new Command_Toggle {
            defaultLabel = "Merthsoft_TimeOfDaySwitches_RequireOccupancy".Translate(),
            defaultDesc = "Merthsoft_TimeOfDaySwitches_RequireOccupancy_Description".Translate(),
            icon = icon,
            isActive = () => requireOccupancy,
            toggleAction = () => SetRequireOccupancy(!requireOccupancy),
        };

        void designateTimes()
        {
            var allTabs = DefDatabase<MainButtonDef>.AllDefs;

            var timersTab = allTabs.First(t => t.defName == "Timers");
            (timersTab.TabWindow as TimersTab).CurrentTimer = this;
            Find.MainTabsRoot.SetCurrentTab(timersTab);
        }
    }

    public void SetRequireOccupancy(bool enabled)
    {
        requireOccupancy = enabled;
        RefreshOccupancy();
        InternalTick();
    }

    public override string GetInspectString()
    {
        var text = base.GetInspectString();
        if (requireOccupancy)
            text += "\n" + "Merthsoft_TimeOfDaySwitches_OccupancyStatus".Translate(occupancyStatusKey.Translate());
        return text;
    }

    private Room FindOccupancyRoom(out bool ambiguous)
    {
        ambiguous = false;
        if (!Spawned) return null;
        var direct = RegionAndRoomQuery.RoomAt(Position, Map, RegionType.Set_Passable);
        if (direct != null)
            return IsEnclosedRoom(direct) ? direct : null;

        // Wall cells have no room. Never choose the first of multiple adjacent rooms.
        Room result = null;
        foreach (var cell in GenAdj.CellsAdjacentCardinal(this))
        {
            if (!cell.InBounds(Map)) continue;
            var adjacent = RegionAndRoomQuery.RoomAt(cell, Map, RegionType.Set_Passable);
            if (!IsEnclosedRoom(adjacent)) continue;
            if (result != null && result != adjacent)
            {
                ambiguous = true;
                return null;
            }
            result = adjacent;
        }
        return result;
    }

    private static bool IsEnclosedRoom(Room room)
        => room != null && room.ProperRoom && !room.UsesOutdoorTemperature && !room.PsychologicallyOutdoors;

    private void RefreshOccupancy()
    {
        ticksUntilOccupancyCheck = GenTicks.TickRareInterval;
        roomOccupied = false;
        if (!requireOccupancy || !Spawned) return;
        var room = FindOccupancyRoom(out var ambiguous);
        occupancyStatusKey = ambiguous ? "Merthsoft_TimeOfDaySwitches_AmbiguousRoom" : "Merthsoft_TimeOfDaySwitches_NoRoom";
        if (room == null) return;
        // Unlike a lamp, arbitrary circuits must stay enabled for sleeping occupants.
        foreach (var thing in room.ContainedAndAdjacentThings)
        {
            if (thing is Pawn pawn && pawn.Spawned && !pawn.Dead && pawn.GetRoom() == room)
            {
                roomOccupied = true;
                break;
            }
        }
        occupancyStatusKey = roomOccupied ? "Merthsoft_TimeOfDaySwitches_OccupiedRoom" : "Merthsoft_TimeOfDaySwitches_EmptyRoom";
    }

    public void SetState(int hour, bool state)
    {
        states[hour] = state;
        InternalTick();
    }

    public bool GetState(int hour) => states[hour];

    private void InternalTick()
    {
        if (!Spawned) return;
        var effectiveState = TransmitsPowerNow;
        if (previousState != effectiveState)
        {
            Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(PowerComp);
            previousState = effectiveState;
            DirtyGraphic();
        }
    }

    private void DirtyGraphic()
    {
        if (Spawned)
            Map.mapDrawer.MapMeshDirty(Position, (ulong)MapMeshFlagDefOf.Buildings | (ulong)MapMeshFlagDefOf.Things);
    }

    public override void TickRare()
    {
        base.TickRare();
        if (requireOccupancy) RefreshOccupancy();
        InternalTick();
    }

    protected override void Tick()
    {
        base.Tick();
        if (requireOccupancy && --ticksUntilOccupancyCheck <= 0) RefreshOccupancy();
        InternalTick();
    }

    public override void ExposeData()
    {
        base.ExposeData();

        var stateList = states.ToList();
        Scribe_Collections.Look(ref stateList, "timeOfDaySwitch");
        if (stateList != null && stateList.Count == GenDate.HoursPerDay)
            states = stateList.ToArray();
        Scribe_Values.Look(ref isTurnedOn, nameof(isTurnedOn));
        Scribe_Values.Look(ref requireOccupancy, nameof(requireOccupancy));

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            previousState = null;
    }

    protected override void ReceiveCompSignal(string signal)
    {
        switch (signal)
        {
            case CompFlickable.FlickedOffSignal:
                isTurnedOn = false;
                break;
            case CompFlickable.FlickedOnSignal:
                isTurnedOn = true;
                break;
        }
        InternalTick();
    }

    public void Copy()
        => ClipBoard.Copy(states);

    public void Paste()
    {
        ClipBoard.Paste(states);
        InternalTick();
    }
}
