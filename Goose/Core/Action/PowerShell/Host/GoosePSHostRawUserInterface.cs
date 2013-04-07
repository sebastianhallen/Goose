namespace Goose.Core.Action.PowerShell.Host
{
    using System;
    using System.Management.Automation.Host;

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
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override ConsoleColor ForegroundColor
        {
            get { return ConsoleColor.White; }
            set { throw new NotImplementedException(); }
        }
        public override ConsoleColor BackgroundColor
        {
            get { return ConsoleColor.Black; }
            set { throw new NotImplementedException(); }
        }
        public override Coordinates CursorPosition
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override Coordinates WindowPosition
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override int CursorSize
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public override Size BufferSize
        {
            get { return new Size(1024, 1); }
            set { throw new NotImplementedException(); }
        }
        public override Size WindowSize
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

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

        public override string WindowTitle
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}