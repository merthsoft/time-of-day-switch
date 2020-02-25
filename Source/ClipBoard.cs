using System.Collections.Generic;
using System.Linq;

namespace TimerSwitches {
    static class ClipBoard
    {
        private static bool[] states;
        public static bool CanPaste => states != null;

        public static void Copy(IEnumerable<bool> states)
            => ClipBoard.states = states.ToArray();

        public static IEnumerable<bool> Paste()
            => states;
    }
}
