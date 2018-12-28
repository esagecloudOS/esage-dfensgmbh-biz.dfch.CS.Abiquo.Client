/**
 * Copyright 2016 d-fens GmbH
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using biz.dfch.CS.Abiquo.Client.Communication;
using biz.dfch.CS.Abiquo.Client.v1;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using biz.dfch.CS.Testing.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace biz.dfch.CS.Abiquo.Client.Tests.v1
{
    [TestClass]
    public class VirtualMachineExtensionsTest
    {
        private const string ABIQUO_API_BASE_URI = "https://abiquo.example.com/api/";

        [TestMethod]
        public void ExtractIdsFromVirtualMachineLinksSucceed()
        {
            var vdcId = 42;
            var vappId = 8;
            var vmId = 15;

            var href = string.Format(AbiquoUriSuffixes.VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID, vdcId, vappId, vmId);
            var links = new List<Link>()
            {
                new LinkBuilder()
                    .BuildRel(AbiquoRelations.EDIT)
                    .BuildHref(ABIQUO_API_BASE_URI + href)
                    .BuildTitle("ABQ_a81a8033-eb56-4cf1-8d7d-6355bb3b5157")
                    .BuildType("application/vnd.abiquo.virtualmachine+json")
                    .GetLink()
            };

            var sut = new VirtualMachine()
            {
                Id = vmId,
                Links = links,
            };

            var result = sut.ExtractIds();

            Assert.IsNotNull(result);
            Assert.AreEqual(vdcId, result.Item1);
            Assert.AreEqual(vappId, result.Item2);
            Assert.AreEqual(vmId, result.Item3);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void ExtractIdsFromNullVirtualMachineThrowsContractException()
        {
            var virtualMachine = default(VirtualMachine);

            VirtualMachineExtensions.ExtractIds(virtualMachine);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "null.+link")]
        public void ExtractIdsFromNullLinkThrowsContractException()
        {
            var link = default(Link);

            VirtualMachineExtensions.ExtractIds(link);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = @"AbiquoRelations\.EDIT.+link.Rel")]
        public void ExtractIdsFromInvalidLinkThrowsContractException()
        {
            var rel = "invalid-rel";
            
            var vdcId = 42;
            var vappId = 8;
            var vmId = 15;

            var href = string.Format(AbiquoUriSuffixes.VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID, vdcId, vappId, vmId);

            var link = new LinkBuilder()
                .BuildRel(rel)
                .BuildHref(ABIQUO_API_BASE_URI + href)
                .BuildTitle("ABQ_a81a8033-eb56-4cf1-8d7d-6355bb3b5157")
                .BuildType("application/vnd.abiquo.virtualmachine+json")
                .GetLink();

            VirtualMachineExtensions.ExtractIds(link);
        }

        [TestMethod]
        public void ExtractIdsFromEditLinksSucceed()
        {
            var vdcId = 42;
            var vappId = 8;
            var vmId = 15;

            var href = string.Format(AbiquoUriSuffixes.VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID, vdcId, vappId, vmId);

            var link = new LinkBuilder()
                .BuildRel(AbiquoRelations.EDIT)
                .BuildHref(ABIQUO_API_BASE_URI + href)
                .BuildTitle("ABQ_a81a8033-eb56-4cf1-8d7d-6355bb3b5157")
                .BuildType("application/vnd.abiquo.virtualmachine+json")
                .GetLink();

            var result = VirtualMachineExtensions.ExtractIds(link);

            Assert.IsNotNull(result);
            Assert.AreEqual(vdcId, result.Item1);
            Assert.AreEqual(vappId, result.Item2);
            Assert.AreEqual(vmId, result.Item3);
        }
    }
}
