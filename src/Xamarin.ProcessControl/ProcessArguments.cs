//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if HAVE_SYSTEM_COLLECTIONS_IMMUTABLE
using System.Collections.Immutable;
#endif

namespace Xamarin.ProcessControl
{
    /// <summary>
    /// An immutable list of process arguments. Arguments will be quoted if escaping is necessary.
    /// </summary>
    public sealed class ProcessArguments : IReadOnlyList<string>
    {
        #if !HAVE_SYSTEM_COLLECTIONS_IMMUTABLE

        /// <summary>
        /// Simple inefficient immutable list backed by List<T> for the sake
        /// of not having to take a dependency on System.Immutable.Collections.
        /// </summary>
        sealed class ImmutableList<T> : IReadOnlyList<T>
        {
            public static readonly ImmutableList<T> Empty = new ImmutableList<T> (Array.Empty<T> ());

            readonly IReadOnlyList<T> items;

            ImmutableList (IReadOnlyList<T> items)
                => this.items = items;

            public T this [int index] => items [index];

            public int Count => items.Count;

            public ImmutableList<T> Add (T item)
            {
                var newItems = new List<T> (items.Count + 1);
                newItems.AddRange (items);
                newItems.Add (item);
                return new ImmutableList<T> (newItems);
            }

            public ImmutableList<T> AddRange (IEnumerable<T> items)
            {
                var newItems = new List<T> (this.items.Count + 1);
                newItems.AddRange (this.items);
                newItems.AddRange (items);
                return new ImmutableList<T> (newItems);
            }

            public ImmutableList<T> Insert (int index, T item)
            {
                var newItems = new List<T> (items.Count + 1);
                newItems.AddRange (items);
                newItems.Insert (index, item);
                return new ImmutableList<T> (newItems);
            }

            public ImmutableList<T> InsertRange (int index, IEnumerable<T> items)
            {
                var newItems = new List<T> (this.items.Count + 1);
                newItems.AddRange (this.items);
                newItems.InsertRange (index, items);
                return new ImmutableList<T> (newItems);
            }

            public IEnumerator<T> GetEnumerator ()
                => ((IEnumerable<T>)items).GetEnumerator ();

            IEnumerator IEnumerable.GetEnumerator ()
                => items.GetEnumerator ();
        }

        #endif // HAVE_SYSTEM_COLLECTIONS_IMMUTABLE

        public static ProcessArguments Parse (string commandLine, bool expandGlobs = true)
        {
            var args = Empty;
            var builder = new StringBuilder ();
            var quote = Char.MinValue;

            void AppendArg (char q, string a)
            {
                if (q == Char.MinValue && String.IsNullOrWhiteSpace (a))
                    return;

                if (!expandGlobs || q == '\'') {
                    args = args.Add (a);
                    return;
                }

                foreach (var glob in PathHelpers.Glob.ShellExpand (a))
                    args = args.Add (glob);
            }

            for (int i = 0; i < commandLine.Length; i++) {
                var c = commandLine [i];
                var n = Char.MinValue;
                if (i < commandLine.Length - 1)
                    n = commandLine [i + 1];

                if (Char.IsWhiteSpace (c) && quote == Char.MinValue) {
                    AppendArg (quote, builder.ToString ());
                    builder.Length = 0;
                } else if (quote == Char.MinValue && (c == '\'' || c == '"')) {
                    quote = c;
                } else if (c == '\\' && n == quote) {
                    builder.Append (n);
                    i++;
                } else if (quote != Char.MinValue && c == quote) {
                    quote = Char.MinValue;
                } else {
                    builder.Append (c);
                }
            }

            if (builder.Length > 0)
                AppendArg (quote, builder.ToString ());

            return args;
        }

        /// <summary>
        /// Quote a single argument if necessary.
        /// </summary>
        public static string Quote (string argument)
        {
            if (argument == null)
                throw new ArgumentNullException (nameof (argument));

            if (argument.Length == 0)
                return "\"\"";

            StringBuilder builder = null;

            for (int i = 0; i < argument.Length; i++) {
                var c = argument [i];
                if (Char.IsWhiteSpace (c) || c == '\b' || c == '"' || c == '\\') {
                    if (builder == null) {
                        builder = new StringBuilder (argument.Length + 8);
                        builder.Append ('"');
                        builder.Append (argument.Substring (0, i));
                    }

                    if (c == '"' || c == '\\')
                        builder.Append ('\\');
                }

                if (builder != null)
                    builder.Append (c);
            }

            if (builder == null)
                return argument;

            builder.Append ('"');
            return builder.ToString ();
        }

        public static ProcessArguments Create (params string [] arguments)
            => Create ((IEnumerable<string>)arguments);

        public static ProcessArguments Create (IEnumerable<string> arguments)
        {
            var processArguments = Empty;
            if (arguments != null)
                processArguments = processArguments.AddRange (arguments);
            return processArguments;
        }

        public static ProcessArguments FromCommandAndArguments (string command, IEnumerable<string> arguments)
        {
            if (command == null)
                throw new ArgumentNullException (nameof (command));

            var processArguments = Empty.Add (command);

            if (arguments != null)
                processArguments = processArguments.AddRange (arguments);

            return processArguments;
        }

        public static readonly ProcessArguments Empty = new ProcessArguments (ImmutableList<string>.Empty);

        readonly ImmutableList<string> arguments;

        ProcessArguments (ImmutableList<string> arguments)
            => this.arguments = arguments;

        public int Count => arguments.Count;

        public string this [int index] => arguments [index];

        public ProcessArguments Add (string argument)
        {
            if (argument == null)
                throw new ArgumentNullException (nameof (argument));

            return new ProcessArguments (
                (arguments ?? ImmutableList<string>.Empty).Add (argument));
        }

        public ProcessArguments AddRange (params string [] arguments)
            => AddRange ((IEnumerable<string>)arguments);

        public ProcessArguments AddRange (IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException (nameof (arguments));

            if (!arguments.Any ())
                return this;

            return new ProcessArguments (
                (this.arguments ?? ImmutableList<string>.Empty).AddRange (arguments));
        }

        public ProcessArguments Insert (int index, string argument)
        {
            if (argument == null)
                throw new ArgumentNullException (nameof (argument));

            return new ProcessArguments (
                (arguments ?? ImmutableList<string>.Empty).Insert (
                    index,
                    argument));
        }

        public ProcessArguments InsertRange (int index, params string [] arguments)
            => InsertRange (index, (IEnumerable<string>)arguments);

        public ProcessArguments InsertRange (int index, IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException (nameof (arguments));

            if (!arguments.Any ())
                return this;

            return new ProcessArguments (
                (this.arguments ?? ImmutableList<string>.Empty).InsertRange (
                    index,
                    arguments));
        }

        public override string ToString ()
            => string.Join (" ", this.Select (Quote));

        public IEnumerator<string> GetEnumerator ()
            => arguments.GetEnumerator ();

        IEnumerator IEnumerable.GetEnumerator ()
            => GetEnumerator ();
    }
}