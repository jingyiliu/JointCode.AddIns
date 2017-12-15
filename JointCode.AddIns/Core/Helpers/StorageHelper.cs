
namespace JointCode.AddIns.Core.Helpers
{
    class StorageHelper
    {
        internal static Storage.Storage CreateStorage(string masterFile, string transactionFile)
        {
            return new Storage.Storage(IoHelper.OpenReadWriteShare(masterFile),
                IoHelper.OpenReadWriteShare(transactionFile));
        }
    }
}
