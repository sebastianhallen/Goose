namespace Goose.Core.Action
{
    using System.Threading.Tasks;

    public interface IGooseAction
    {
        string StartMessage { get; }
        Task Work { get; }
    }
}