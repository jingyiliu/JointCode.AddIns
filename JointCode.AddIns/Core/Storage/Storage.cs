//Copyright (c) 2012 Tomaz Koritnik

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
//files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
//modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
//Software is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using JointCode.AddIns.Core.Storage.Transactions;

namespace JointCode.AddIns.Core.Storage
{
    /// 以 StorageStream 为单位实现单文件二进制数据的增删改查功能。
    /// <summary>
    /// Storage
    /// </summary>
    public class Storage
    {
        #region Fields
        // System streams
        StorageStream _storageMetadataStream;
        StorageStreamMetadata _streamTableStreamMetadata;
        StorageStream _streamTableStream;
        internal StorageStream FreeSpaceStream { get; set; }

        // Transaction support
        TransactionStream _transactionStream;
        int _transactionLevel = 0;
        List<StorageStream> _streamsChangedDuringTransaction = new List<StorageStream>();
        List<Guid> _streamsCreatedDuringTransaction = new List<Guid>();

        // Stream table
        StreamTable _streamTable;

        // <源码修改>
        // List of opened streams
        // Dictionary<Guid, WeakReference<StorageStream>> openedStreams = new Dictionary<Guid, WeakReference<StorageStream>>();
        Dictionary<Guid, WeakReference> _openedStreams = new Dictionary<Guid, WeakReference>();
        // </源码修改>
        #endregion

        #region Properties
        /// <summary>
        /// Storage metadata
        /// </summary>
        public StorageMetadata StorageMetadata { get; set; }

        bool isClosed = false;
        /// <summary>
        /// Flag indicated whether storage is closed
        /// </summary>
        public bool IsClosed
        {
            get { return isClosed; }
        }
        /// <summary>
        /// Returns true if storage is in transaction
        /// </summary>
        public bool InTransaction
        {
            get { return _transactionLevel > 0; }
        }

        public StorageStatistics Statistics { get; set; }

        /// <summary>
        /// Master stream where all of the storage data is stored
        /// </summary>
        internal MasterStream MasterStream { get; set; }

        internal StreamTable StreamTable
        {
            get { return _streamTable; }
        }
        internal int OpenedStreamsCount
        {
            get { return _openedStreams.Count; }
        }

        internal const uint BlockSize = 512;
        #endregion

