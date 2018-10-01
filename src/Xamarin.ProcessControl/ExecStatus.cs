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
        public DateTimeOffset EventTime { get; }
        public TimeSpan ExecDuration { get; }

        ExecStatusEventArgs (
            Exec exec,
            ExecStatus status,
            DateTimeOffset eventTime,
            TimeSpan execDuration)
        {
            Exec = exec;
            Status = status;
            EventTime = eventTime;
            ExecDuration = execDuration;
        }

        internal ExecStatusEventArgs (Exec exec)
            : this (
                exec,
                ExecStatus.ProcessStarted,
                DateTimeOffset.UtcNow,
                TimeSpan.Zero)
        {
        }

        internal ExecStatusEventArgs WithProcessEnded ()
        {
            var now = DateTimeOffset.UtcNow;
            return new ExecStatusEventArgs (
                Exec,
                ExecStatus.ProcessEnded,
                now,
                now - EventTime);
        }
    }
}