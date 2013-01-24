namespace Goose.Core.Solution
{
    using System;

    public class MonitoredFile<T>
        : MonitoredFile
    {        
        public readonly T FileData;
     
        
        public MonitoredFile(uint monitorCookie, string filePath, T fileData)
            : base(monitorCookie, filePath)
        {            
            this.FileData = fileData;
        }       
    }

    public class MonitoredFile
    {
        public readonly uint MonitorCookie;
        protected readonly string filePath;

        public MonitoredFile(uint monitorCookie, string filePath)
        {
            this.MonitorCookie = monitorCookie;
            this.filePath = filePath;
        }

        public bool PathMatches(string candidate)
        {
            return String.Equals(candidate, this.filePath);
        }
    }
}