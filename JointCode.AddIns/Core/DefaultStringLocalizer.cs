using System;
using System.Collections.Generic;
using System.Text;

namespace JointCode.AddIns.Core
{
    class DefaultStringLocalizer : IStringLocalizer
    {
        public string GetLocalizedString(string msgid)
        {
            return msgid;
        }
    }
}
