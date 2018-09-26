// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

using Xunit;

using static Xamarin.GuidHelpers;

namespace Xamarin
{
    public class GuidHelperTests
    {
        [Fact]
        public void V3 ()
        {
            Assert.Equal (new Guid ("ead916d5-7bbe-34b4-9946-f015e8b9371e"), GuidV3 (GuidNamespace.DNS, "catoverflow.com"));
            Assert.Equal (new Guid ("b8b17fb8-058d-355d-9a2b-d090ac5f7cb7"), GuidV3 (GuidNamespace.URL, "https://catoverflow.com/cqK1y73n"));
        }

        [Fact]
        public void V5 ()
        {
            Assert.Equal (new Guid ("e8ef0d79-b41b-5d81-9ca6-2303e133a365"), GuidV5 (GuidNamespace.DNS, "catoverflow.com"));
            Assert.Equal (new Guid ("a63a9b6e-3429-5fc3-8f13-fefd7b356e94"), GuidV5 (GuidNamespace.URL, "https://catoverflow.com/cqK1y73n"));
        }

        static readonly string [] expectedGB18030TestDataHashes = {
            "4e5588fa-23b6-54ad-93fd-b60eadb0ad96",
            "63032cec-394f-566d-8256-10fad9a917fc",
            "741142e5-3b70-5bdd-93e7-4f92263870b5",
            "e83026e4-5606-5b81-8dd0-6331aeefbde5",
            "9ef2a5dd-16b3-5a41-90ec-7b68b2edcd7f",
            "8106e789-4558-5d9d-aecf-c5a02d5a8b1a",
            "98af12aa-1e51-5345-ba32-c0527c300d29",
            "88aa0f6f-546c-516b-9286-9757bd465fe6",
            "ce18fe1b-b2af-574d-af0c-a4a50b731d43",
            "99c39637-9252-59d8-b3ca-e362b832c3b1",
            "13c4c0f9-ccb9-5999-bb22-23720837c689"
        };

        [Theory]
        [ClassData (typeof (GB18030TestData))]
        public void GB18030 (string stringDescription, string value)
        {
            var hashIndex = GB18030TestData.GetIndexOfStringDescription (stringDescription);
            Assert.True (
                hashIndex >= 0 && hashIndex < expectedGB18030TestDataHashes.Length,
                $"{nameof (expectedGB18030TestDataHashes)} needs updating to reflect {nameof (GB18030TestData)}");

            Assert.Equal (
                new Guid (expectedGB18030TestDataHashes [hashIndex]),
                GuidV5 (new Guid ("0495e02e-a13d-44f7-b7d6-f3385434cde9"), value));
        }

        [Fact]
        public void ThreadLocal ()
        {
            var ns = Guid.NewGuid ();

            void CreateGuid (Guid expected, string name, Func<Guid, string, Guid> func)
            {
                for (int i = 0; i < 100000; i++)
                    Assert.Equal (expected, func (ns, name));
            }

            var actions = new Action [64];
            for (int i = 0; i < actions.Length; i++) {
                Func<Guid, string, Guid> func;
                var name = $"thread{i}v";

                if (i % 2 == 0) {
                    func = GuidV3;
                    name += "3";
                } else {
                    func = GuidV5;
                    name += "5";
                }

                actions [i] = () => CreateGuid (func (ns, name), name, func);
            }

            Parallel.Invoke (actions);
        }
    }
}