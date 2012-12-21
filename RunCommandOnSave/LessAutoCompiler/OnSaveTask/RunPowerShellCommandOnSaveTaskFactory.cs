namespace tretton37.RunCommandOnSave.LessAutoCompiler.OnSaveTask
{
  using System;
  using System.IO;
  using System.Management.Automation.Runspaces;
  using System.Text;
  using System.Threading.Tasks;
  using System.Xml;
  using tretton37.LessToCssAutoCompiler;

	public class RunPowerShellCommandOnSaveTaskFactory
        : IOnSaveActionTaskFactory
  {

    public Task CreateOnSaveAction(string projectDirectory)
    {
      return new Task(() =>
      {
        var compileConfiguration = new LessCompileConfiguration(projectDirectory);
        var log = new StringBuilder();

        if (compileConfiguration.Configure())
        {
          this.RunPowerShellCommand(compileConfiguration.CompileCommand);
          //log.AppendLine(buildLog);
        }
        else
        {
          log.AppendLine("Unable to configure less compiler:");
          log.AppendLine(compileConfiguration.ConfigurationFailedReason);
        }

        WriteBuildLog(compileConfiguration.BuildDirectory, log.ToString());

        System.Threading.Thread.Sleep(1000);
      });
    }

    private static void WriteBuildLog(string buildDirectory, string log)
    {
      return;
      try
      {
        var logFile = Path.Combine(buildDirectory, "less-build.log");
        File.WriteAllText(logFile, log);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine("unable to write to build log: {0}", ex);
      }
    }

    private void RunPowerShellCommand(string rawCommand)
    {
      var log = new StringBuilder();

      try
      {
        System.Diagnostics.Debug.WriteLine(rawCommand);
        log.AppendFormat("$>{0}{1}", rawCommand, Environment.NewLine);
        using (var runspace = RunspaceFactory.CreateRunspace())
        {
          var command = new Command(rawCommand, isScript: true);

          runspace.Open();
          var pipeline = runspace.CreatePipeline();
          pipeline.Commands.Add(command);
          //pipeline.Commands.Add("Out-String");
          foreach (var result in pipeline.Invoke())
          {
            log.AppendFormat("{0}", result);
            System.Diagnostics.Debug.WriteLine(result);
          }

          runspace.Close();
        }
      }
      catch (Exception ex)
      {
        log.AppendFormat("Failed to run compile command: {0}", ex);
        System.Diagnostics.Debug.WriteLine("Failed to run compile command: {0}", ex);
      }

      //   return log.ToString();
    }

    private class LessCompileConfiguration
    {
      private readonly string projectDirectory;
      private string compileCommandField;
      private string ConfigPath
      {
        get { return Path.Combine(projectDirectory, Constants.ConfigFileName); }
      }

      public string ConfigurationFailedReason { get; private set; }
      public string BuildDirectory { get; private set; }
      public string CompileCommand
      {
        get
        {
          return string.Format(@"cd ""{0}"" ; {1}", this.BuildDirectory, this.compileCommandField);
        }
        private set { this.compileCommandField = value; }
      }

      public LessCompileConfiguration(string projectDirectory)
      {
        this.projectDirectory = projectDirectory;
      }

      public bool Configure()
      {
        if (!File.Exists(this.ConfigPath))
        {
          this.ConfigurationFailedReason = string.Format("Unable to find config file @ {0}", this.ConfigPath);
          return false;
        }

        try
        {
          var xml = new XmlDocument();
          xml.Load(this.ConfigPath);
          var buildDirectory = xml.SelectSingleNode("compile-less/build-directory").InnerText;
          var compileLessCommand = xml.SelectSingleNode("compile-less/compile-command").InnerText;

          this.BuildDirectory = Path.Combine(projectDirectory, buildDirectory);
          this.CompileCommand = compileLessCommand;

        }
        catch (Exception ex)
        {
          this.ConfigurationFailedReason = string.Format("could not determine compile command from config: {0}", ex);
          return false;
        }

        return true;
      }
    }
  }
}
