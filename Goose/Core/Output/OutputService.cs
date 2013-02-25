namespace Goose.Core.Output
{
	using System;
	using System.Collections.Concurrent;
    using System.Linq;
	using Debugging;
	using Microsoft.VisualStudio.Shell.Interop;

	public class OutputService
		: IOutputService
	{
		private readonly IVsOutputWindow outputWindow;
        private ConcurrentDictionary<string, IVsOutputWindowPane> panes = new ConcurrentDictionary<string, IVsOutputWindowPane>();

		public OutputService(IServiceProvider serviceProvider)
		{
			this.outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
		}

		public void Handle(CommandOutput output, bool clear = true)
		{
			if (output.Version == 1)
			{                
				var pane = GetPane(output.Name);
			    if (pane == null) return;

                if (clear) pane.Clear();
                if (output.Time.HasValue) pane.OutputString("\nInvoked @ " + output.Time.Value.ToString("s") + ":\n");				

				foreach (var item in output.Results)
				{
					if (item.Type == CommandOutputItemType.Error)
					{
						var outputText = CreateErrorOutput(item);
						pane.OutputTaskItemString(outputText + Environment.NewLine, VSTASKPRIORITY.TP_NORMAL, VSTASKCATEGORY.CAT_CODESENSE, "", 0, item.FullPath ?? item.FileName ?? "", item.Line, string.Format("{0}: {1}", item.Message, outputText));
					}
					if (item.Type == CommandOutputItemType.Message)
					{
						pane.OutputString(item.Message + Environment.NewLine);
					}
				}

				pane.FlushToTaskList();


                if (!"goose.debug".Equals(output.Name))
                {
                    this.Debug<OutputService>(string.Join(Environment.NewLine, output.Results));
                }
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
		    IVsOutputWindowPane pane;
		    if (this.panes.TryGetValue(name, out pane))
		    {
		        return pane;
		    }

		    this.panes.AddOrUpdate(name, addKey =>
		    {
		        var paneId = Guid.NewGuid();
                outputWindow.CreatePane(paneId, addKey, 1, 1);
		        outputWindow.GetPane(paneId, out pane);
		        return pane;
		    }, (updateKey, existing) => existing ?? pane);
		    
			return pane;
		}
	}
}
