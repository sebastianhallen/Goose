namespace Goose.Core.Output
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

    public class GooseErrorListProvider
        : ErrorListProvider
    {
        public GooseErrorListProvider(IServiceProvider provider)
            : base(provider)
        {
        }

        public void ShowErrors(IEnumerable<CommandOutputItem> errors)
        {
            //this.ClearExisting();
            foreach (var error in errors)
            {
                this.Tasks.Add(
                    new GooseErrorTask(
                        error.Message,
                        error.FullPath ?? error.FileName ?? "", 
                        (int) error.Line)
                );
            }
        }

        private void ClearExisting()
        {
            var gooseErrors = this.Tasks.OfType<GooseErrorTask>().ToArray();

            foreach (var gooseError in gooseErrors)
            {
                this.Tasks.Remove(gooseError);
            }
        }

        private class GooseErrorTask
            : ErrorTask
        {
            public GooseErrorTask(string message, string file, int line)
            {
                this.ErrorCategory = TaskErrorCategory.Error;
                this.Text = message;
                this.Document = file;
                this.Line = line;
            }

        }
    }

    public class OutputService
		: IOutputService
	{
		private readonly IVsOutputWindow outputWindow;
        private readonly ConcurrentDictionary<string, Guid> panes = new ConcurrentDictionary<string, Guid>();
        private GooseErrorListProvider errorTaskProvider;

        public OutputService(IServiceProvider serviceProvider)
		{
			this.outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
		    this.errorTaskProvider = new GooseErrorListProvider(serviceProvider);
		}

		public void Handle(CommandOutput output)
		{
			if (output.Version == 1)
			{                
                this.HandleErrors(output.Name, output.Results.Where(result => CommandOutputItemType.Error.Equals(result.Type)));    
                this.HandleMessages(output.Name, output.Time, output.Results.Where(result => CommandOutputItemType.Message.Equals(result.Type)));			  
			}
		}

        public void RemovePanels()
        {
            var panelKeys = this.panes.Keys.ToArray();
            foreach (var panelKey in panelKeys)
            {
                Guid paneId;
                if (this.panes.TryRemove(panelKey, out paneId))
                {
                    this.outputWindow.DeletePane(paneId);
                }
            }
        }

	    private void HandleMessages(string panel, DateTime? invokationTime, IEnumerable<CommandOutputItem> messages)
	    {
            var messagePane = this.GetOrAddPane(panel);
            if (messagePane == null) return;

	        if (invokationTime.HasValue)
	        {
	            messagePane.OutputString(string.Format(@"{0}Invoked @{1}:{0}", Environment.NewLine, invokationTime.Value.ToString("s")));
	        }
	        foreach (var message in messages)
	        {                
                messagePane.OutputStringThreadSafe(message.Message + Environment.NewLine);
	        }
	    }

	    private void HandleErrors(string panel, IEnumerable<CommandOutputItem> errors)
	    {
            this.errorTaskProvider.ShowErrors(errors);
            return;
	        

            var errorPane = this.GetOrAddPane(panel);
	        if (errorPane == null) return;

            var currentErrors = errors.ToArray();
            if (currentErrors.Any())
            {
                errorPane.Clear();
            }

            foreach (var error in currentErrors)
	        {
	            var outputText = CreateErrorOutput(error);                
	            errorPane.OutputTaskItemString(
	                outputText + Environment.NewLine, 
	                VSTASKPRIORITY.TP_NORMAL, 
	                VSTASKCATEGORY.CAT_CODESENSE, 
	                "", 
	                0,
	                error.FullPath ?? error.FileName ?? "",
	                error.Line, 
	                outputText);
			
	        }
	        errorPane.FlushToTaskList();
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

		private IVsOutputWindowPane GetOrAddPane(string name)
		{            		    
            var paneKey = name;
		    var paneId = this.panes.GetOrAdd(paneKey, paneName =>
		    {
		        var paneid = Guid.NewGuid();
                var paneVisibility = 1;
                this.outputWindow.CreatePane(ref paneid, name, paneVisibility, 1);

		        return paneid;
		    });

            IVsOutputWindowPane pane;
            this.outputWindow.GetPane(ref paneId, out pane);
            return pane;
		}

	    private enum OutputWindowType
	    {
            Message,
            Error
	    }
	}
}
