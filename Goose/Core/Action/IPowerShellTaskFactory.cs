namespace Goose.Core.Action
{
    using System.Threading.Tasks;

    public interface IPowerShellTaskFactory
    {
        Task Create(string command);
    }
}