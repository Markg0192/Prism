using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism
{
    public class CpuCounter
    {
        private PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public CpuCounter()
        {
            float firstValue = cpuCounter.NextValue();
        }

        private List<int> last4 = new List<int>();

        public int CheckCPU()
        {
            cpuCounter.NextValue();

            System.Threading.Thread.Sleep(10);
            last4.Add((int)cpuCounter.NextValue());

            while (last4.Count < 4)
            {
                System.Threading.Thread.Sleep(10);
                last4.Add((int)cpuCounter.NextValue());
            }

            if (last4.Count > 4) last4.RemoveAt(0);
            return last4.Sum() / last4.Count;
        }
    }
}