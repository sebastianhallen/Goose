using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Web.Script.Serialization;

namespace tretton37.RunCommandOnSave.LessAutoCompiler
{
	public class OutputService
	{
		private readonly JavaScriptSerializer serializer;
		private readonly IVsOutputWindow outputWindow;
		private IDictionary<string, IVsOutputWindowPane> panes = new Dictionary<string, IVsOutputWindowPane>();

		public OutputService(IServiceProvider serviceProvider)
		{
			this.outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
			serializer = new JavaScriptSerializer();
		}

		public void Handle(string output)
		{
			var co = serializer.Deserialize(output, typeof(CommandOutput)) as CommandOutput;

			if (co.Version == 1)
			{
				var pane = GetPane(co.Name);
				if (co.Time.HasValue)
				{
					pane.OutputString("\nInvoked @ " + co.Time.Value.ToString("s") + ":\n");
				}

				foreach (var item in co.Results)
				{
					switch (item.Type)
					{
						case CommandOutputItemType.Error:
							var outputText = String.Format("{0} #{1}: {2}", item.FileName, item.Line, item.Message);
							if (!String.IsNullOrWhiteSpace(item.Excerpt))
							{
								var excerpt = (from line in item.Excerpt.Split('\n')
											   select "  " + line.Trim());

								outputText += ":" + Environment.NewLine + String.Join("\n", excerpt);
							}
							pane.OutputTaskItemString(outputText + Environment.NewLine, VSTASKPRIORITY.TP_NORMAL, VSTASKCATEGORY.CAT_CODESENSE, "", 0, item.FileName ?? "", item.Line, item.Message);
							break;
						case CommandOutputItemType.Message:
							pane.OutputString(item.Message + Environment.NewLine);
							break;
					}
				}

				pane.FlushToTaskList();

			}

		}
		public void Handle(StringBuilder output)
		{
			Handle(output.ToString());
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

		/** EXAMPLE **/

		// {
		//   "version": 1,
		//   "name": "lessify",
		//   "time": "2013-01-14T09:19:40.786Z",
		//   "results": [
		//     {
		//       "type": "error",
		//       "message": "Syntax Error on line 6",
		//       "excerpt": " 5:     position: absolute;\n 6:     t op: 70px;\n 7:     bottom: 0;",
		//       "line": 6,
		//       "filename": "assets/datamodelviewer/datamodelviewer.less"
		//     },
		//     {
		//       "type": "error",
		//       "message": "Syntax Error on line 6",
		//       "excerpt": " 5:     position: absolute;\n 6:     t op: 70px;\n 7:     bottom: 0;",
		//       "line": 6,
		//       "filename": "assets/assets.less"
		//     }
		//   ]
		// }

		class CommandOutput
		{
			public string Name = null;
			public float Version = 0;
			public DateTime? Time = null;
			public IList<CommandOutputItem> Results = new List<CommandOutputItem>();
		}

		class CommandOutputItem
		{
			public CommandOutputItemType Type = CommandOutputItemType.None;
			public string Message = null;
			public string FileName = null;
			public uint Line = 0;
			public string Excerpt = null;
		}

		enum CommandOutputItemType
		{
			None,
			Error,
			Message
		}
	}
}
