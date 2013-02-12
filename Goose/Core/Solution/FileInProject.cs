namespace Goose.Core.Solution
{
    public class FileInProject
    {
        public string ProjectPath { get; private set; }
        public string FilePath { get; private set; }
        public uint ItemId { get; private set; }

        public FileInProject(string projectPath, string filePath, uint itemId)
        {
            this.ProjectPath = projectPath;
            this.FilePath = filePath;
            this.ItemId = itemId;
        }

        public override string ToString()
        {
            return this.FilePath;
        }
    }
}