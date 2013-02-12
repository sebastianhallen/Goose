namespace Goose.Core.Solution.EventHandling
{
    using System;

    public class MonitoredFile<T>
        : MonitoredFile
    {        
        public readonly T FileData;
     
        
        public MonitoredFile(uint monitorCookie, string projectPath, string filePath, T fileData)
            : base(monitorCookie, projectPath, filePath)
        {            
            this.FileData = fileData;
        }       
    }

    public class MonitoredFile
    {
        public uint MonitorCookie { get; private set; }
        public string ProjectPath { get; private set; }
        public string FilePath { get; private set; }        

        public MonitoredFile(uint monitorCookie, string projectPath, string filePath)
        {
            this.MonitorCookie = monitorCookie;
            this.ProjectPath = projectPath;
            this.FilePath = filePath;
        }
       
        public bool PathMatches(string candidate)
        {
            return String.Equals(candidate, this.FilePath);
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}