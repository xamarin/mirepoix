// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Xamarin.ProcessControl
{
    public enum ExecStatus
    {
        None,
        ProcessStarted,
        ProcessEnded
    }

    public sealed class ExecStatusEventArgs : EventArgs
    {
        public Exec Exec { get; }
        public ExecStatus Status { get; }
        public int? ExitCode { get; }
        public DateTimeOffset EventTime { get; }
        public TimeSpan ExecDuration { get; }

        ExecStatusEventArgs (
            Exec exec,
            ExecStatus status,
            int? exitCode,
            DateTimeOffset eventTime,
            TimeSpan execDuration)
        {
            Exec = exec;
            Status = status;
            ExitCode = exitCode;
            EventTime = eventTime;
            ExecDuration = execDuration;
        }

        internal ExecStatusEventArgs (Exec exec)
            : this (
                exec,
                ExecStatus.ProcessStarted,
                null,
                DateTimeOffset.UtcNow,
                TimeSpan.Zero)
        {
        }

        internal ExecStatusEventArgs WithProcessEnded (int exitCode)
        {
            var now = DateTimeOffset.UtcNow;
            return new ExecStatusEventArgs (
                Exec,
                ExecStatus.ProcessEnded,
                exitCode,
                now,
                now - EventTime);
        }
    }
}