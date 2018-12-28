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

using System;
using biz.dfch.CS.Abiquo.Client;
using biz.dfch.CS.Abiquo.Client.Factory;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using biz.dfch.CS.Testing.Attributes;
using biz.dfch.CS.Testing.PowerShell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Current = biz.dfch.CS.Abiquo.Client.v1;
using System.Collections.Generic;
using System.Linq;

namespace biz.dfch.PS.Abiquo.Client.Tests
{
    [TestClass]
    public class GetMachineTest
    {
        public static BaseAbiquoClient Client;
        public BaseAbiquoClient CurrentClient;
        public static User User;

        public VirtualMachines VirtualMachines = new VirtualMachines()
        {
            Collection = new List<VirtualMachine>()
            {
                new VirtualMachine()
                {
                    Id = 42,
                    Name = "Edgar"
                },
                new VirtualMachine()
                {
                    Id = 1,
                    Name = "MachineWithDuplicateName"
                },
                new VirtualMachine()
                {
                    Id = 2,
                    Name = "mACHINEwITHdUPLICATEnAME"
                },
            }
        };

        private readonly Type sut = typeof(GetMachine);

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            //Mock.SetupStatic(typeof(ContractFailedEventHandler));
            //Mock.Arrange(
            //        () =>
            //            ContractFailedEventHandler.EventHandler(Arg.IsAny<object>(),
            //                Arg.IsAny<ContractFailedEventArgs>()))
            //    .DoNothing();

            User = new User()
            {
                Active = true,
                AuthType = "Abiquo",
                AvailableVirtualDatacenters = "all",
                Description = "arbitrary-description",
                Email = "someone@example.com",
                FirstLogin = false,
                Id = EnterServer.TENANT_ID_DEFAULT_VALUE,
                Locale = "en-us",
            };

            var enterpriseLink =
                new Current.LinkBuilder()
                    .BuildHref(string.Format("https://abiquo.example.com/api/admin/enterprises/{0}",
                        EnterServer.TENANT_ID_DEFAULT_VALUE))
                    .BuildRel(Current.AbiquoRelations.ENTERPRISE)
                    .BuildTitle("Abiquo")
                    .GetLink();

            User.Links = new List<Link>()
            {
                enterpriseLink
            };

            // this must be inside ClassInitialize - otherwise the tests will only work one at a time
            Client = Mock.Create<Current.AbiquoClient>(Behavior.CallOriginal);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Mock.Arrange(() => Client.CurrentUserInformation)
                .IgnoreInstance()
                .Returns(User);

            Mock.SetupStatic(typeof(AbiquoClientFactory));
            Mock.Arrange(() => AbiquoClientFactory.GetByVersion())
                .Returns(Client);
            Mock.Arrange(() => AbiquoClientFactory.GetByVersion(Arg.IsAny<string>()))
                .Returns(Client);

