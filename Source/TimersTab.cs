using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TimerSwitches
{
    internal class TimersTab : MainTabWindow
    {
        private const float hourWidth = 20.833334f;
        private const float hourHeight = 30f;
        private bool currentState = false;

        public TimeOfDaySwitch CurrentTimer { get; set; }

        public override Vector2 RequestedTabSize 
            => new(550, 160);

        public override void DoWindowContents(Rect fillRect)
        {
            GUI.BeginGroup(fillRect);

            var position = new Rect(0f, 0f, fillRect.width, 65f);
            GUI.BeginGroup(position);
            var rect = new Rect(0f, 0f, 191f, position.height);
            drawTimeAssignmentSelectorGrid(rect);
            var num = 0f;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerLeft;
            for (var i = 0; i < GenDate.HoursPerDay; i++)
            {
                var rect2 = new Rect(num + 4f, 0f, hourWidth, position.height + 3f);
                Widgets.Label(rect2, i.ToString());
                num += hourWidth;
            }

            GUI.EndGroup();
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            var rect5 = new Rect(0f, position.height, fillRect.width, fillRect.height - position.height);
            drawTimerRow(rect5);

            var pasteRect = new Rect(4, position.height + hourHeight + 4, CopyPasteUI.CopyPasteColumnWidth, CopyPasteUI.CopyPasteIconHeight);
            var pasteAction = ClipBoard.CanPaste ? (() => CurrentTimer.Paste()) : (Action)null;
            CopyPasteUI.DoCopyPasteButtons(pasteRect, () => CurrentTimer.Copy(), pasteAction);

            GUI.EndGroup();

            void drawTimerRow(Rect rect)
            {
                GUI.BeginGroup(rect);
                var num = 0f;

                for (var i = 0; i < GenDate.HoursPerDay; i++)
                {
                    var rect3 = new Rect(num, 0f, hourWidth, hourHeight);
                    DoTimeAssignment(rect3, i);
                    num += hourWidth;
                }
                GUI.EndGroup();
            }

            void drawTimeAssignmentSelectorGrid(Rect rect)
            {
                rect.yMax -= 2f;
                var rect2 = rect;
                rect2.xMax = rect2.center.x;
                rect2.yMax = rect2.center.y;
                drawTimeAssignmentSelectorFor(rect2, true);
                rect2.x += rect2.width;
                drawTimeAssignmentSelectorFor(rect2, false);
            }

            void drawTimeAssignmentSelectorFor(Rect rect, bool state)
            {
                rect = rect.ContractedBy(2f);
                GUI.DrawTexture(rect, state ? TimeOfDaySwitch.OnColor : TimeOfDaySwitch.OffColor);
                if (Widgets.ButtonInvisible(rect, false))
                {
                    currentState = state;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                GUI.color = Color.white;
                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlight(rect);

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.white;
                Widgets.Label(rect, state ? "On" : "Off");
                Text.Anchor = TextAnchor.UpperLeft;

                if (currentState == state)
                    Widgets.DrawBox(rect, 2);
            }
        }

        private void DoTimeAssignment(Rect rect, int hour)
        {
            rect = rect.ContractedBy(1f);
            var state = CurrentTimer.GetState(hour);

            GUI.DrawTexture(rect, state ? TimeOfDaySwitch.OnColor : TimeOfDaySwitch.OffColor);
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawBox(rect, 2);
                if (state != currentState && Input.GetMouseButton(0))
                {
                    SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
                    CurrentTimer.SetState(hour, currentState);
                }
            }
        }
    }
}
