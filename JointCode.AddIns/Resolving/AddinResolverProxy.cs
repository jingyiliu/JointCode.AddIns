//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;
using JointCode.AddIns.Core.Convertion;
using JointCode.AddIns.Core.FileScanning;
using JointCode.AddIns.Core.Helpers;
using JointCode.AddIns.Metadata;
using JointCode.Common.Conversion;

namespace JointCode.AddIns.Resolving
{
    class AddinResolverProxy : MarshalByRefObject
    {
        // @return value: whether the persistence file (AddinIndexManager/AddinBodyRepository) has been updated.
        public bool Resolve(IMessageDialog dialog, FilePackResult filePackResult, string persistentFile, string transactionFile)
        {
            var storage = StorageHelper.CreateStorage(persistentFile, transactionFile);
            var indexManager = new IndexManager { Storage = storage };
            if (indexManager.Read())
                indexManager.Build();
            var bodyRepo = new BodyRepository { Storage = storage };
            var convertionManager = new ConvertionManager();
            InitializeConvertion(convertionManager);

            var resolver = new DefaultAddinResolver(indexManager, bodyRepo, convertionManager);
            var hasNewAddin = resolver.Resolve(dialog, filePackResult);

            //storage.Close();
            return hasNewAddin;
        }

        static void InitializeConvertion(ConvertionManager convertionManager)
        {
            convertionManager.Register(new StringToVersionConverter());
            convertionManager.Register(new StringToGuidConverter());
            convertionManager.Register(new StringToDateTimeConverter());
            convertionManager.Register(new StringToTimeSpanConverter());
            convertionManager.Register(new StringToDecimalConverter());
            convertionManager.Register(new StringToBooleanConverter());
            convertionManager.Register(new StringToCharConverter());
            convertionManager.Register(new StringToSByteConverter());
            convertionManager.Register(new StringToByteConverter());
            convertionManager.Register(new StringToInt16Converter());
            convertionManager.Register(new StringToUInt16Converter());
            convertionManager.Register(new StringToInt32Converter());
            convertionManager.Register(new StringToUInt32Converter());
            convertionManager.Register(new StringToInt64Converter());
            convertionManager.Register(new StringToUInt64Converter());
            convertionManager.Register(new StringToSingleConverter());
            convertionManager.Register(new StringToDoubleConverter());
        }
    }
}