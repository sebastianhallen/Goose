namespace Goose.Core.Action.PowerShell.Host
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation.Host;

    internal class GoosePSHost
        : PSHost
    {
        private readonly CultureInfo culture;
        private readonly CultureInfo uiCulture;
        private readonly Guid instanceId;

        public IList<string> Output { get; private set; }
        public IList<string> Error { get; private set; }
        public IList<string> Other { get; private set; }

        private PSHostUserInterface ui;

        public GoosePSHost()
        {
            this.culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            this.uiCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            this.instanceId = Guid.NewGuid();
            this.Output = new List<string>();
            this.Error = new List<string>();
            this.Other = new List<string>();
            
            this.ui = new GoosePSHostUserInterface(this.Output, this.Other, this.Error, this.Other, this.Other);
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
        }

        public override void NotifyEndApplication()
        {
        }

        public override string Name
        {
            get { return "GoosePSHost"; }
        }

        public override Version Version
        {
            get
            {
                return new Version(3, 0);
            }
        }

        public override Guid InstanceId
        {
            get { return this.instanceId; }
        }

        public override PSHostUserInterface UI
        {
            get { return this.ui; }
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
}