namespace JointCode.AddIns.Core.Storage
{
    public sealed class StorageStatistics
    {
        readonly Storage _storage;

        public StorageStatistics(Storage storage)
        {
            _storage = storage;
        }

        public long StorageSize
        {
            get { return _storage.MasterStream.Length; }
        }
        public long BytesRead
        {
            get { return _storage.MasterStream.BytesRead; }
        }
        public long BytesWritten
        {
            get { return _storage.MasterStream.BytesWritten; }
        }
        public int TotalStreamCount
        {
            get { return _storage.StreamTable.Count; }
        }
        public int OpenedStreamsCount
        {
            get { return _storage.OpenedStreamsCount; }
        }
        public long TransactionsCommited { get; internal set; }
        public long TransactionsRolledBack { get; internal set; }
    }
}
