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
using System.Management.Automation;
using biz.dfch.CS.Abiquo.Client;
using biz.dfch.CS.Abiquo.Client.Authentication;
using biz.dfch.CS.Abiquo.Client.Factory;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using biz.dfch.CS.Testing.Attributes;
using biz.dfch.CS.Testing.PowerShell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Current = biz.dfch.CS.Abiquo.Client.v1;
using System.Collections.Generic;

namespace biz.dfch.PS.Abiquo.Client.Tests
{
    [TestClass]
    public class EnterServerTest
    {
        public static BaseAbiquoClient Client;
        public static User User;

        private readonly Type sut = typeof(EnterServer);

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
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
                .BuildHref(string.Format("https://abiquo.example.com/api/admin/enterprises/{0}", EnterServer.TENANT_ID_DEFAULT_VALUE))
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
            var currentClient = ModuleConfiguration.Current.Client;
        }
        
        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'Uri'.+'System\.Uri'")]
        public void InvokeWithInvalidUriParameterThrowsParameterBindingException1()
        {
            var parameters = @"-Uri ";
            var results = PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"Username.+Password")]
        public void InvokeWithMissingParameterThrowsParameterBindingException()
        {
            var parameters = @"-Uri httpS://abiquo.example.com/api/";
            var results = PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"Credential")]
        public void InvokeWithNullCredentialParameterThrowsParameterBindingValidationException()
        {
            var parameters = @"-Uri httpS://abiquo.example.com/api/ -Credential $null";
            var results = PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'Credential'.+.System\.String.")]
        public void InvokeWithInvalidCredentialParameterThrowsParameterBindingException()
        {
            var parameters = @"-Uri httpS://abiquo.example.com/api/ -Credential arbitrary-user-as-string";
            var results = PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        public void InvokeWithInexistentUriThrowsWebException()
        {
            var parameters = @"-Uri httpS://abiquo.example.com/api/ -Username admin -Password password";
            var results = PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeWithParameterSetPlainSucceeds()
        {
            var uri = new Uri("httpS://abiquo.example.com/api/");
            var user = "arbitrary-user";
            var password = "arbitrary-password";
            var parameters = string.Format(@"-Uri {0} -User '{1}' -Password '{2}'", uri, user, password);

            Mock.Arrange(() => Client.Login(Arg.IsAny<string>(), Arg.IsAny<IAuthenticationInformation>()))
                .IgnoreInstance()
                .Returns(true);

            var results = PsCmdletAssert.Invoke(sut, parameters);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.IsInstanceOfType(results[0].BaseObject, typeof(BaseAbiquoClient));
            var result = (BaseAbiquoClient) results[0].BaseObject;
            Assert.IsNotNull(result);
            Assert.AreEqual(User.Id, result.CurrentUserInformation.Id);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeWithParameterSetPlainWithValidTenantSucceeds()
        {
            var uri = new Uri("httpS://abiquo.example.com/api/");
            var user = "arbitrary-user";
            var password = "arbitrary-password";
            var tenantId = 42;
            var parameters = string.Format(@"-Uri {0} -User '{1}' -Password '{2}' -TenantId {3}", uri, user, password, tenantId);

            Mock.Arrange(() => Client.Login(Arg.IsAny<string>(), Arg.IsAny<IAuthenticationInformation>()))
                .IgnoreInstance()
                .Returns(true)
                .MustBeCalled();
            
            Mock.Arrange(() => Client.SwitchEnterprise(tenantId))
                .IgnoreInstance()
                .DoNothing()
                .MustBeCalled();

            var results = PsCmdletAssert.Invoke(sut, parameters);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.IsInstanceOfType(results[0].BaseObject, typeof(BaseAbiquoClient));
            var result = (BaseAbiquoClient) results[0].BaseObject;
            Assert.IsNotNull(result);
            Assert.AreEqual(User.Id, result.CurrentUserInformation.Id);

            Mock.Assert(Client);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeWithParameterSetCredSucceeds()
        {
            var uri = new Uri("httpS://abiquo.example.com/api/");
            var user = "arbitrary-user";
            var password = "arbitrary-password";
            var parameters = string.Format(@"-Uri {0} -Credential $([pscredential]::new('{1}', (ConvertTo-SecureString -AsPlainText -String {2} -Force)))", uri, user, password);

            Mock.Arrange(() => Client.Login(Arg.IsAny<string>(), Arg.IsAny<IAuthenticationInformation>()))
                .IgnoreInstance()
                .Returns(true);

            var results = PsCmdletAssert.Invoke(sut, parameters);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.IsInstanceOfType(results[0].BaseObject, typeof(BaseAbiquoClient));
            var result = (BaseAbiquoClient) results[0].BaseObject;
            Assert.IsNotNull(result);
            Assert.AreEqual(User.Id, result.CurrentUserInformation.Id);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeWithParameterSetOAuth2Succeeds()
        {
            var uri = new Uri("httpS://abiquo.example.com/api/");
            var token = "arbitrary-token";
            var parameters = string.Format(@"-Uri {0} -OAuth2Token '{1}'", uri, token);

            Mock.Arrange(() => Client.Login(Arg.IsAny<string>(), Arg.IsAny<IAuthenticationInformation>()))
                .IgnoreInstance()
                .Returns(true);

            var results = PsCmdletAssert.Invoke(sut, parameters);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.IsInstanceOfType(results[0].BaseObject, typeof(BaseAbiquoClient));
            var result = (BaseAbiquoClient) results[0].BaseObject;
            Assert.IsNotNull(result);
            Assert.AreEqual(User.Id, result.CurrentUserInformation.Id);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        [ExpectContractFailure(MessagePattern = "Assertion.+hasLoginSucceeded1")]
        public void InvokeWithInvalidTokenThrowsContractException()
        {
            var uri = new Uri("httpS://abiquo.example.com/api/");
            var token = "invalid-token";
            var parameters = string.Format(@"-Uri {0} -OAuth2Token '{1}'", uri, token);

            Mock.Arrange(() => Client.Login(Arg.IsAny<string>(), Arg.IsAny<IAuthenticationInformation>()))
                .IgnoreInstance()
                .Returns(false);

            var results = PsCmdletAssert.Invoke(sut, parameters, ex => ex);
            
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        // DFTODO - maybe this test should be a generic test inside the Testing package
        [TestMethod]
        [ExpectedException(typeof(IncompleteParseException))]
        public void InvokeWithInvalidStringThrowsIncompleteParseException()
        {
            var user = "arbitrary-user";
            var password = "arbitrary-password";
            // missing string terminator
            var parameters = string.Format(@"-Uri httpS://abiquo.example.com/api/ -User '{0} -Password '{1}'", user, password);
            var results = PsCmdletAssert.Invoke(sut, parameters);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            var result = results[0].BaseObject.ToString();
            Assert.AreEqual("tralala", result);
        }
    }
}
