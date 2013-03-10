namespace Goose.Debugging
{    
    using Core.Output;

    public static class OutputServiceExtensions
    {
        public static void Debug<T>(this IOutputService outputService, string message)
        {
#if DEBUG
            var typedMessage = string.Format("{0}: {1}", typeof(T), message);
            var output = new CommandOutput("goose", typedMessage, "", CommandOutputItemType.Message);
            outputService.Handle(output);
#endif
   
        }
    }
}
