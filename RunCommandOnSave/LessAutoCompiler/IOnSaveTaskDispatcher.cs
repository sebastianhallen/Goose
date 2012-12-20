namespace tretton37.RunCommandOnSave.LessAutoCompiler
{
    public interface IOnSaveTaskDispatcher
    {
        void QueueOnChangeTaskFor(string filePath);
    }
}
