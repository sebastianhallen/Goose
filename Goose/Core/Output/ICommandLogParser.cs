namespace Goose.Core.Output
{
    public interface ICommandLogParser
    {
        CommandOutput Parse(string buildLog);
    }
}
