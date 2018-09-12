using System;
using System.Collections.Generic;
using System.Text;

namespace JointCode.AddIns.RootAddin
{
    class AddinActivator : IAddinActivator
    {
        public void Start(IAddinContext context)
        {
            Console.WriteLine("I am started");
        }

        public void Stop(IAddinContext context)
        {
            Console.WriteLine("I am stopped");
        }
    }
}
