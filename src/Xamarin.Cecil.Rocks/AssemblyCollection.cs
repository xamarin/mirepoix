// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

using Mono.Cecil;

namespace Xamarin.Cecil.Rocks
{
    public class AssemblyCollection
    {
        readonly HashSet<string> searchDirectories = new HashSet<string> ();
        readonly List<string> assemblyFileNames = new List<string> ();

        public ReaderParameters ReaderParameters { get; }
        public BaseAssemblyResolver AssemblyResolver { get; }

        public AssemblyCollection (ReadingMode readingMode = ReadingMode.Immediate) : this (
            new ReaderParameters (readingMode),
            new DefaultAssemblyResolver ())
        {
        }

        public AssemblyCollection (
            ReaderParameters readerParameters,
            BaseAssemblyResolver assemblyResolver)
        {
            ReaderParameters = readerParameters
                ?? throw new ArgumentNullException (nameof (readerParameters));

            ReaderParameters.AssemblyResolver = assemblyResolver;
            ReaderParameters.MetadataResolver = new MetadataResolver (assemblyResolver);

            AssemblyResolver = assemblyResolver
                ?? throw new ArgumentNullException (nameof (assemblyResolver));
        }

        public AssemblyCollection AddAssembly (string assemblyFileName)
        {
            assemblyFileName = Path.GetFullPath (assemblyFileName);

            var searchDirectory = Path.GetDirectoryName (assemblyFileName);
            if (searchDirectories.Add (searchDirectory))
                AssemblyResolver.AddSearchDirectory (searchDirectory);

            if (!assemblyFileNames.Contains (assemblyFileName))
                assemblyFileNames.Add (assemblyFileName);

            return this;
        }

        public AssemblyCollection AddAssemblies (IEnumerable<string> assemblyFileNames)
        {
            foreach (var assemblyFileName in assemblyFileNames ?? Array.Empty<string> ())
                AddAssembly (assemblyFileName);

            return this;
        }

        public IEnumerable<AssemblyDefinition> Load ()
        {
            foreach (var assemblyFileName in assemblyFileNames)
                yield return AssemblyDefinition.ReadAssembly (
                    assemblyFileName,
                    ReaderParameters);
        }
    }
}