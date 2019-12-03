using System;
using System.Collections.Generic;
using System.Text;

namespace Search.IndexService.SiteMap
{
    class RobotCommand
    {
        private string command;

        private string url = string.Empty;

        private string useragent = string.Empty;

        public RobotCommand(string commandline)
        {
            int PosOfComment = commandline.IndexOf('#');
            if (PosOfComment == 0)
            {
                command = "COMMENT";
            }
            else
            {
                if (PosOfComment >= 0)
                {
                    commandline = commandline.Substring(0, PosOfComment);
                }
                if (commandline.Length > 0)
                {
                    string[] lineArray = commandline.Split(':');
                    command = lineArray[0].Trim().ToLower();
                    if (lineArray.Length > 1)
                    {
                        if (command == "user-agent")
                        {
                            useragent = lineArray[1].Trim();
                        }
                        else
                        {
                            url = lineArray[1].Trim();
                            if (lineArray.Length > 2)
                            {
                                url += ":" + lineArray[2].Trim();
                            }
                        }

                    }
                }
            }
        }

        public string Command
        {
            get { return command; }
        }

        public string Url
        {
            get { return url; }
        }

        public string UserAgent
        {
            get { return useragent; }
        }
    }
}
