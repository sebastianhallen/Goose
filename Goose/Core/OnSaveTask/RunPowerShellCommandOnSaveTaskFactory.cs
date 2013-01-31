namespace Goose.Core.OnSaveTask
{
    using System;
    using System.Management.Automation.Runspaces;
    using System.Text;
    using System.Threading.Tasks;
    using Output;

    public class RunPowerShellCommandOnSaveTaskFactory
		: IOnSaveActionTaskFactory
	{
		private readonly OutputService outputService;
        private readonly ICommandLogParser logParser;

        public RunPowerShellCommandOnSaveTaskFactory(OutputService outputService, ICommandLogParser logParser)
		{
			this.outputService = outputService;
            this.logParser = logParser;
		}

		public Task CreateOnSaveAction(string projectDirectory)
		{
			return new Task(() =>
			{
				var compileConfiguration = new PowerShellCommandConfiguration(projectDirectory);
			    
				var output = compileConfiguration.Configure() 
                    ? this.RunPowerShellCommand(compileConfiguration.CompileCommand)
                    : new CommandOutput("goose", "Unable to configure less compiler - make sure goose.config is present", compileConfiguration.ConfigurationFailedReason, CommandOutputItemType.Error);

                this.outputService.Handle(output);			    
				System.Threading.Thread.Sleep(1000);
			});
		}

        private CommandOutput RunPowerShellCommand(string rawCommand)
		{
			var output = new StringBuilder();
			try
			{
				System.Diagnostics.Debug.WriteLine(rawCommand);
				using (var runspace = RunspaceFactory.CreateRunspace())
				{
					var command = new Command(rawCommand, isScript: true);

					runspace.Open();
					var pipeline = runspace.CreatePipeline();
					pipeline.Commands.Add(command);
					pipeline.Commands.Add("Out-String");
					foreach (var result in pipeline.Invoke())
					{
						output.AppendFormat("{0}", result);
						System.Diagnostics.Debug.WriteLine(result);
					}

					runspace.Close();
				}
			}
			catch (Exception ex)
			{
                return new CommandOutput("goose", "Failed to run compile command", ex.ToString(), CommandOutputItemType.Error);
			}

            return this.logParser.Parse(output.ToString());
 		}
  
	}    
}
