namespace Goose.Core.Action
{
    public class CommandEvironmentVariables
    {
        public string FilePath { get; private set; }

        public CommandEvironmentVariables(string filePath)
        {
            this.FilePath = filePath;
        }
    }
}