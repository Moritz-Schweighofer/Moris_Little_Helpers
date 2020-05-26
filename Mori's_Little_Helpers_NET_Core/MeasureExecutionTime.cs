using System;
using System.Diagnostics;

namespace Schweigm_NETCore_Helpers
{
    public static class MeasureExecutionTime
    {
        public static long  MeasureSinge(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            var elapsedTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();

            return elapsedTime;
        }
    }
}
