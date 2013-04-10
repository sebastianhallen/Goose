namespace Goose.Core.Output
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using Goose.Core.Solution;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

    public class GooseErrorListProvider
        : ErrorListProvider
    {        
        private readonly ErrorTaskFactory errorTaskFactory;
        private readonly TimeSpan clearInterval = TimeSpan.FromSeconds(15); // should probably be some kind of config setting
        private DateTime lastCleared;

        public GooseErrorListProvider(IServiceProvider provider, ISolutionFilesService solutionFilesService)
            : base(provider)
        {            
            this.lastCleared = DateTime.MinValue;
            this.errorTaskFactory = new ErrorTaskFactory(solutionFilesService, this);
            this.ProviderGuid = Guid.Parse("823860A6-2143-4262-93FE-70FB764F035A");
            this.ProviderName = "Goose Error Provider";
        }

        public void ShowErrors(IEnumerable<CommandOutputItem> errors)
        {            
            this.ClearExisting();
            foreach (var error in errors)
            {
                var taskError = this.errorTaskFactory.Create(
                    error.Message, 
                    error.FullPath ?? error.FileName ?? "", 
                    (int)error.Line, 
                    0);
                this.Tasks.Add(taskError);                                    
            }
        }

        private void ClearExisting()
        {
            if (this.ShouldClear())
            {
                var errors = this.Tasks.OfType<ErrorTask>().ToArray();
                foreach (var error in errors)
                {
                    this.Tasks.Remove(error);
                }

                this.lastCleared = DateTime.UtcNow;
            }                        
        }

        private bool ShouldClear()
        {
            return DateTime.UtcNow - this.lastCleared > this.clearInterval;
        }
    }

    public class ErrorTaskFactory
    {
        private readonly ISolutionFilesService solutionFiles;
        private readonly ErrorListProvider errorListProvider;

        public ErrorTaskFactory(ISolutionFilesService solutionFiles, ErrorListProvider errorListProvider)
        {
            this.solutionFiles = solutionFiles;
            this.errorListProvider = errorListProvider;
        }

        public ErrorTask Create(string message, string file, int line, int column)
        {            
            var error = new ErrorTask
            {
                CanDelete = true,
                Column = column,
                Line = line,
                Document = file,
                HierarchyItem = this.FindHierarchyItem(file),
                Text = message,
            };

            error.Navigate += (s, a) =>
                {
                    var task = (ErrorTask) s;
                    this.errorListProvider.Navigate(task, new Guid(EnvDTE.Constants.vsViewKindCode));
                };
            return error;
        }

        private IVsHierarchy FindHierarchyItem(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;

            var project = this.solutionFiles.Projects
                                .FirstOrDefault(prj => prj.Files
                                    .Any(file => file.FilePath.Equals(filePath)));

            return project == null
                       ? null
                       : project.Hierarchy;
        }
    }

    public class OutputService
		: IOutputService
	{
		private readonly IVsOutputWindow outputWindow;
        private readonly ConcurrentDictionary<string, Guid> panes = new ConcurrentDictionary<string, Guid>();
        private GooseErrorListProvider errorTaskProvider;

        public OutputService(IServiceProvider serviceProvider, ISolutionFilesService solutionFilesService)
		{
			this.outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            this.errorTaskProvider = new GooseErrorListProvider(serviceProvider, solutionFilesService);
		}

		public void Handle(CommandOutput output)
		{
			if (output.Version == 1)
			{                
                //this.HandleErrors(output.Name, output.Results.Where(result => CommandOutputItemType.Error.Equals(result.Type)));    
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
		    try
		    {
                this.outputWindow.GetPane(ref paneId, out pane);
		    }
		    catch (Exception)
		    {
		        pane = null;
		    }
            
            return pane;
		}

	    private enum OutputWindowType
	    {
            Message,
            Error
	    }
	}
}
