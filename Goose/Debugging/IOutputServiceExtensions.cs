namespace Goose.Debugging
{
    using System;
    using Core.Output;

    public static class OutputServiceExtensions
    {
        public static void Debug<T>(this IOutputService outputService, string message, Exception exception = null)
        {
#if DEBUG
            var typedMessage = string.Format("{0}: {1}", typeof(T), message);
            var output = new CommandOutput("goose", typedMessage, "", CommandOutputItemType.Message, exception);
            outputService.Handle(output);
#endif
   
        }
    }
}
