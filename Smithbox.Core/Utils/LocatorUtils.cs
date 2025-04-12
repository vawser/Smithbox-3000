using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Utils;

public class LocatorUtils
{
    public static string GetGameDirectory(Project curProject)
    {
        switch (curProject.ProjectType)
        {
            case ProjectType.None:
                return "NONE";
            case ProjectType.DES:
                return "DES";
            case ProjectType.DS1:
                return "DS1";
            case ProjectType.DS1R:
                return "DS1R";
            case ProjectType.DS2:
                return "DS2";
            case ProjectType.DS2S:
                return "DS2S";
            case ProjectType.BB:
                return "BB";
            case ProjectType.DS3:
                return "DS3";
            case ProjectType.SDT:
                return "SDT";
            case ProjectType.ER:
                return "ER";
            case ProjectType.AC6:
                return "AC6";
            case ProjectType.AC4:
                return "AC4";
            case ProjectType.ACFA:
                return "ACFA";
            case ProjectType.ACV:
                return "ACV";
            case ProjectType.ACVD:
                return "ACVD";
            default:
                throw new Exception("Game type not set");
        }
    }
}
