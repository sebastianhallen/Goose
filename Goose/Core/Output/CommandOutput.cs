namespace Goose.Core.Output
{
    using System;
    using System.Collections.Generic;

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
    //       "type": "message",
    //       "message": "This is now done."
    //     }
    //   ]
    // }      

    public class CommandOutput
    {
        public string Name = null;
        public float Version = 0;
        public DateTime? Time = null;
        public IList<CommandOutputItem> Results = new List<CommandOutputItem>();
    }
}