namespace Goose.Core.OnSaveTask
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Runspaces;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using Output;

    public class RunPowerShellCommandOnSaveTaskFactory
		: IOnSaveActionTaskFactory
	{
		private readonly OutputService outputService;
        private readonly JavaScriptSerializer serializer;

        public RunPowerShellCommandOnSaveTaskFactory(OutputService outputService)
		{
			this.outputService = outputService;
            this.serializer = new JavaScriptSerializer();
		}

		public Task CreateOnSaveAction(string projectDirectory)
		{
			return new Task(() =>
			{
				var compileConfiguration = new PowerShellCommandConfiguration(projectDirectory);
			    
				var output = compileConfiguration.Configure() 
                    ? this.RunPowerShellCommand(compileConfiguration.CompileCommand)
                    : CreateCommandOutput("Unable to configure less compiler - make sure goose.config is present", compileConfiguration.ConfigurationFailedReason, CommandOutputItemType.Error);

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
                return CreateCommandOutput("Failed to run compile command", ex.ToString(), CommandOutputItemType.Error);
			}

            return this.CreateCommandOutput(output.ToString());
		}

        private CommandOutput CreateCommandOutput(string buildLog)
        {
            try
            {
                var output = this.serializer.Deserialize<CommandOutput>(buildLog);
                return output ?? CreateCommandOutput("on save command completed", "", CommandOutputItemType.Message);
            }
            catch (Exception ex)
            {
                return CreateCommandOutput(string.Format("unable to make sense of build log: {0}", ex), buildLog, CommandOutputItemType.Error);
            }            
        }


        private static CommandOutput CreateCommandOutput(string message, string excerpt, CommandOutputItemType type)
        {
            return new CommandOutput
            {
                Name = "goose",
                Results = new List<CommandOutputItem>
				                                 {
				                                     new CommandOutputItem
				                                     {
				                                         Message = message,
				                                         Excerpt = excerpt,
				                                         FileName = "goose.plugin",
				                                         Type = type
				                                     }
				                                 },
                Version = 1,
                Time = DateTime.Now
            };
        }

	}    
}
