namespace Goose.Core.Action.PowerShell.Host
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Security;

    internal class GoosePSHostUserInterface
        : PSHostUserInterface
    {
        private readonly PSHostRawUserInterface rawUi;
        public IList<string> Output { get; private set; }
        public IList<string> Warning { get; private set; }
        public IList<string> Error { get; private set; }
        public IList<string> Verbose { get; private set; }
        public IList<string> Debug { get; private set; }

        public GoosePSHostUserInterface(IList<string> output, IList<string> warning, IList<string> error, IList<string> verbose, IList<string> debug)
        {
            this.Output = output;
            this.Warning = warning;
            this.Error = error;
            this.Verbose = verbose;
            this.Debug = debug;
            
            this.rawUi = new GoosePSHostRawUserInterface();
        }

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
            this.Output.Add(value.Trim());
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
            this.Error.Add(value.Trim());            
        }

        public override void WriteDebugLine(string message)
        {
            this.Debug.Add(message.Trim());
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            throw new NotImplementedException();
        }

        public override void WriteVerboseLine(string message)
        {
            this.Verbose.Add(message.Trim());
        }

        public override void WriteWarningLine(string message)
        {
            this.Warning.Add(message.Trim());
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
            get { return this.rawUi; }
        }
    }
}