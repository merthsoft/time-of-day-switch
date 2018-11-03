using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TimerSwitches {
    [StaticConstructorOnStartup]
    public class TimeOfDaySwitch : Building_PowerSwitch {
        public const int HOURS_IN_DAY = GenDate.HoursPerDay;
        private static Texture2D icon;

        public static Texture2D OffColor => TimeAssignmentDefOf.Anything.ColorTexture;
        public static Texture2D OnColor => TimeAssignmentDefOf.Sleep.ColorTexture;

        protected List<bool> states;
        protected bool isTurnedOn = true;
        protected bool previousState = false;

        public override bool TransmitsPowerNow => isTurnedOn && GetState(GenLocalDate.HourOfDay(this));

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);

            states = Enumerable.Repeat(true, HOURS_IN_DAY).ToList();

            LongEventHandler.ExecuteWhenFinished(() => icon = ContentFinder<Texture2D>.Get("UI/Commands/SetTime", true));
        }

        void designateTimes() {
            var allTabs = DefDatabase<MainButtonDef>.AllDefs;
            
            var timersTab = allTabs.First(t => t.defName == "Timers");
            (timersTab.TabWindow as TimersTab).CurrentTimer = this;
            Find.MainTabsRoot.SetCurrentTab(timersTab);
        }

        public override IEnumerable<Gizmo> GetGizmos() {
            List<Gizmo> baseGizmos = base.GetGizmos().ToList();
            for (int i = 0; i < baseGizmos.Count; i++) {
                yield return baseGizmos[i];
            }
            
            yield return new Command_Action() {
                defaultLabel = "Set Times",
                defaultDesc = "Set the times that this switch is on or off.",
                hotKey = KeyBindingDefOf.Misc2,
                activateSound = SoundDef.Named("Click"),
                action = designateTimes,
                groupKey = 313123001,
                icon = icon,
            };
        }

        public void SetState(int hour, bool state) {
            states[hour] = state;
        }

        public bool GetState(int hour) { return states[hour]; }

        public override void Tick() {
            base.Tick();
            updatePower();
        }

        private void updatePower() {
            if (previousState != TransmitsPowerNow) {
                Log.Message("Updating power state changed.", true);
                Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(PowerComp);
                previousState = TransmitsPowerNow;
            }
        }

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Collections.Look<bool>(ref states, "timeOfDaySwitch");
            Scribe_Values.Look<bool>(ref isTurnedOn, "isTurnedOn");

            previousState = !TransmitsPowerNow;
        }

        protected override void ReceiveCompSignal(string signal) {
            switch (signal) {
                case "FlickedOff":
                    isTurnedOn = false;
                    break;
                case "FlickedOn":
                    isTurnedOn = true;
                    break;
            }
        }
    }
}
