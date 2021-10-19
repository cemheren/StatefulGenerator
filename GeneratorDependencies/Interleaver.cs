using System;

namespace GeneratorDependencies
{
    public class Interleaver
    {
        public static void Pause() { }

        public static void Wait(TimeSpan timeToWait)
        {

        }
    }

    public class InterleaverState
    {
        public int ExecutionState;

        public DateTime CurrentStateStartTime;
    }
}
