namespace Goose.Core.Output
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.Shell.Interop;

    public class OutputService
        : IOutputService
    {
		private readonly IVsOutputWindow outputWindow;
		private IDictionary<string, IVsOutputWindowPane> panes = new Dictionary<string, IVsOutputWindowPane>();
        
		public OutputService(IServiceProvider serviceProvider)
		{
			this.outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
		}

        public void Handle(CommandOutput output)
		{
            if (output.Version == 1)
			{
                var pane = GetPane(output.Name);
                pane.Clear();

                if (output.Time.HasValue)
				{
                    pane.OutputString("\nInvoked @ " + output.Time.Value.ToString("s") + ":\n");
				}

                foreach (var item in output.Results)
				{
				    if (item.Type == CommandOutputItemType.Error)
				    {
				        var outputText = CreateErrorOutput(item);
                        pane.OutputTaskItemString(outputText + Environment.NewLine, VSTASKPRIORITY.TP_NORMAL, VSTASKCATEGORY.CAT_CODESENSE, "", 0, item.FileName ?? "", item.Line, string.Format("{0}: {1}", item.Message, outputText));
				    }
				    if (item.Type == CommandOutputItemType.Message)
				    {
                        pane.OutputString(item.Message + Environment.NewLine);
				    }
				}
			    
				pane.FlushToTaskList();
			}
		}

        private static string CreateErrorOutput(CommandOutputItem item)
        {
            var outputText = String.Format("{0} #{1}: {2}", item.FileName, item.Line, item.Message);
            if (!String.IsNullOrWhiteSpace(item.Excerpt))
            {
                var excerpt = from line in item.Excerpt.Split('\n')
                              select "  " + line.Trim();

                outputText += ":" + Environment.NewLine + String.Join("\n", excerpt);
            }
            return outputText;
        }

        private IVsOutputWindowPane GetPane(string name)
		{
			if (!panes.ContainsKey(name))
			{
				var paneGuid = Guid.NewGuid();
				IVsOutputWindowPane pane;
				outputWindow.CreatePane(paneGuid, name, 1, 1);
				outputWindow.GetPane(paneGuid, out pane);
				panes[name] = pane;
			}
			return panes[name];
		}  
	}
}
