using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using JointCode.AddIns.RootAddin;

namespace JointCode.AddIns.SchemaChildAddin
{
    public class SchemaChild1Command : IRootCommand
    {
        public void Run()
        {
            MessageBox.Show("SchemaChild1Command");
        }
    }
}
