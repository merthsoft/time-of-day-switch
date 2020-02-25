﻿using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TimerSwitches {
    class TimersTab : MainTabWindow {
        private static float hourWidth = 20.833334f;
        private static float hourHeight = 30f;
        private bool currentState = false;

        public TimeOfDaySwitch CurrentTimer { get; set; }

        public override Vector2 RequestedTabSize {
            get {
                return new Vector2(550, 160);
            }
        }

        public override void DoWindowContents(Rect fillRect) {
            base.DoWindowContents(fillRect);

            GUI.BeginGroup(fillRect);

            Rect position = new Rect(0f, 0f, fillRect.width, 65f);
            GUI.BeginGroup(position);
            Rect rect = new Rect(0f, 0f, 191f, position.height);
            drawTimeAssignmentSelectorGrid(rect);
            float num = 0f;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerLeft;
            for (int i = 0; i < GenDate.HoursPerDay; i++) {
                Rect rect2 = new Rect(num + 4f, 0f, hourWidth, position.height + 3f);
                Widgets.Label(rect2, i.ToString());
                num += hourWidth;
            }
            
            GUI.EndGroup();
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Rect rect5 = new Rect(0f, position.height, fillRect.width, fillRect.height - position.height);
            drawTimerRow(rect5);

            var pasteRect = new Rect(4, position.height + hourHeight + 4, CopyPasteUI.CopyPasteColumnWidth, CopyPasteUI.CopyPasteIconHeight);
            var pasteAction = ClipBoard.CanPaste ? (() => CurrentTimer.Paste()) : (Action)null;
            CopyPasteUI.DoCopyPasteButtons(pasteRect, () => CurrentTimer.Copy(), pasteAction);

            GUI.EndGroup();
        }

        private void drawTimeAssignmentSelectorGrid(Rect rect) {
            rect.yMax -= 2f;
            Rect rect2 = rect;
            rect2.xMax = rect2.center.x;
            rect2.yMax = rect2.center.y;
            this.drawTimeAssignmentSelectorFor(rect2, true);
            rect2.x += rect2.width;
            this.drawTimeAssignmentSelectorFor(rect2, false);
        }

        private void drawTimeAssignmentSelectorFor(Rect rect, bool state) {
            rect = rect.ContractedBy(2f);
            GUI.DrawTexture(rect, state ? TimeOfDaySwitch.OnColor : TimeOfDaySwitch.OffColor);
            if (Widgets.ButtonInvisible(rect, false)) {
                currentState = state;
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }

            GUI.color = Color.white;
            if (Mouse.IsOver(rect)) {
                Widgets.DrawHighlight(rect);
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(rect, (state ? "on" : "off").CapitalizeFirst());
            Text.Anchor = TextAnchor.UpperLeft;

            if (this.currentState == state) {
                Widgets.DrawBox(rect, 2);
            }
        }

        private void drawTimerRow(Rect rect) {
            GUI.BeginGroup(rect);
            float num = 0f;

            for (int i = 0; i < GenDate.HoursPerDay; i++) {
                Rect rect3 = new Rect(num, 0f, hourWidth, hourHeight);
                DoTimeAssignment(rect3, i);
                num += hourWidth;
            }
            GUI.EndGroup();
        }
        
        private void DoTimeAssignment(Rect rect, int hour) {
            rect = rect.ContractedBy(1f);
            var state = CurrentTimer.GetState(hour);

            GUI.DrawTexture(rect, state ? TimeOfDaySwitch.OnColor : TimeOfDaySwitch.OffColor);
            if (Mouse.IsOver(rect)) {
                Widgets.DrawBox(rect, 2);
                if (state != this.currentState && Input.GetMouseButton(0)) {
                    SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
                    CurrentTimer.SetState(hour, this.currentState);
                }
            }
        }

    }
}
