namespace tretton37.RunCommandOnSave.LessAutoCompiler.OnSaveTask
{
	using System.Threading.Tasks;

	public interface IOnSaveActionTaskFactory
    {
        Task CreateOnSaveAction(string projectDirectory);
    }
}
