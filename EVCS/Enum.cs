using System;
using System.Collections.Generic;
using System.Text;

namespace EVCS
{
    public enum Messagetype
    {
        NULL = 0,
        ID = 1,
        carinfomessage = 2,
        volumepackage = 3,
        order = 4,
        codeus = 5,
        package = 6,
        update=7
    }
    public enum Codemode
    {
        release = -1,
        stop = 0,
        play = 1,
        monitor = 2,
        sendvolume = 3,
        stopsendvolume = 4
    }
}
