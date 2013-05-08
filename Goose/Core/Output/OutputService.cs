namespace Goose.Core.Output
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.VisualStudio.Shell.Interop;
    
    public class OutputService
		: IOutputService
	{
		private readonly IVsOutputWindow outputWindow;
        private readonly ConcurrentDictionary<string, Guid> panes = new ConcurrentDictionary<string, Guid>();
        
        public OutputService(IServiceProvider serviceProvider)
		{
			this.outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
        }

		public void Handle(CommandOutput output)
		{
			if (output.Version == 1)
			{                
                this.HandleMessages(output.Name, output.Time, output.Results);			  
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
	}
}
