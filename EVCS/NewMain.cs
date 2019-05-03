using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EVCS
{
    class NewMain
    {
        public NewMain()
        {
            Nform = this;
            cloud = new Special();
        }

        public Special cloud;
        public static NewMain Nform;

    }
}
