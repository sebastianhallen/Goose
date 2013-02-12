namespace Goose.Core.Output
{
    public interface IOutputService
    {
        void Handle(CommandOutput output, bool clear = true);
    }
}