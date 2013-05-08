namespace Goose.Core.Output
{
    public interface IErrorListProviderFacade
    {
        void Add(IGooseErrorTask error);
        void Remove(IGooseErrorTask error);
    }
}