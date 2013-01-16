﻿namespace tretton37.RunCommandOnSave.LessAutoCompiler
{
    public class ProjectFile
    {
        public string ProjectPath { get; private set; }
        public string FilePath { get; private set; }
        public uint ItemId { get; private set; }

        public ProjectFile(string projectPath, string filePath, uint itemId)
        {
            this.ProjectPath = projectPath;
            this.FilePath = filePath;
            this.ItemId = itemId;
        }      
    }
}