using RimWorld;
using System;
using System.Linq;

namespace TimerSwitches {
    static class ClipBoard
    {
        static bool[] states;

        public static bool CanPaste => states != null;
        public static bool[] States => states ??= Enumerable.Repeat(true, GenDate.HoursPerDay).ToArray();

        public static void Copy(bool[] states)
            => Array.Copy(states, 0, States, 0, states.Length);

        public static void Paste(bool[] states)
            => Array.Copy(States, 0, states, 0, states.Length);
    }
}
