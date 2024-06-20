using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTracker
{
    public class ProcessStatistic
    {
        private string ProgramName;
        private TimeSpan Process_ExcecutionTime;
        private double PersentOfProcessUsage;

        public string GetActiveProgram_Name()
        {
            return ProgramName;
        }
        public TimeSpan GetActiveProcess_ExcecutionTime()
        {
            return Process_ExcecutionTime;
        }
        public double GetActiveProcess_PersentOfUsage()
        {
            return PersentOfProcessUsage;
        }

        public void SetActiveProgram_Name(string programName)
        {
            ProgramName = programName;
        }
        public void SetActiveProcess_ExcecutionTime(TimeSpan processExecutionTime)
        {
            Process_ExcecutionTime = processExecutionTime;
        }
        public void SetActiveProcess_PersentOfUsage(double PersentOfUsage)
        {
            PersentOfProcessUsage = PersentOfUsage;
        }
    }
}
