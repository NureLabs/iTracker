using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iTracker
{
    public class ActiveWindowClass
    {
        public string BrowserTab;
        public string ProgramName;
        public string ProcessName;
        public DateTime Process_StartTime;
        public DateTime Process_EndTime;
        public TimeSpan Process_ExcecutionTime;
        public string ProcessPath;
        public IntPtr ProcessHandle;

        public string GetActiveBrowser_Tab()
        {
            return BrowserTab;
        }
        public string GetActiveProcess_Name()
        {
            return ProcessName;
        }
        public string GetActiveProgram_Name()
        {
            return ProgramName;
        }
        public DateTime GetActiveProcess_StartTime()
        {
            return Process_StartTime;
        }
        public DateTime GetActiveProcess_EndTime()
        {
            return Process_EndTime;
        }
        public TimeSpan GetActiveProcess_ExcecutionTime()
        {
            return Process_ExcecutionTime;
        }
        public string GetActiveProcess_Path()
        {
            return ProcessPath;
        }
        public IntPtr GetActiveProcess_Handle()
        {
            return ProcessHandle;
        }


        public void SetActiveBrowser_Tab(string browserTab)
        {
            BrowserTab = browserTab;
        }
        public void SetActiveProcess_Name(string processName)
        {
            ProcessName = processName;
        }
        public void SetActiveProgram_Name(string programName)
        {
            ProgramName = programName;
        }
        public void SetActiveProcess_StartTime(DateTime processStartTime)
        {
            Process_StartTime = processStartTime;
        }
        public void SetActiveProcess_EndTime(DateTime processEndTime)
        {
            Process_EndTime = processEndTime;
        }
        public void SetActiveProcess_ExcecutionTime(TimeSpan processExecutionTime)
        {
            Process_ExcecutionTime = processExecutionTime;
        }
        public void SetActiveProcess_Path(string processPath)
        {
            ProcessPath = processPath;
        }
        public void SetActiveProcess_Handle(IntPtr processHandle)
        {
            ProcessHandle = processHandle;
        }
    }
}
