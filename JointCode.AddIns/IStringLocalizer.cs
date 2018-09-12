using System;
using System.Collections.Generic;
using System.Text;

namespace JointCode.AddIns
{
    public interface IStringLocalizer
    {
        /// <summary>
        /// Gets a localized message.
        /// </summary>
        /// <returns>
        /// The localized message.
        /// </returns>
        /// <param name='msgid'>
        /// The message identifier. 
        /// </param>
        string GetLocalizedString(string msgid);
    }
}
