using System;
using DrOpen.DrCommon.DrData;

namespace DrOpen.DrTask.DrtPlugin
{
    /// <summary>
    /// Class for transfering event information in form of DDNode
    /// </summary>
    public class DrtEventArgs : EventArgs
    {
        public DDNode EventData { get; set; }

        public DrtEventArgs() { }

        public DrtEventArgs(DDNode EventData)
        {
            this.EventData = EventData;
        }
    }

    /// <summary>
    /// Extension of DrtEventArgs with flag that indicates if plugin doesn't have to be executed
    /// </summary>
    public class DrtEventCancelArgs : DrtEventArgs
    {
        public DrtEventCancelArgs()
        {
            this.Cancel = false;
        }

        public bool Cancel { get; set; }
    }
}
