namespace Goose.Core.Action.PowerShell
{
    public class CommandResult
    {
        public string Result { get; private set; }
        public string Output { get; private set; }
        public string Error { get; private set; }

        public CommandResult(string result, string output, string error)
        {
            this.Result = result;
            this.Output = output;
            this.Error = error;
        }
    }
}