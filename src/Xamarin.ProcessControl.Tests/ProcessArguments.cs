// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

using Xunit;

using Xamarin.ProcessControl;

namespace Xamarin.ProcessControl.Tests
{
    public class ProcessArgumentsTests
    {
        [Fact]
        public void Empty ()
        {
            Assert.Same (ProcessArguments.Empty, ProcessArguments.Create ());
            Assert.Same (ProcessArguments.Empty, ProcessArguments.Parse (string.Empty));
        }

        [Theory]
        [InlineData]
        [InlineData ("one")]
        [InlineData ("one", "two")]
        [InlineData ("one", "two", "three")]
        [InlineData ("one", "two", "three", "four")]
        public void Create (params string [] args)
            => Assert.Equal (args, ProcessArguments.Create (args));

        [Fact]
        public void Insert ()
            => Assert.Equal (
                new [] { "zero", "one", "two", "three", "four" },
                ProcessArguments
                    .Create ("one", "three")
                    .Insert (1, "two")
                    .Insert (3, "four")
                    .Insert (0, "zero"));

        [Fact]
        public void InsertRange ()
            => Assert.Equal (
                new [] { "zero", "one", "two", "three", "four" },
                ProcessArguments
                    .Create ("four")
                    .InsertRange (0, "zero", "one")
                    .InsertRange (2, new [] { "two", "three" }));

        [Fact]
        public void Add ()
            => Assert.Equal (
                new [] { "one", "two", "three" },
                ProcessArguments
                    .Empty
                    .Add ("one")
                    .Add ("two")
                    .Add ("three"));

        [Fact]
        public void AddRange ()
            => Assert.Equal (
                new [] { "zero", "one", "two", "three", "four" },
                ProcessArguments
                    .Empty
                    .AddRange ("zero", "one", "two")
                    .AddRange (new [] { "three", "four" }));

        [Theory]
        [InlineData ("hello", "hello")]
        [InlineData ("hello world", "\"hello world\"")]
        [InlineData ("\"", "\"\\\"\"")]
        public void Quote (string unquoted, string quoted)
            => Assert.Equal (quoted, ProcessArguments.Quote (unquoted));

        [Theory]
        [InlineData ("hello", "hello")]
        [InlineData ("hello world", "hello", "world")]
        [InlineData ("'hello world'", "hello world")]
        [InlineData ("\"hello world\"", "hello world")]
        [InlineData ("one 'two three' four", "one", "two three", "four")]
        public void ParseWithoutGlobs (string commandLine, params string [] expectedArguments)
            => Assert.Equal (
                expectedArguments,
                ProcessArguments.Parse (commandLine));

        [Theory]
        [InlineData ("hello", "hello")]
        [InlineData ("hello world", "hello world")]
        [InlineData ("'hello world'", "\"hello world\"")]
        [InlineData ("\"hello world\"", "\"hello world\"")]
        [InlineData ("one 'two three' four", "one \"two three\" four")]
        public void ParseAndToStringRoundTrip (string commandLine, string expectedToString)
            => Assert.Equal (
                expectedToString,
                ProcessArguments.Parse (commandLine).ToString ());
    }
}