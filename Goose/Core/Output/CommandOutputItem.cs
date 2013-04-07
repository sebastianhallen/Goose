namespace Goose.Core.Output
{
    using System;

    public class CommandOutputItem
	{
		public CommandOutputItemType Type = CommandOutputItemType.None;
		public string Message = null;
		public string FileName = null;
		public string FullPath = null;
		public uint Line = 0;
		public string Excerpt = null;
	    public Exception Exception = null;

        public override string ToString()
        {
            return string.Format("{0}: {1} - {2}", this.Type, this.Message, this.Excerpt);
        }
	}
}