        #region Construction
        /// <summary>
        /// Constructor
        /// </summary>
        public Storage(Stream stream, Stream transactionLogStream)
        {
            Statistics = new StorageStatistics(this);

            if (stream.Length == 0)
                CreateStorage(stream);

            _transactionStream = transactionLogStream != null ? new TransactionStream(stream, transactionLogStream, BlockSize) : null;
            MasterStream = new MasterStream(_transactionStream ?? stream, false);

            OpenStorage();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public Storage(string filename, string transactionLogFilename)
            : this(File.Open(filename, FileMode.OpenOrCreate),
                   transactionLogFilename != null ? File.Open(transactionLogFilename, FileMode.OpenOrCreate) : null)
        { }
        #endregion

        #region Public methods

        /// <summary>
        /// Creates a stream
        /// </summary>
        /// <param name="streamId">Stream Id</param>
        /// <param name="tag">The tag is a place to store some custom information, i.e. type of data in the stream.</param>
        /// <returns></returns>
        /// <exception cref="InvalidStreamIdException"></exception>
        /// <exception cref="StreamExistsException"></exception>
        public StorageStream CreateStream(Guid streamId, int tag = 0)
        {
            CheckClosed();

            if (SystemStreamId.IsSystemStreamId(streamId))
                throw new InvalidStreamIdException();
            if (ContainsStream(streamId))
                throw new StreamExistsException();

            StartTransaction();
            try
            {
                _streamTable.Add(streamId, tag);
                CommitTransaction();
                _streamsCreatedDuringTransaction.Add(streamId);
            }
            catch
            {
                RollbackTransaction();
                throw;
            }

            return OpenStream(streamId);
        }
        /// <summary>
        /// Opens a stream
        /// </summary>
        /// <param name="streamId">Stream Id</param>
        public StorageStream OpenStream(Guid streamId)
        {
            CheckClosed();

            if (SystemStreamId.IsSystemStreamId(streamId))
                throw new InvalidStreamIdException();

            StartTransaction();
            try
            {
                StorageStream tmpStream = null;
                WeakReference streamRef;

                // Check if stream is already opened
                if (_openedStreams.TryGetValue(streamId, out streamRef))
                {
                    var target = streamRef.Target;
                    if (target == null) _openedStreams.Remove(streamId);
                    else tmpStream = target as StorageStream;
                }

                // Open stream
                if (tmpStream == null)
                {
                    var streamMetadata = _streamTable.Get(streamId);
                    if (streamMetadata == null)
                        throw new StreamNotFoundException();

                    tmpStream = new StorageStream(streamMetadata, this);
                    //tmpStream.Changed += StorageStream_Changed;
                    _openedStreams.Add(streamId, new WeakReference(tmpStream));
                }

                tmpStream.Position = 0;

                CommitTransaction();

                return tmpStream;
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }
        /// <summary>
        /// Deletes a stream
        /// </summary>
        /// <param name="streamId">Stream Id</param>
        public void DeleteStream(Guid streamId)
        {
            CheckClosed();

            if (SystemStreamId.IsSystemStreamId(streamId))
                throw new InvalidStreamIdException();

            StartTransaction();
            try
            {
                // Before deleting, set stream size to zero to deallocate all of the space it occupies
                StorageStream tmpStream = OpenStream(streamId);
                tmpStream.SetLength(0);
                tmpStream.Close();

                _openedStreams.Remove(streamId);
                _streamTable.Remove(streamId);

                // Remove stream from list of changed streams
                // <源码修改>
                //tmpStream = streamsChangedDuringTransaction.SingleOrDefault(x => x.StreamId == streamId);
                tmpStream = null;
                foreach (var streamChangedDuringTransaction in _streamsChangedDuringTransaction)
                {
                    if (streamChangedDuringTransaction.StreamId == streamId)
                    {
                        tmpStream = streamChangedDuringTransaction;
                        break;
                    }
                }
                // </源码修改>

                if (tmpStream != null)
                    _streamsChangedDuringTransaction.Remove(tmpStream);
                // Remove stream from list of created streams
                if (_streamsCreatedDuringTransaction.Contains(streamId))
                    _streamsCreatedDuringTransaction.Remove(streamId);

                CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// Checks if storage contains specified stream
        /// </summary>
        public bool ContainsStream(Guid streamId)
        {
            CheckClosed();

            if (SystemStreamId.IsSystemStreamId(streamId))
                throw new InvalidStreamIdException();

            return _streamTable.Contains(streamId);
        }
        /// <summary>
        /// Gets areas where specified stream segments are located
        /// </summary>
        public List<SegmentExtent> GetStreamExtents(Guid streamId)
        {
            CheckClosed();

            if (SystemStreamId.IsSystemStreamId(streamId))
                throw new InvalidStreamIdException();

            StorageStream stream = OpenStream(streamId);
            return stream.GetStreamExtents();
        }
        /// <summary>
        /// Gets areas where empty space segments are located
        /// </summary>
        public IEnumerable<SegmentExtent> GetFreeSpaceExtents()
        {
            CheckClosed();
            // <源码修改>
            //if (FreeSpaceStream != null)
            //{
            //    return FreeSpaceStream.Segments
            //        .Select(x => new SegmentExtent(x.Location, x.Size)).ToList();
            //}
            //else
            //    return new List<SegmentExtent>();
            if (FreeSpaceStream == null) return new List<SegmentExtent>();
            var result = new List<SegmentExtent>();
            foreach (var segment in FreeSpaceStream.Segments)
                result.Add(new SegmentExtent(segment.Location, segment.Size));
            return result;
            // </源码修改>
        }
        /// <summary>
        /// Closes the storage
        /// </summary>
        public void Close()
        {
            if (_transactionLevel > 0)
            {
                InternalRollbackTransaction();
                throw new StorageException("Unable to close storage while transaction is pending");
            }

            if (!isClosed)
            {
                lock (_openedStreams)
                {
                    //cacheCleanupTimer.Dispose();
                    //cacheCleanupTimer = null;

                    RollbackTransaction();

                    // Cache stream table into empty space stream
                    MasterStream.Flush();
                    MasterStream.Close();
                    // <源码增加>
                    _transactionStream.Flush();
                    _transactionStream.Close();
                    // </源码增加>

                    _openedStreams.Clear();
                    _streamsChangedDuringTransaction.Clear();

                    isClosed = true;
                }
            }
        }

        /// <summary>
        /// Gets all of the stream Id's
        /// </summary>
        public IEnumerable<Guid> GetStreams()
        {
            return GetStreams(null);
        }

        /// <summary>
        /// Gets all of the stream Id's
        /// </summary>
        /// <param name="tag">
        /// If specified, only streams with specified tag are returned.
        /// The tag is a place to store some custom information, i.e. type of data in the stream.
        /// </param>
        public IEnumerable<Guid> GetStreams(int? tag)
        {
            //return streamTable.Get()
            //    .Where(x => !SystemStreamId.IsSystemStreamId(x.StreamId))
            //    .Where(x => !tag.HasValue || x.Tag == tag.Value)
            //    .Select(x => x.StreamId)
            //    .ToList();
            var metadatas = _streamTable.Get();
            var result = new List<Guid>();
            foreach (var metadata in metadatas)
            {
                if (!SystemStreamId.IsSystemStreamId(metadata.StreamId) && (!tag.HasValue || metadata.Tag == tag.Value))
                    result.Add(metadata.StreamId);
            }
            return result;
        }

        /// <summary>
        /// Trim the master file to the location where data ends
        /// </summary>
        public void TrimStorage()
        {
            // <源码修改>
            //Segment lastSegment = FreeSpaceStream.Segments.SingleOrDefault(x => !x.NextLocation.HasValue);
            Segment lastSegment = null;
            foreach (var segment in FreeSpaceStream.Segments)
            {
                if (!segment.NextLocation.HasValue)
                {
                    lastSegment = segment;
                    break;
                }
            }
            // </源码修改>
            if (lastSegment != null)
            {
                MasterStream.SetLength(lastSegment.DataAreaStart);
            }
        }

        /// <summary>
        /// Start a transaction
        /// </summary>
        public void StartTransaction()
        {
            try
            {
                CheckClosed();
                _transactionLevel++;

                if (_transactionLevel == 1)
                {
                    if (_streamsChangedDuringTransaction.Count > 0)
                        throw new StorageException("At the begining of transaction there should be no changed streams");

                    NotifyTransactionChanging(TransactionStateChangeType.Start);

                    MasterStream.StartTransaction();

                    if (_transactionStream != null)
                    {
                        // Make a list of extents that doesn't need to be backed up
                        //IEnumerable<Transactions.Segment> list = FreeSpaceStream != null ? FreeSpaceStream.Segments.Select(x => new Transactions.Segment(x.DataAreaStart, x.DataAreaSize)) : null;
                        IEnumerable<Transactions.Segment> list;
                        if (FreeSpaceStream == null)
                        {
                            list = null;
                        }
                        else
                        {
                            List<Transactions.Segment> segments = new List<Transactions.Segment>();
                            foreach (var segment in FreeSpaceStream.Segments)
                                segments.Add(new Transactions.Segment(segment.DataAreaStart, segment.DataAreaSize));
                            list = segments;
                        }
                        _transactionStream.BeginTransaction(list);
                    }
    
                    NotifyTransactionChanged(TransactionStateChangeType.Start);
                }
            }
            catch
            {
                InternalRollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// Commits a transaction
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                CheckClosed();
                if (_transactionLevel == 1)
                {
                    NotifyTransactionChanging(TransactionStateChangeType.Commit);

                    SaveChanges();
                    if (_transactionStream != null)
                        _transactionStream.EndTransaction();

                    _streamsCreatedDuringTransaction.Clear();
                    MasterStream.Flush();
                    MasterStream.CommitTransaction();
                    Statistics.TransactionsCommited++;
                }

                if (_transactionLevel > 0)
                {
                    _transactionLevel--;

                    if (_transactionLevel == 0)
                        NotifyTransactionChanged(TransactionStateChangeType.Commit);
                }
            }
            catch
            {
                InternalRollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// Rollbacks a transaction
        /// </summary>
        public void RollbackTransaction()
        {
            CheckClosed();

            if (_transactionStream != null)
            {
                NotifyTransactionChanging(TransactionStateChangeType.Rollback);

                InternalRollbackTransaction();

                _transactionLevel = 0;
                Statistics.TransactionsRolledBack++;
                NotifyTransactionChanged(TransactionStateChangeType.Rollback);
            }
            else
            {
                CommitTransaction();
            }
        }

        public void TruncateStorage()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods
        void InternalRollbackTransaction()
        {
            if (_transactionLevel > 0)
            {
                // Remove opened streams created during transaction
                lock (_openedStreams)
                {
                    foreach (Guid streamId in _streamsCreatedDuringTransaction)
                    {
                        WeakReference reference;
                        if (_openedStreams.TryGetValue(streamId, out reference))
                        {
                            var target = reference.Target;
                            if (target != null)
                            {
                                StorageStream tmpStream = target as StorageStream;
                                tmpStream.InternalClose();
                            }
                            _openedStreams.Remove(streamId);
                        }
                    }
                    _streamsCreatedDuringTransaction.Clear();

                    // Rollback data
                    _transactionStream.RollbackTransaction();
                    MasterStream.RollbackTransaction();
                    _streamsChangedDuringTransaction.Clear();

                    // Rollback changes in stream table
                    _streamTableStream.ReloadSegmentsOnRollback(_streamTableStreamMetadata);
                    _streamTable.RollbackTransaction();

                    // Reload segments in system and opened streams because segments has changed
                    // <源码修改>
                    //foreach (var item in openedStreams.Values.ToList())
                    foreach (var item in _openedStreams.Values)
                    // </源码修改>
                    {
                        var target = item.Target;
                        if (target != null)
                        {
                            StorageStream tmpStream = target as StorageStream;
                            if (_streamTable.Contains(tmpStream.StreamId))
                            {
                                StorageStreamMetadata tmpStreamMetadata = _streamTable.Get(tmpStream.StreamId);
                                tmpStream.ReloadSegmentsOnRollback(tmpStreamMetadata);
                            }
                            else
                            {
                                tmpStream.InternalClose();
                            }
                        }
                    }

                    // Reload empty space segments
                    var freeSpaceStreamMetadata = _streamTable.Get(SystemStreamId.EmptySpace);
                    FreeSpaceStream.ReloadSegmentsOnRollback(freeSpaceStreamMetadata);
                }
            }
        }
        /// <summary>
        /// Creates a storage
        /// </summary>
        void CreateStorage(Stream stream)
        {
            this.MasterStream = new MasterStream(stream, false);

            // Initialize storage metadata
            Segment metadataStreamSegment = Segment.Create(0, BlockSize, null);
            metadataStreamSegment.Save(stream);

            StorageStream metadataStream = new StorageStream(new StorageStreamMetadata(null)
            {
                FirstSegmentPosition = 0,
                InitializedLength = BlockSize - Segment.StructureSize,
                Length = BlockSize - Segment.StructureSize,
                StreamId = SystemStreamId.StorageMetadata,
                StreamTableIndex = -1
            }, this);
            StorageMetadata storageMetadata = new StorageMetadata("[TmStorage 1.0]"); // Set metadata again because above, stream was not specified
            storageMetadata.Save(metadataStream);
            metadataStream.Close();

            // Initialize stream table
            long streamTableSegmentSize = 1000 / ((int)BlockSize / StorageStreamMetadata.StructureSize) * BlockSize;
            Segment streamTableSegment = Segment.Create(BlockSize, streamTableSegmentSize, null);
            stream.Position = metadataStreamSegment.DataAreaEnd;
            streamTableSegment.Save(stream);

            StorageStream streamTableStream = new StorageStream(new StorageStreamMetadata(null)
            {
                FirstSegmentPosition = BlockSize,
                InitializedLength = streamTableSegmentSize - Segment.StructureSize,
                Length = streamTableSegmentSize - Segment.StructureSize,
                StreamId = SystemStreamId.StreamTable,
                StreamTableIndex = -1
            }, this);

            // Initialize empty space stream
            Segment emptyStreamSegment = Segment.Create(streamTableSegment.DataAreaEnd, long.MaxValue - streamTableSegment.DataAreaEnd, null);
            stream.Position = streamTableSegment.DataAreaEnd;
            emptyStreamSegment.Save(stream);

            // Write empty space stream metadata to stream table
            StorageStreamMetadata emptySpaceStreamMetadata = new StorageStreamMetadata(streamTableStream)
            {
                FirstSegmentPosition = emptyStreamSegment.Location,
                InitializedLength = emptyStreamSegment.DataAreaSize,
                Length = emptyStreamSegment.DataAreaSize,
                StreamId = SystemStreamId.EmptySpace,
                StreamTableIndex = 0
            };
            emptySpaceStreamMetadata.Save();

            this.MasterStream = null;
        }
        /// <summary>
        /// Opens the storage
        /// </summary>
        void OpenStorage()
        {
            StartTransaction();
            try
            {
                // For metadata assume block size of 512 because BlockSize is unknown at this point.
                // 512 is the smallest block size so it will work as long as storage metadata is not
                // longer than 512 bytes
                _storageMetadataStream = new StorageStream(new StorageStreamMetadata(null)
                {
                    FirstSegmentPosition = 0,
                    InitializedLength = 512 - Segment.StructureSize,
                    Length = 512 - Segment.StructureSize,
                    StreamId = SystemStreamId.StorageMetadata,
                    StreamTableIndex = -1
                }, this);
                StorageMetadata = StorageMetadata.Load(_storageMetadataStream);

                _streamTableStreamMetadata = new StorageStreamMetadata(_storageMetadataStream)
                {
                    FirstSegmentPosition = BlockSize,
                    StreamId = SystemStreamId.StreamTable,
                    StreamTableIndex = -1
                };
                _streamTableStream = new StorageStream(_streamTableStreamMetadata, this);
                _streamTable = new StreamTable(_streamTableStream);

                var freeSpaceStreamMetadata = _streamTable.Get(SystemStreamId.EmptySpace);
                FreeSpaceStream = new StorageStream(freeSpaceStreamMetadata, this);

                CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }
        /// <summary>
        /// Saves changes of all changed streams during transaction
        /// </summary>
        void SaveChanges()
        {
            foreach (var stream in _streamsChangedDuringTransaction)
            {
                stream.Save();
            }
            if (_streamTable != null)
                _streamTable.SaveChanges();
            _streamsChangedDuringTransaction.Clear();
        }
        void CheckClosed()
        {
            if (isClosed)
                throw new StorageClosedException();
        }
        void NotifyTransactionChanged(TransactionStateChangeType transactionStateChangeType)
        {
            if (TransactionStateChanged != null)
                TransactionStateChanged(this, new TransactionStateChangedEventArgs(transactionStateChangeType));
        }
        void NotifyTransactionChanging(TransactionStateChangeType transactionStateChangeType)
        {
            if (TransactionStateChanging != null)
                TransactionStateChanging(this, new TransactionStateChangedEventArgs(transactionStateChangeType));
        }
        #endregion

        #region Internal methods
        internal void StreamChanged(StorageStreamChangeType changeType, StorageStream stream)
        {
            if (!SystemStreamId.IsSystemStreamId(stream.StreamId) || stream.StreamId == SystemStreamId.EmptySpace)
            {
                switch (changeType)
                {
                    case StorageStreamChangeType.SegmentsAndMetadata:
                        if (!_streamsChangedDuringTransaction.Contains(stream))
                            _streamsChangedDuringTransaction.Add(stream);
                        break;
                    case StorageStreamChangeType.Closing:
                        if (_streamsChangedDuringTransaction.Contains(stream))
                            _streamsChangedDuringTransaction.Remove(stream);

                        _openedStreams.Remove(stream.StreamId);
                        //e.Stream.Changed -= StorageStream_Changed;
                        break;
                }
            }
        }
        #endregion Internal methods

        #region Event handlers
        /*void StorageStream_Changed(object sender, StorageStreamChangedArgs e)
        {
            switch (e.ChangeType)
            {
                case StorageStreamChangeType.SegmentsAndMetadata:
                    if (!streamsChangedDuringTransaction.Contains(e.Stream))
                        streamsChangedDuringTransaction.Add(e.Stream);
                    break;
                case StorageStreamChangeType.Closing:
                    if (streamsChangedDuringTransaction.Contains(e.Stream))
                        streamsChangedDuringTransaction.Remove(e.Stream);
                    openedStreams.Remove(e.Stream.StreamId);
                    //e.Stream.Changed -= StorageStream_Changed;
                    break;
            }
        }*/
        #endregion

        #region Events
        /// <summary>
        /// Triggered after transaction state has changed
        /// </summary>
        public event EventHandler<TransactionStateChangedEventArgs> TransactionStateChanged;
        /// <summary>
        /// Triggered before changing transaction state
        /// </summary>
        public event EventHandler<TransactionStateChangedEventArgs> TransactionStateChanging;
        #endregion
    }
}
