namespace Goose.Core.Action.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Runspaces;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Goose.Core.Output;

    public interface IShellCommandRunner
    {
        string RunCommand(ShellCommand command);
    }

    public class PowerShellPSCommandRunner
        : IShellCommandRunner
    {
        public string RunCommand(ShellCommand command)
        {
            var setLocation = new Command("set-location");
            setLocation.Parameters.Add("path", command.WorkingDirectory);

            var cmd = new PSCommand()
                .AddCommand(setLocation)
                .AddCommand(command.Command)
                .AddCommand("out-string");

            var shell = PowerShell.Create();
            shell.Commands = cmd;

            var output = new StringBuilder();
            
            foreach (var result in shell.Invoke())
            {
                output.AppendFormat("{0}", result);
                System.Diagnostics.Debug.WriteLine(result);
            }

            return output.ToString();
        }
    }

    public class PowerShellRunspaceCommandRunner
        : IShellCommandRunner
    {       
        public string RunCommand(ShellCommand command)
        {
            var output = new StringBuilder();
            var errors = new List<Object>();
            
            using (var runspace = RunspaceFactory.CreateRunspace(/*new GoosePSHost()*/))
            {
                var setWorkingDirectory = new Command("set-location");
                setWorkingDirectory.Parameters.Add("path", command.WorkingDirectory);

                var payloadCommand = new Command(command.Command, isScript: true);
                var redirectOutput = new Command("out-string");

                runspace.Open();
                var pipeline = runspace.CreatePipeline();
                pipeline.Commands.Add(setWorkingDirectory);
                pipeline.Commands.Add(payloadCommand);
                pipeline.Commands.Add(redirectOutput);

                foreach (var result in pipeline.Invoke())
                {
                    errors.AddRange(pipeline.Error.ReadToEnd());
                    output.AppendFormat("{0}", result);
                    System.Diagnostics.Debug.WriteLine(result);
                }

                runspace.Close();
            }
            
            return output.ToString();
        }
    }

    public class PowerShellTaskFactory
        : IPowerShellTaskFactory
    {
        private readonly IOutputService outputService;
        private readonly ICommandLogParser logParser;
        private readonly IShellCommandRunner commandRunner;

        public PowerShellTaskFactory(IOutputService outputService, ICommandLogParser logParser)
            : this(outputService, logParser, new PowerShellRunspaceCommandRunner())
        {
        }

        public PowerShellTaskFactory(IOutputService outputService, ICommandLogParser logParser, IShellCommandRunner commandRunner)
        {
            this.outputService = outputService;
            this.logParser = logParser;
            this.commandRunner = commandRunner;
        }

        public Task Create(ShellCommand command)
        {
            return new Task(() =>
                {
                    CommandOutput output;
                    try
                    {
                        var rawOutput = this.commandRunner.RunCommand(command);
                        output = this.logParser.Parse(rawOutput);
                    }
                    catch (Exception ex)
                    {
                        output = new CommandOutput("goose", "Failed to run command", ex.ToString(), CommandOutputItemType.Error);
                    }
                                        
                    this.outputService.Handle(output);    
                });
        }        
    }

    internal class GoosePSHost
        : PSHost
    {
        private CultureInfo culture;
        private CultureInfo uiCulture;
        private Guid instanceId;

        public GoosePSHost()
        {
            this.culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            this.uiCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            this.instanceId = Guid.NewGuid();
        }

        public override void SetShouldExit(int exitCode)
        {
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void NotifyBeginApplication()
        {
            return;
        }

        public override void NotifyEndApplication()
        {
            return;
        }

        public override string Name
        {
            get { return "GoosePSHost"; }
        }

        public override Version Version
        {
            get { return new Version(1, 0); }
        }

        public override Guid InstanceId
        {
            get { return this.instanceId; }
        }

        public override PSHostUserInterface UI
        {
            get { return new GoosePSHostUserInterface(); }
        }

        public override CultureInfo CurrentCulture
        {
            get { return this.culture; }
        }

        public override CultureInfo CurrentUICulture
        {
            get { return this.uiCulture; }
        }
    }

    internal class GoosePSHostUserInterface
        : PSHostUserInterface
    {
        public override string ReadLine()
        {
            throw new Exception("Cannot use run commands that require user input");
        }

        public override SecureString ReadLineAsSecureString()
        {
            throw new Exception("Cannot use run commands that require user input");
        }

        public override void Write(string value)
        {
            throw new NotImplementedException();
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            this.Write(value);
        }

        public override void WriteLine(string value)
        {
            this.Write(string.Format("{0}{1}", value, Environment.NewLine));
        }

        public override void WriteErrorLine(string value)
        {
            throw new NotImplementedException();
        }

        public override void WriteDebugLine(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            throw new NotImplementedException();
        }

        public override void WriteVerboseLine(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteWarningLine(string message)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new Exception("Cannot use run commands that require user input");
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new Exception("Cannot use run commands that require user input");
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName,
                                                         PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new Exception("Cannot use run commands that require user input");
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new Exception("Cannot use run commands that require user input");
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return new GoosePSHostRawUserInterface(); }
        }
    }

    internal class GoosePSHostRawUserInterface
        : PSHostRawUserInterface
    {
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new Exception("Cannot use run commands that require user input");
        }

        public override void FlushInputBuffer()
        {
            throw new Exception("Cannot use run commands that require user input");
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            return null;
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            
        }

        public override ConsoleColor ForegroundColor
        {
            get { return ConsoleColor.White; }
            set { }
        }
        public override ConsoleColor BackgroundColor
        {
            get { return ConsoleColor.Black; }
            set { }
        }
        public override Coordinates CursorPosition { get; set; }
        public override Coordinates WindowPosition { get; set; }
        public override int CursorSize { get; set; }
        public override Size BufferSize { get; set; }
        public override Size WindowSize { get; set; }

        public override Size MaxWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override bool KeyAvailable
        {
            get { throw new NotImplementedException(); }
        }

        public override string WindowTitle { get; set; }
    }
}
