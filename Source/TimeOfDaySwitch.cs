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

    public static Texture2D OffColor => TimeAssignmentDefOf.Anything.ColorTexture;

    public static Texture2D OnColor => TimeAssignmentDefOf.Sleep.ColorTexture;

    protected bool[] states = Enumerable.Repeat(true, GenDate.HoursPerDay).ToArray();
    protected bool isTurnedOn = true;
    protected bool? previousState;

    public override bool TransmitsPowerNow => isTurnedOn && GetState(GenLocalDate.HourOfDay(this));

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

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

        void designateTimes()
        {
            var allTabs = DefDatabase<MainButtonDef>.AllDefs;

            var timersTab = allTabs.First(t => t.defName == "Timers");
            (timersTab.TabWindow as TimersTab).CurrentTimer = this;
            Find.MainTabsRoot.SetCurrentTab(timersTab);
        }
    }

    public void SetState(int hour, bool state) => states[hour] = state;

    public bool GetState(int hour) => states[hour];

    public override void TickRare()
    {
        base.TickRare();
        if (previousState != TransmitsPowerNow)
        {
            Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(PowerComp);
            previousState = TransmitsPowerNow;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        var stateList = states.ToList();
        Scribe_Collections.Look(ref stateList, "timeOfDaySwitch");
        states = stateList.ToArray();
        Scribe_Values.Look(ref isTurnedOn, nameof(isTurnedOn));

        previousState = !TransmitsPowerNow;
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
    }

    public void Copy()
        => ClipBoard.Copy(states);

    public void Paste()
        => ClipBoard.Paste(states);
}
