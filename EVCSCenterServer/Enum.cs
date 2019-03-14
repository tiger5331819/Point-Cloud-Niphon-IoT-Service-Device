using System;
using System.Collections.Generic;
using System.Text;

namespace EVCS
{
    public enum TypeNet
    {
        Device = 1,
        User = 20,
        CenterSever = 30
    }
    public enum Messagetype
    {
        NULL = 0,
        ID = 1,
        carinfomessage = 2,
        volumepackage = 3,
        order = 4,
        codeus = 5,
        package = 6,
        update = 7
    }
    public enum Datatype
    {
        Device = 1,
        User = 2,

        CenterSever = 10,
        Sever = 11
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
