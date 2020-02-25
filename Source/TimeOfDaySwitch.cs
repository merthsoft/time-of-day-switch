﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TimerSwitches {
    [StaticConstructorOnStartup]
    public class TimeOfDaySwitch : Building_PowerSwitch {
        private static Texture2D icon;

        public static Texture2D OffColor => TimeAssignmentDefOf.Anything.ColorTexture;
        public static Texture2D OnColor => TimeAssignmentDefOf.Sleep.ColorTexture;

        protected bool[] states = Enumerable.Repeat(true, GenDate.HoursPerDay).ToArray();
        protected bool isTurnedOn = true;
        protected bool previousState = false;

        public override bool TransmitsPowerNow => isTurnedOn && GetState(GenLocalDate.HourOfDay(this));

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            
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
                Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(PowerComp);
                previousState = TransmitsPowerNow;
            }
        }

        public override void ExposeData() {
            base.ExposeData();

            var stateList = states.ToList();
            Scribe_Collections.Look<bool>(ref stateList, "timeOfDaySwitch");
            states = stateList.ToArray();
            Scribe_Values.Look<bool>(ref isTurnedOn, "isTurnedOn");

            Log.Message(string.Join(",", states.Select(b => b.ToString()).ToArray()));

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

        public void Copy()
            => ClipBoard.Copy(states);

        public void Paste()
            => ClipBoard.Paste(states);
    }
}