            // strange - the mock inside the PSCmdlet only works when we invoke the mocked methods here first
            // this seems to be related to the Lazy<T> we use to initialise the Abiquo client via the factory
            CurrentClient = ModuleConfiguration.Current.Client;
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"'Id'.+range")]
        public void InvokeWithInvalidIdParameterThrowsParameterBindingValidationException1()
        {
            var parameters = @"-Id 0";
            var results = PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"'Name'.+empty")]
        public void InvokeWithInvalidNameParameterThrowsParameterBindingValidationException1()
        {
            var parameters = @"-Name ''";
            var results = PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        public void ParameterSetListHasExpectedOutputType()
        {
            PsCmdletAssert.HasOutputType(sut, typeof(VirtualMachine), GetMachine.ParameterSets.LIST);
        }

        [TestMethod]
        public void ParameterSetNameHasExpectedOutputType()
        {
            PsCmdletAssert.HasOutputType(sut, typeof(VirtualMachine), GetMachine.ParameterSets.NAME);
        }

        [TestMethod]
        public void ParameterSetIdHasExpectedOutputType()
        {
            PsCmdletAssert.HasOutputType(sut, typeof(VirtualMachine), GetMachine.ParameterSets.ID);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "ModuleConfiguration.Current.Client.IsLoggedIn")]
        public void InvokeNotLoggedInThrowsContractException()
        {
            Mock.Arrange(() => CurrentClient.IsLoggedIn)
                .Returns(false);

            var parameters = @"-ListAvailable";

            var results = PsCmdletAssert.Invoke(sut, parameters, ex => ex);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void InvokeParameterSetListAvailableSucceeds()
        {
            Mock.Arrange(() => CurrentClient.IsLoggedIn)
                .Returns(true);

            Mock.Arrange(() => CurrentClient.GetAllVirtualMachines())
                .Returns(VirtualMachines)
                .MustBeCalled();

            var parameters = @"-ListAvailable";
            
            var results = PsCmdletAssert.Invoke(sut, parameters);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(3, results.Count);

            Mock.Assert(() => CurrentClient.GetAllVirtualMachines());
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void InvokeParameterSetIdSucceeds()
        {
            Mock.Arrange(() => CurrentClient.IsLoggedIn)
                .Returns(true);

            Mock.Arrange(() => CurrentClient.GetAllVirtualMachines())
                .Returns(VirtualMachines)
                .MustBeCalled();

            // this Id does not exist
            var parameters = @"-Id 42";
            
            var results = PsCmdletAssert.Invoke(sut, parameters);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);

            var result = results[0].BaseObject as VirtualMachine;
            Assert.IsNotNull(result, results[0].BaseObject.GetType().FullName);
            Assert.AreEqual(42, result.Id);
            Assert.AreEqual("Edgar", result.Name);

            Mock.Assert(() => CurrentClient.GetAllVirtualMachines());
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void InvokeParameterSetIdWriterErrorRecord()
        {
            Mock.Arrange(() => CurrentClient.IsLoggedIn)
                .Returns(true);

            Mock.Arrange(() => CurrentClient.GetAllVirtualMachines())
                .Returns(VirtualMachines)
                .MustBeCalled();

            // this Id does not exist
            var parameters = @"-Id 9999";
            
            var results = PsCmdletAssert.Invoke(sut, parameters, er => { Assert.IsTrue(er.Single().Exception.Message.Contains("9999")); });
            
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);

            Mock.Assert(() => CurrentClient.GetAllVirtualMachines());
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void InvokeParameterSetNameSucceeds()
        {
            Mock.Arrange(() => CurrentClient.IsLoggedIn)
                .Returns(true);

            Mock.Arrange(() => CurrentClient.GetAllVirtualMachines())
                .Returns(VirtualMachines)
                .MustBeCalled();

            // this Id does not exist
            var parameters = @"-Name Edgar";
            
            var results = PsCmdletAssert.Invoke(sut, parameters);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);

            var result = results[0].BaseObject as VirtualMachine;
            Assert.IsNotNull(result, results[0].BaseObject.GetType().FullName);
            Assert.AreEqual(42, result.Id);
            Assert.AreEqual("Edgar", result.Name);

            Mock.Assert(() => CurrentClient.GetAllVirtualMachines());
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void InvokeParameterSetNameSucceedsAndReturnsCollection()
        {
            Mock.Arrange(() => CurrentClient.IsLoggedIn)
                .Returns(true);

            Mock.Arrange(() => CurrentClient.GetAllVirtualMachines())
                .Returns(VirtualMachines)
                .MustBeCalled();

            // this Id does not exist
            var parameters = @"-Name MachineWithDuplicateName";
            
            var results = PsCmdletAssert.Invoke(sut, parameters);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);

            var result0 = results[0].BaseObject as VirtualMachine;
            Assert.IsNotNull(result0, results[0].BaseObject.GetType().FullName);
            Assert.AreEqual("MachineWithDuplicateName".ToLower(), result0.Name.ToLower());

            var result1 = results[1].BaseObject as VirtualMachine;
            Assert.IsNotNull(result1, results[1].BaseObject.GetType().FullName);
            Assert.AreEqual("MachineWithDuplicateName".ToLower(), result1.Name.ToLower());
            
            Assert.AreNotEqual(result0.Id, result1.Id);

            Mock.Assert(() => CurrentClient.GetAllVirtualMachines());
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "Assertion.+isValidVirtualDataCenterIdAndVirtualApplianceIdCombination")]
        public void InvokeParameterSetListWithInvalidVdcVappCombination()
        {
            Mock.Arrange(() => CurrentClient.IsLoggedIn)
                .Returns(true);

            // this Id does not exist
            var parameters = @"-VirtualApplianceId 42";
            
            var results = PsCmdletAssert.Invoke(sut, parameters, ex => ex);
        }
    }
}
