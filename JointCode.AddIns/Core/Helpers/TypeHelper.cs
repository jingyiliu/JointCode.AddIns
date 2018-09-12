using JointCode.Common.Extensions;
using System;

namespace JointCode.AddIns.Core.Helpers
{
    static class TypeHelper
    {
        internal static void ThrowIfNotMarshallable(Type type)
        {
            var attrib = type.GetCustomAttribute<SerializableAttribute>(false);
            if (attrib != null)
                return;
            if (!typeof(MarshalByRefObject).IsAssignableFrom(type))
                throw new ArgumentException(string.Format("The type [{0}] must be a marshallable type, as it might be passed across AppDomains.", type.ToFullTypeName()));
        }
    }
}
