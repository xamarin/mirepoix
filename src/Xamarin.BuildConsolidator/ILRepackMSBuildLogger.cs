
//
// Author:
//   Aaron Bockover <abock@microsoft.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xamarin.BuildConsolidator
{
    sealed class ILRepackMSBuildLogger : ILRepacking.ILogger
    {
        readonly TaskLoggingHelper log;

        public ILRepackMSBuildLogger (TaskLoggingHelper log)
            => this.log = log;

        bool ILRepacking.ILogger.ShouldLogVerbose { get; set; }

        void ILRepacking.ILogger.DuplicateIgnored (string ignoredType, object ignoredObject)
            => log.LogMessage (
                MessageImportance.Low,
                "Ignoring duplicate {0} {1}",
                ignoredType,
                ignoredObject);

        void ILRepacking.ILogger.Log (object str)
            => log.LogMessage (MessageImportance.Normal, str.ToString ());

        void ILRepacking.ILogger.Error (string msg)
            => log.LogError (msg);

        void ILRepacking.ILogger.Warn (string msg)
        {
            if (!msg.StartsWith ("Did not write source server data to output assembly.", StringComparison.OrdinalIgnoreCase))
                log.LogWarning (msg);
        }

        void ILRepacking.ILogger.Info (string msg)
            => log.LogMessage (MessageImportance.Normal, msg);

        void ILRepacking.ILogger.Verbose (string msg)
            => log.LogMessage (MessageImportance.Low, msg);
    }
}