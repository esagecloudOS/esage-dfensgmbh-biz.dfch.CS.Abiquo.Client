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
using System.Collections.Generic;
using System.Net.Http.Headers;
using biz.dfch.CS.Abiquo.Client.Authentication;
﻿using biz.dfch.CS.Abiquo.Client.Communication;
﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using biz.dfch.CS.Abiquo.Client.v1;
using biz.dfch.CS.Abiquo.Client.General;
﻿using biz.dfch.CS.Abiquo.Client.v1.Model;
using biz.dfch.CS.Commons;
using biz.dfch.CS.Commons.Rest;
using biz.dfch.CS.Testing.Attributes;
using Task = biz.dfch.CS.Abiquo.Client.v1.Model.Task;

namespace biz.dfch.CS.Abiquo.Client.Tests
{
    [TestClass]
    public class BaseAbiquoClientTest
    {
        private const string ABIQUO_API_BASE_URI = "https://abiquo.example.com/api/";
        private const string VIRTUALMACHINETEMPLATE_HREF = "http://abiquo/api/admin/enterprises/42/datacenterrepositories/42/virtualmachinetemplates/42";
        private const string USERNAME = "ArbitraryUsername";
        private const string PASSWORD = "ArbitraryPassword";
        private const string SESSION_TOKEN = "auth=ARBITRARY";
        private const string AUTH_COOKIE_VALUE = "auth=ABC123";
        private const int INVALID_ID = 0;

        private static readonly string SET_COOKIE_HEADER_VALUE_1 = string.Format("{0}; Expires=Fri, 30-Dec-2016 23:59:59 GMT; Path=/; Secure; HttpOnly", AUTH_COOKIE_VALUE);
        private const string SET_COOKIE_HEADER_VALUE_2 = "ABQSESSIONID=1234567891234567891; Expires=Fri, 30-Dec-2016 23:59:59 GMT; Path=/; Secure; HttpOnly";

        private readonly IAuthenticationInformation _authenticationInformation = new BasicAuthenticationInformation(USERNAME, PASSWORD);

        private delegate bool TryGetValuesDelegate(string name, out IEnumerable<string> values);

        private BaseAbiquoClient sut = new DummyAbiquoClient();

        private readonly VirtualMachine _validVirtualMachine = new VirtualMachine()
        {
            Cpu = 2
            ,
            Ram = 512
            ,
            Name = "Arbitrary"
        };

        private readonly Task _validTask = new Task()
        {
            OwnerId = "ArbitraryOwnerId"
            ,
            State = TaskStateEnum.FINISHED_SUCCESSFULLY
            ,
            TaskId = Guid.NewGuid().ToString()
            ,
            Timestamp = 1
            ,
            Type = TaskTypeEnum.DEPLOY
        };

        private readonly VirtualMachineState _virtualMachineState = new VirtualMachineState()
        {
            State = VirtualMachineStateEnum.ON
        };

        [TestMethod]
        [ExpectContractFailure]
        public void GetTenantIdBeforeLoginThrowsContractException()
        {
            // Arrange
            sut.Logout();

            // Act
            var tenantId = sut.TenantId;

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void InvalidAbqiuoClientThatDoesNotSetVersionPropertyThrowsContractExceptionOnInstantiation()
        {
            // Arrange

            // Act
            new InvalidAbiquoClient(null, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void InvalidAbqiuoClientThatDoesNotSetValidTaskPollingWaitTimeMillisecondsPropertyThrowsContractExceptionOnInstantiation()
        {
            // Arrange

            // Act
            new InvalidAbiquoClient("v1", 0, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void InvalidAbqiuoClientThatDoesNotSetValidTaskPollingTimeoutMillisecondsPropertyThrowsContractExceptionOnInstantiation()
        {
            // Arrange

            // Act
            new InvalidAbiquoClient("v1", 42, 0);

            // Assert
        }

        #region ExecuteRequest

        [TestMethod]
        public void ExecuteRequestWithoutAdditionalHeadersAndBodyCallsRestCallExecutor()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var expectedRequestUri = UriHelper.ConcatUri(ABIQUO_API_BASE_URI, AbiquoUriSuffixes.ENTERPRISES);

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2 };

                    return true;
                }))
                .OccursOnce();

            var expectedHeaders = new Dictionary<string, string>()
            {
                {Client.Constants.Authentication.COOKIE_HEADER_KEY, SESSION_TOKEN}
            };

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, expectedRequestUri, expectedHeaders, null))
                .IgnoreInstance()
                .Returns("Arbitrary-Result")
                .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            // Act
            var result = sut.ExecuteRequest(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, null, null);

            // Assert
            Assert.AreEqual("Arbitrary-Result", result);

            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void ExecuteRequestWithAdditionalHeadersMergesHeadersAndCallsRestCallExecutorWithMergedHeaders()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var expectedRequestUri = UriHelper.ConcatUri(ABIQUO_API_BASE_URI, AbiquoUriSuffixes.ENTERPRISES);

            var headers = new Dictionary<string, string>()
            {
                { Client.Constants.Authentication.COOKIE_HEADER_KEY, SESSION_TOKEN }
                ,
                { AbiquoHeaderKeys.ACCEPT_HEADER_KEY, VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISES }
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2 };

                    return true;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, expectedRequestUri, headers, null))
                .IgnoreInstance()
                .Returns("Arbitrary-Result")
                .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            // Act
            var result = sut.ExecuteRequest(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, headers, null);

            // Assert
            Assert.AreEqual("Arbitrary-Result", result);

            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void ExecuteRequestSetsCookieRequestHeaderIfSessionTokenNotNull()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var expectedRequestUri = UriHelper.ConcatUri(ABIQUO_API_BASE_URI, AbiquoUriSuffixes.ENTERPRISES);

            var headers = new Dictionary<string, string>()
            {
                { Constants.Authentication.COOKIE_HEADER_KEY, SESSION_TOKEN }
                ,
                { AbiquoHeaderKeys.ACCEPT_HEADER_KEY, VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISES }
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2 };

                    return true;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, expectedRequestUri, headers, null))
                .IgnoreInstance()
                .Returns("Arbitrary-Result")
                .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            // Act
            var result = sut.ExecuteRequest(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, headers, null);

            // Assert
            Assert.AreEqual("Arbitrary-Result", result);

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void ExecuteRequestSetsSessionTokenBasedOnSetCookieResponseHeader()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var expectedRequestUri = UriHelper.ConcatUri(ABIQUO_API_BASE_URI, AbiquoUriSuffixes.ENTERPRISES);

            var headers = new Dictionary<string, string>()
            {
                { Constants.Authentication.COOKIE_HEADER_KEY, SESSION_TOKEN }
                ,
                { AbiquoHeaderKeys.ACCEPT_HEADER_KEY, VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISES }
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2 };

                    return true;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, expectedRequestUri, headers, null))
                .IgnoreInstance()
                .Returns("Arbitrary-Result")
                .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            // Act
            var result = sut.ExecuteRequest(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, headers, null);

            // Assert
            Assert.AreEqual("Arbitrary-Result", result);
            Assert.AreEqual(AUTH_COOKIE_VALUE, sut.SessionToken);

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void ExecuteRequestResultingInResponseWithoutSetCookieHeaderSetsSessionTokenToNull()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var expectedRequestUri = UriHelper.ConcatUri(ABIQUO_API_BASE_URI, AbiquoUriSuffixes.ENTERPRISES);

            var headers = new Dictionary<string, string>()
            {
                { Constants.Authentication.COOKIE_HEADER_KEY, SESSION_TOKEN }
                ,
                { AbiquoHeaderKeys.ACCEPT_HEADER_KEY, VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISES }
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = null;

                    return false;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, expectedRequestUri, headers, null))
                .IgnoreInstance()
                .Returns("Arbitrary-Result")
                .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            // Act
            var result = sut.ExecuteRequest(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, headers, null);

            // Assert
            Assert.AreEqual("Arbitrary-Result", result);
            Assert.IsNull(sut.SessionToken);

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void ExecuteRequestResultingInResponseWithSetCookieHeaderNotContainingSessionTokenNotModifyingSessionToken()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var expectedRequestUri = UriHelper.ConcatUri(ABIQUO_API_BASE_URI, AbiquoUriSuffixes.ENTERPRISES);

            var headers = new Dictionary<string, string>()
            {
                { Constants.Authentication.COOKIE_HEADER_KEY, SESSION_TOKEN }
                ,
                { AbiquoHeaderKeys.ACCEPT_HEADER_KEY, VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISES }
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { "ArbitraryCookieValue" };

                    return true;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, expectedRequestUri, headers, null))
                .IgnoreInstance()
                .Returns("Arbitrary-Result")
                .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            // Act
            var result = sut.ExecuteRequest(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, headers, null);

            // Assert
            Assert.AreEqual("Arbitrary-Result", result);
            Assert.AreEqual(SESSION_TOKEN, sut.SessionToken);

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        #endregion ExecuteRequest


        #region Invoke

        [TestMethod]
        [ExpectContractFailure]
        public void GenericInvokeWithEmptyUriSuffixThrowsContractException()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            // Act
            sut.Invoke<Enterprise>(null, new Dictionary<string, string>());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GenericInvokeIfNotLoggedInThrowsContractException()
        {
            // Arrange

            // Act
            sut.Invoke<Enterprises>(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, null, null, default(string));

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void InvokeWithEmptyUriSuffixThrowsContractException()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            // Act
            sut.Invoke(HttpMethod.Get, " ", null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void InvokeWithInvalidUriSuffixThrowsContractException()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            // Act
            sut.Invoke(HttpMethod.Get, "http://example.com", null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void InvokeIfNotLoggedInThrowsContractException()
        {
            // Arrange

            // Act
            sut.Invoke(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, null, null, default(AbiquoBaseDto));

            // Assert
        }

        [TestMethod]
        public void InvokeWithFilterCallsRestCallExecutorWithRequestUriContainingFilterExpression()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var filter = new FilterBuilder()
                .BuildFilterPart("currentPage", 1)
                .BuildFilterPart("limit", 25)
                .GetFilter();

            var expectedRequestUri = string.Format("{0}?{1}", UriHelper.ConcatUri(ABIQUO_API_BASE_URI, AbiquoUriSuffixes.ENTERPRISES), "currentPage=1&limit=25");

            var headers = new Dictionary<string, string>()
            {
                { Constants.Authentication.COOKIE_HEADER_KEY, SESSION_TOKEN }
                ,
                { AbiquoHeaderKeys.ACCEPT_HEADER_KEY, VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISES }
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2 };

                    return true;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, expectedRequestUri, headers, null))
                .IgnoreInstance()
                .Returns("Arbitrary-Result")
                .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            // Act
            var result = sut.Invoke(HttpMethod.Get, AbiquoUriSuffixes.ENTERPRISES, filter, headers);

            // Assert
            Assert.AreEqual("Arbitrary-Result", result);

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void InvokeWithAbsoluteUriSucceeds()
        {
            var rel = "disk1";
            var href = "cloud/virtualdatacenters/1/disks/42";
            var link = new LinkBuilder()
                .BuildRel(rel)
                .BuildHref(ABIQUO_API_BASE_URI + href)
                .BuildTitle("/a81a8033-eb56-4cf1-8d7d-6355bb3b5157")
                .BuildType("application/vnd.abiquo.harddisk+json")
                .GetLink();

            var dicionaryParameers = new DictionaryParameters()
            {
                { "expected-key", "expected-value" }
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2 };

                    return true;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, link.Href, Arg.IsAny<Dictionary<string, string>>(), null))
                .IgnoreInstance()
                .Returns(dicionaryParameers.SerializeObject())
                .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var result = sut.Invoke(new Uri(link.Href));

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DictionaryParameters));
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("expected-key"));
            Assert.IsTrue(result.ContainsValue("expected-value"));

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        #endregion Invoke


        #region Invoke Link(s)

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "link")]
        public void GenericInvokeLinkWithNullLinkThrowsContractException()
        {
            // Arrange

            // Act
            sut.InvokeLink<AbiquoBaseDto>(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "href")]
        public void GenericInvokeLinkWithLinkContainingInvalidHrefThrowsContractException()
        {
            // Arrange
            var link = new LinkBuilder().BuildHref(string.Empty).BuildType(AbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINETEMPLATE).GetLink();

            // Act
            sut.InvokeLink<VirtualMachineTemplate>(link);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "link")]
        public void NonGenericInvokeLinkWithNullLinkThrowsContractException()
        {
            // Arrange

            // Act
            sut.InvokeLink(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "href")]
        public void NonGenericInvokeLinkWithLinkContainingInvalidHrefThrowsContractException()
        {
            // Arrange
            var link = new LinkBuilder().BuildHref(string.Empty).BuildType(AbiquoMediaDataTypes.VND_ABIQUO_VIRTUALMACHINETEMPLATE).GetLink();

            // Act
            sut.InvokeLink(link);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "links")]
        public void InvokeLinkByRelOnNullLinksThrowsContractException()
        {
            var rel = "disk0";
            var links = default(ICollection<Link>);

            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var result = sut.InvokeLinkByRel(links, rel);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "rel")]
        public void InvokeLinkByRelOnNullRelThrowsContractException()
        {
            var rel = "   ";
            var links = new List<Link>()
            {
                new LinkBuilder()
                    .BuildRel(rel)
                    .BuildHref("https://abiquo.example.com:443/api/cloud/virtualdatacenters/1/disks/42")
                    .BuildTitle("/a81a8033-eb56-4cf1-8d7d-6355bb3b5157")
                    .BuildType("application/vnd.abiquo.harddisk+json")
                    .GetLink()
            };

            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var result = sut.InvokeLinkByRel(links, rel);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "null.+link.+rel.+disk1")]
        public void InvokeLinkByRelWithInexistingRelThrowsContractException()
        {
            var rel = "disk1";
            var links = new List<Link>()
            {
                new LinkBuilder()
                    .BuildRel("inexistent-rel")
                    .BuildHref("https://abiquo.example.com:443/api/cloud/virtualdatacenters/1/disks/42")
                    .BuildTitle("/a81a8033-eb56-4cf1-8d7d-6355bb3b5157")
                    .BuildType("application/vnd.abiquo.harddisk+json")
                    .GetLink()
            };

            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var result = sut.InvokeLinkByRel(links, rel);
        }

        [TestMethod]
        public void InvokeLinkByRelWithValidLinkAndExistingRelSucceeds()
        {
            var rel = "disk1";
            var href = "cloud/virtualdatacenters/1/disks/42";
            var link = new LinkBuilder()
                .BuildRel(rel)
                .BuildHref(ABIQUO_API_BASE_URI + href)
                .BuildTitle("/a81a8033-eb56-4cf1-8d7d-6355bb3b5157")
                .BuildType("application/vnd.abiquo.harddisk+json")
                .GetLink();
            var links = new List<Link>()
            {
                link
            };

            var dicionaryParameers = new DictionaryParameters()
            {
                { "expected-key", "expected-value" }
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2 };

                    return true;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, link.Href, Arg.IsAny<Dictionary<string, string>>(), null))
                .IgnoreInstance()
                .Returns(dicionaryParameers.SerializeObject())
                .OccursOnce();
            
            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var result = sut.InvokeLinkByRel(links, rel);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DictionaryParameters));
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("expected-key"));
            Assert.IsTrue(result.ContainsValue("expected-value"));

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void InvokeLinksByTypeWithLinksAndLinkTypeSucceeds()
        {
            var roleId = 42;
            var roleName = "CLOUD_ADMIN";

            var type = AbiquoMediaDataTypes.VND_ABIQUO_ROLE;
            var href = UriHelper.ConcatUri(ABIQUO_API_BASE_URI, string.Format(AbiquoUriSuffixes.ROLE_BY_ID, roleId));
            var link = new LinkBuilder()
                .BuildHref(href)
                .BuildRel(AbiquoRelations.ROLE)
                .BuildTitle(roleName)
                .BuildType(type)
                .GetLink();

            var links = new List<Link>()
            {
                link, link
            };

            var role = new Role()
            {
                Name = roleName,
                Id = roleId
            };

            var responseHeaders = Mock.Create<HttpResponseHeaders>();
            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() { SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2 };

                    return true;
                }))
                .Occurs(2);

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor.Invoke(HttpMethod.Get, link.Href, Arg.IsAny<Dictionary<string, string>>(), null))
                .IgnoreInstance()
                .Returns(role.SerializeObject())
                .Occurs(2);

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .Occurs(2);

            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            var result = sut.InvokeLinksByType(links, AbiquoMediaDataTypes.VND_ABIQUO_ROLE);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ICollection<DictionaryParameters>));
            Assert.AreEqual(2, result.Count);

            foreach (var dictionaryParameters in result)
            {
                Assert.IsTrue(dictionaryParameters.ContainsKey("name"));
                Assert.AreEqual(roleName, dictionaryParameters["name"]);
                Assert.IsTrue(dictionaryParameters.ContainsKey("id"));
                Assert.AreEqual(42L, dictionaryParameters["id"]);
            }

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        #endregion Invoke Link(s)


        #region Enterprises

        [TestMethod]
        [ExpectContractFailure]
        public void GetEnterpriseWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetEnterprise(INVALID_ID);

            // Assert
        }

        #endregion Enterprises


        #region Users

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "enterpriseId")]
        public void GetUsersWithRolesWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUsersWithRoles(INVALID_ID);

            // Assert
        } 
        
        [TestMethod]
        [ExpectContractFailure(MessagePattern = "enterprise")]
        public void GetUsersWithRolesWithNullEnterpriseThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUsersWithRoles(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserOfCurrentEnterpriseWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUserOfCurrentEnterprise(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "enterprise")]
        public void GetUserWithNullEnterpriseAndValidUserIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUser(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetUserWithEnterpriseAndInvalidUserIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUser(new Enterprise(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUser(INVALID_ID, 15);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUser(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserInformationBeforeLoginThrowsContractException()
        {
            // Arrange
            sut.Logout();

            // Act
            sut.GetUserInformation();

            // Assert
        }

        [TestMethod]
        public void GetUserInformationAfterLoginReturnsUserInformationAboutCurrentlyLoggedInUser()
        {
            // Arrange
            sut.Login(ABIQUO_API_BASE_URI, _authenticationInformation);

            // Act
            var userInfo = sut.GetUserInformation();

            // Assert
            Assert.IsNotNull(userInfo);
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserInformationOfSpecificUserWithNullUsernameThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUserInformation(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserInformationOfSpecificUserWithEmptyUsernameThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUserInformation(" ");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserInformationOfSpecificUserInSpecificEnterpriseWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUserInformation(INVALID_ID, USERNAME);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserInformationOfSpecificUserInSpecificEnterpriseWithNullUsernameThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUserInformation(42, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetUserInformationOfSpecificUserInSpecificEnterpriseWithEmptyUsernameThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetUserInformation(42, " ");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void SwitchEnterpriseWithNullEnterpriseThrowsContractException()
        {
            // Arrange

            // Act
            sut.SwitchEnterprise(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void SwitchEnterpriseWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.SwitchEnterprise(INVALID_ID);

            // Assert
        }

        #endregion Users


        #region Roles

        [TestMethod]
        [ExpectContractFailure]
        public void GetRoleWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetRole(INVALID_ID);

            // Assert
        }

        #endregion Roles


        #region DataCentersLimits


        [TestMethod]
        [ExpectContractFailure(MessagePattern = "enterprise")]
        public void GetDataCentersLimitsWithNullEnterpriseThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCentersLimits(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetDataCentersLimitsWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCentersLimits(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetDataCenterLimitsOfCurrentEnterpriseWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterLimitsOfCurrentEnterprise(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "enterprise")]
        public void GetDataCenterLimitsWithNullEnterpriseAndValidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterLimits(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetDataCenterLimitsWithEnterpriseAndInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterLimits(new Enterprise(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetDataCenterLimitsWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterLimits(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetDataCenterLimitsWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterLimits(42, INVALID_ID);

            // Assert
        }

        #endregion DataCentersLimits


        #region VirtualMachines

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachinesWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachines(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualAppliance")]
        public void GetVirtualMachinesWithNullVirtualApplianceThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachines(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachinesWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachines(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualAppliance")]
        public void GetVirtualMachineWithNullVirtualApplianceAndValidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachine(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetVirtualMachineWithVirtualApplianceAndInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachine(new VirtualAppliance(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachine(INVALID_ID, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachine(42, INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachineWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachine(42, 42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(0, 42, VIRTUALMACHINETEMPLATE_HREF);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 0, VIRTUALMACHINETEMPLATE_HREF);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithNullVirtualMachineTemplateHrefThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithEmptyVirtualMachineTemplateHrefThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, " ");
            
            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualMachineTemplateHrefThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, "Arbitrary");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(INVALID_ID, 42, 42, 42, 42, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, INVALID_ID, 42, 42, 42, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, INVALID_ID, 42, 42, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidDataCenterRepositoryIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, 42, INVALID_ID, 42, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualAppliance")]
        public void CreateVirtualMachineWithNullVirtualApplianceThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(null, new VirtualMachineTemplate(), _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachineTemplate")]
        public void CreateVirtualMachineWithNullVirtualMachineTemplateThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(new VirtualAppliance(), null, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void CreateVirtualMachineWithNullVirtualMachineThrowsContractException2()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(new VirtualAppliance(), new VirtualMachineTemplate(), null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualMachineTemplateIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, 42, 42, INVALID_ID, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, 42, 42, 42, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualDataCenterId3ThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(INVALID_ID, 42, VIRTUALMACHINETEMPLATE_HREF, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualApplianceId3ThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, INVALID_ID, VIRTUALMACHINETEMPLATE_HREF, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualAppliance")]
        public void CreateVirtualMachineWithNullVirtualApplianceThrowsContractException2()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(null, new VirtualMachineTemplate());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachineTemplate")]
        public void CreateVirtualMachineWithNullVirtualMachineTemplateThrowsContractException2()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(new VirtualAppliance(), null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithNullVirtualMachineTemplateHrefThrowsContractException2()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, null, _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithEmptyVirtualMachineTemplateHrefThrowsContractException2()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, " ", _validVirtualMachine);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, VIRTUALMACHINETEMPLATE_HREF, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateVirtualMachineWithInvalidVirtualMachineTemplateHrefThrowsContractException2()
        {
            // Arrange

            // Act
            sut.CreateVirtualMachine(42, 42, "Arbitrary", null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeployVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.DeployVirtualMachine(INVALID_ID, 42, 42, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeployVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.DeployVirtualMachine(42, INVALID_ID, 42, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeployVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.DeployVirtualMachine(null, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeployVirtualMachineWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.DeployVirtualMachine(42, 42, INVALID_ID, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeployVirtualMachineWithNullVirtualMachineThrowsContractException2()
        {
            // Arrange

            // Act
            sut.DeployVirtualMachine(null, false, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeployVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.DeployVirtualMachine(INVALID_ID, 42, 42, false, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeployVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.DeployVirtualMachine(42, INVALID_ID, 42, false, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeployVirtualMachineWithInvalidVirtualMachineIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.DeployVirtualMachine(42, 42, INVALID_ID, false, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void UpdateVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(null, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void UpdateVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(INVALID_ID, 42, 42, _validVirtualMachine, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void UpdateVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(42, INVALID_ID, 42, _validVirtualMachine, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void UpdateVirtualMachineWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(42, 42, INVALID_ID, _validVirtualMachine, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void UpdateVirtualMachineWithInvalidVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(42, 42, 42, new VirtualMachine(), false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void UpdateVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(INVALID_ID, 42, 42, _validVirtualMachine, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void UpdateVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(42, INVALID_ID, 42, _validVirtualMachine, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void UpdateVirtualMachineWithNullVirtualMachineThrowsContractException2()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(null, false, false);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void UpdateVirtualMachineWithInvalidVirtualMachineIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.UpdateVirtualMachine(42, 42, INVALID_ID, _validVirtualMachine, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ChangeStateOfVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(INVALID_ID, 42, 42, _virtualMachineState);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ChangeStateOfVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(42, INVALID_ID, 42, _virtualMachineState);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ChangeStateOfVirtualMachineWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(42, 42, INVALID_ID, _virtualMachineState);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ChangeStateOfVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(INVALID_ID, 42, 42, _virtualMachineState, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ChangeStateOfVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(42, INVALID_ID, 42, _virtualMachineState, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ChangeStateOfVirtualMachineWithInvalidVirtualMachineIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(42, 42, INVALID_ID, _virtualMachineState, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ChangeStateOfVirtualMachineWithInvalidVirtualMachineStateThrowsContractException2()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(42, 42, INVALID_ID, new VirtualMachineState(), true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void ChangeStateOfVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(null, VirtualMachineStateEnum.OFF);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "state")]
        public void ChangeStateOfVirtualMachineWithNullVirtualMachineStateThrowsContractException2()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(new VirtualMachine(), null, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void ChangeStateOfVirtualMachineWithNullVirtualMachineThrowsContractException3()
        {
            // Arrange

            // Act
            sut.ChangeStateOfVirtualMachine(null, VirtualMachineStateEnum.OFF, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void DeleteVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.DeleteVirtualMachine(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void ProtectVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void ProtectVirtualMachineWithNullVirtualMachineAndValidProtectionCauseThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(null, "Arbitrary Protection Cause");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "protectionCause")]
        public void ProtectVirtualMachineWithVirtualMachineAndNullProtectionCauseThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(new VirtualMachine(), null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "protectionCause")]
        public void ProtectVirtualMachineWithVirtualMachineAndEmptyProtectionCauseThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(new VirtualMachine(), string.Empty);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenterId")]
        public void ProtectVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(INVALID_ID, 42, 42, "Arbitrary Protection Cause");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualApplianceId")]
        public void ProtectVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(42, INVALID_ID, 42, "Arbitrary Protection Cause");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachineId")]
        public void ProtectVirtualMachineWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(42, 42, INVALID_ID, "Arbitrary Protection Cause");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "protectionCause")]
        public void ProtectVirtualMachineWithNullProtectionCauseThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(42, 42, 42, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "protectionCause")]
        public void ProtectVirtualMachineWithEmptyProtectionCauseThrowsContractException()
        {
            // Arrange

            // Act
            sut.ProtectVirtualMachine(42, 42, 42, "");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void UnprotectVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.UnprotectVirtualMachine(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenterId")]
        public void UnprotectVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.UnprotectVirtualMachine(INVALID_ID, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualApplianceId")]
        public void UnprotectVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.UnprotectVirtualMachine(42, INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachineId")]
        public void UnprotectVirtualMachineWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.UnprotectVirtualMachine(42, 42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeleteVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.DeleteVirtualMachine(0, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeleteVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.DeleteVirtualMachine(42, 0, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeleteVirtualMachineWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.DeleteVirtualMachine(42, 42, 0);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeleteVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.DeleteVirtualMachine(0, 42, 42, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeleteVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.DeleteVirtualMachine(42, 0, 42, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void DeleteVirtualMachineWithInvalidVirtualMachineIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.DeleteVirtualMachine(42, 42, 0, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void DeleteVirtualMachineWithNullVirtualMachineThrowsContractException2()
        {
            // Arrange

            // Act
            sut.DeleteVirtualMachine(null, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void GetNetworkConfigurationsForVmWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationsForVm(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetNetworkConfigurationsForVmWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationsForVm(INVALID_ID, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetNetworkConfigurationsForVmWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationsForVm(42, INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetNetworkConfigurationsForVmWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationsForVm(42, 42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetNetworkConfigurationForVmWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationForVm(INVALID_ID, 42, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetNetworkConfigurationForVmWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationForVm(42, INVALID_ID, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetNetworkConfigurationForVmWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationForVm(42, 42, INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetNetworkConfigurationForVmWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationForVm(42, 42, 42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void GetNetworkConfigurationForVmWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationForVm(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetNetworkConfigurationForVmWithInvalidIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.GetNetworkConfigurationForVm(new VirtualMachine(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void GetNicsOfVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetNicsOfVirtualMachine(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetAllTasksOfVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetAllTasksOfVirtualMachine(INVALID_ID, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetAllTasksOfVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetAllTasksOfVirtualMachine(42, INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetAllTasksOfVirtualMachineWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetAllTasksOfVirtualMachine(42, 42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void GetAllTasksOfVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetAllTasksOfVirtualMachine(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetTaskOfVirtualMachineWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetTaskOfVirtualMachine(INVALID_ID, 42, 42, Guid.NewGuid().ToString());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetTaskOfVirtualMachineWithInvalidVirtualApplianceIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetTaskOfVirtualMachine(42, INVALID_ID, 42, Guid.NewGuid().ToString());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetTaskOfVirtualMachineWithInvalidVirtualMachineIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetTaskOfVirtualMachine(42, 42, INVALID_ID, Guid.NewGuid().ToString());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetTaskOfVirtualMachineWithNullTaskIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetTaskOfVirtualMachine(42, 42, 42, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetTaskOfVirtualMachineWithEmptyTaskIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetTaskOfVirtualMachine(42, 42, 42, string.Empty);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualMachine")]
        public void GetTaskOfVirtualMachineWithNullVirtualMachineThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetTaskOfVirtualMachine(null, Guid.NewGuid().ToString());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "taskId")]
        public void GetTaskOfVirtualMachineWithNullTaskIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.GetTaskOfVirtualMachine(_validVirtualMachine, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "taskId")]
        public void GetTaskOfVirtualMachineWithEmptyTaskIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.GetTaskOfVirtualMachine(_validVirtualMachine, string.Empty);

            // Assert
        }

        #endregion VirtualMachines


        #region VirtualMachineTemplates

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "dataCenterRepository")]
        public void GetVirtualMachineTemplatesWithNullThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachineTemplates(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachineTemplatesWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachineTemplates(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachineTemplatesWithInvalidDataCenterRepositoryIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachineTemplates(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "dataCenterRepository")]
        public void GetVirtualMachineTemplateWithNullDataCenterRepositoryThrowsAndIdContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachineTemplate(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetVirtualMachineTemplateWithInvalidIdThrowsAndIdContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachineTemplate(new DataCenterRepository(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachineTemplateWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachineTemplate(INVALID_ID, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachineTemplateWithInvalidDataCenterRepositoryIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachineTemplate(42, INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualMachineTemplateWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualMachineTemplate(42, 42, INVALID_ID);

            // Assert
        }

        #endregion VirtualMachineTemplates


        #region VirtualDataCenters

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualDataCenterWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualDataCenter(INVALID_ID);

            // Assert
        }

        #endregion VirtualDataCenters


        #region VirtualAppliances

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void GetVirtualAppliancesWithNullVirtualDataCenterThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualAppliances(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualAppliancesWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualAppliances(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void GetVirtualAppliancesWithNullVirtualDataCenterAndIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualAppliance(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetVirtualAppliancesWithDataCenterAndInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualAppliance(new VirtualDataCenter(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualApplianceWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualAppliance(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetVirtualApplianceWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetVirtualAppliance(42, INVALID_ID);

            // Assert
        }

        #endregion VirtualAppliances


        #region DataCenterRepositories

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "enterprise")]
        public void GetDataCenterRepositoriesWithNullEnterpriseThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterRepositories(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetDataCenterRepositoriesWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterRepositories(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetDataCenterRepositoryOfCurrentEnterpriseWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterRepositoryOfCurrentEnterprise(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "enterprise")]
        public void GetDataCenterRepositoryWithNullEnterpriseThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterRepository(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetDataCenterRepositoryWithInvalidIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.GetDataCenterRepository(new Enterprise(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetDataCenterRepositoryWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterRepository(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetDataCenterRepositoryWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetDataCenterRepository(42, INVALID_ID);

            // Assert
        }

        #endregion DataCenterRepositories


        #region Tasks

        [TestMethod]
        [ExpectContractFailure]
        public void WaitForTaskCompletionWithNullTaskThrowsContractException()
        {
            // Arrange

            // Act
            sut.WaitForTaskCompletion(null, 1, 1);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void WaitForTaskCompletionWithInvalidTaskThrowsContractException()
        {
            // Arrange

            // Act
            sut.WaitForTaskCompletion(new Task(), 1, 1);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void WaitForTaskCompletionWithInvalidBasePollingWaitTimeThrowsContractException()
        {
            // Arrange

            // Act
            sut.WaitForTaskCompletion(_validTask, INVALID_ID, 1);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void WaitForTaskCompletionWithInvalidTimeoutThrowsContractException()
        {
            // Arrange

            // Act
            sut.WaitForTaskCompletion(_validTask, 1, INVALID_ID);

            // Assert
        }

        #endregion Tasks


        #region Networks

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void GetPrivateNetworksWithNullVirtualDataCenterThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPrivateNetworks(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetPrivateNetworksWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPrivateNetworks(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void GetPrivateNetworkWithNullVirtualDataCenterThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPrivateNetwork(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetPrivateNetworkWithObjectAndInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPrivateNetwork(new VirtualDataCenter(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetPrivateNetworkWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPrivateNetwork(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetPrivateNetworkWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPrivateNetwork(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "vlan")]
        public void GetIpsOfPrivateNetworkWithNullVlanThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfPrivateNetwork(null, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetIpsOfPrivateNetworkWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfPrivateNetwork(INVALID_ID, 42, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetIpsOfPrivateNetworkWithInvalidPrivateNetworkIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfPrivateNetwork(42, INVALID_ID, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetExternalNetworksOfCurrentEnterpriseWithInvalidDataCenterLimitsIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetExternalNetworksOfCurrentEnterprise(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetExternalNetworksWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetExternalNetworks(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetExternalNetworksWithInvalidDataCenterLimitsIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetExternalNetworks(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetExternalNetworkOfCurrentEnterpriseWithInvalidDataCenterLimitsIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetExternalNetworkOfCurrentEnterprise(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetExternalNetworkOfCurrentEnterpriseWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetExternalNetworkOfCurrentEnterprise(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetExternalNetworkWithInvalidLEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetExternalNetwork(INVALID_ID, 42, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetExternalNetworkWithInvalidDataCenterLimitsIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetExternalNetwork(42, INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetExternalNetworkWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetExternalNetwork(42, 42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "vlan")]
        public void GetIpsOfExternalNetworkOfCurrentEnterpriseWithNullVlanThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfExternalNetworkOfCurrentEnterprise(null, true);

            // Assert
        }


        [TestMethod]
        [ExpectContractFailure]
        public void GetIpsOfExternalNetworkOfCurrentEnterpriseWithInvalidDataCenterLimitsIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfExternalNetworkOfCurrentEnterprise(INVALID_ID, 42, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetIpsOfExternalNetworkOfCurrentEnterpriseWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfExternalNetworkOfCurrentEnterprise(42, INVALID_ID, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetIpsOfExternalNetworkWithInvalidEnterpriseIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfExternalNetwork(INVALID_ID, 42, 42, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetIpsOfExternalNetworkWithInvalidDataCenterLimitsIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfExternalNetwork(42, INVALID_ID, 42, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetIpsOfExternalNetworkWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetIpsOfExternalNetwork(42, 42, INVALID_ID, true);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void GetPublicNetworksWithNullVirtualDataCenterThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicNetworks(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetPublicNetworksWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicNetworks(INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void GetPublicNetworkWithNullVirtualDataCenterThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicNetwork(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "id")]
        public void GetPublicNetworkWithInvalidIdThrowsContractException2()
        {
            // Arrange

            // Act
            sut.GetPublicNetwork(new VirtualDataCenter(), INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetPublicNetworkWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicNetwork(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetPublicNetworkWithInvalidIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicNetwork(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetPublicIpsToPurchaseOfPublicNetworkWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicIpsToPurchaseOfPublicNetwork(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetPublicIpsToPurchaseOfPublicNetworkWithInvalidVlanIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicIpsToPurchaseOfPublicNetwork(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void GetPublicIpsToPurchaseOfPublicNetworkWithNullVirtualDataCenterThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicIpsToPurchaseOfPublicNetwork(null, new VlanNetwork());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "vlan")]
        public void GetPublicIpsToPurchaseOfPublicNetworkWithNullVlanThrowsContractException()
        {
            // Arrange

            // Act
            sut.GetPublicIpsToPurchaseOfPublicNetwork(new VirtualDataCenter(), null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void PurchasePublicIpWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.PurchasePublicIp(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void PurchasePublicIpWithInvalidPublicIpIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.PurchasePublicIp(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void PurchasePublicIpWithNullVirtualDataCenterThrowsContractException()
        {
            // Arrange

            // Act
            sut.PurchasePublicIp(null, new PublicIp());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "publicIp")]
        public void PurchasePublicIpWithNullPublicIpThrowsContractException()
        {
            // Arrange

            // Act
            sut.PurchasePublicIp(new VirtualDataCenter(), null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ReleasePublicIpWithInvalidVirtualDataCenterIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.ReleasePublicIp(INVALID_ID, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ReleasePublicIpWithInvalidPublicIpIdThrowsContractException()
        {
            // Arrange

            // Act
            sut.ReleasePublicIp(42, INVALID_ID);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "virtualDataCenter")]
        public void ReleasePublicIpWithNullVirtualDataCenterThrowsContractException()
        {
            // Arrange

            // Act
            sut.ReleasePublicIp(null, new PublicIp());

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "publicIp")]
        public void ReleasePublicIpWithNullPublicIpThrowsContractException()
        {
            // Arrange

            // Act
            sut.ReleasePublicIp(new VirtualDataCenter(), null);

            // Assert
        }

        #endregion Networks


        private class DummyAbiquoClient : BaseAbiquoClient
        {
            public override int TenantId
            {
                get { return 42; }
            }

            public DummyAbiquoClient()
            {
                AbiquoApiVersion = "Arbitrary-Version";
                TaskPollingWaitTimeMilliseconds = 10 * 1000;
                TaskPollingTimeoutMilliseconds = 10 * 1000;
            }

            public override bool Login(string abiquoApiBaseUri, IAuthenticationInformation authenticationInformation)
            {
                AbiquoApiBaseUri = new Uri(abiquoApiBaseUri).AbsoluteUri;
                AuthenticationInformation = authenticationInformation;
                CurrentUserInformation = new User();

                SessionToken = SESSION_TOKEN;

                IsLoggedIn = true;

                return true;
            }

            public override T InvokeLink<T>(Link link)
            {
                return default(T);
            }

            public override AbiquoBaseDto InvokeLink(Link link)
            {
                return new Enterprise();
            }

            public override Enterprises GetEnterprises()
            {
                return new Enterprises();
            }

            public override Enterprise GetCurrentEnterprise()
            {
                return new Enterprise();
            }

            public override Enterprise GetEnterprise(int id)
            {
                return new Enterprise();
            }

            public override UsersWithRoles GetUsersWithRolesOfCurrentEnterprise()
            {
                return new UsersWithRoles();
            }

            public override UsersWithRoles GetUsersWithRoles(Enterprise enterprise)
            {
                return new UsersWithRoles();
            }

            public override UsersWithRoles GetUsersWithRoles(int enterpriseId)
            {
                return new UsersWithRoles();
            }

            public override User GetUserOfCurrentEnterprise(int id)
            {
                return new User();
            }

            public override User GetUser(Enterprise enterprise, int id)
            {
                return new User();
            }

            public override User GetUser(int enterpriseId, int id)
            {
                return new User();
            }

            public override User GetUserInformation()
            {
                return new User();
            }

            public override User GetUserInformation(string username)
            {
                return new User();
            }

            public override User GetUserInformation(int enterpriseId, string username)
            {
                return new User();
            }

            public override void SwitchEnterprise(Enterprise enterprise)
            {
                // Intentionally do nothing
            }

            public override void SwitchEnterprise(int id)
            {
                // Intentionally do nothing
            }

            public override Roles GetRoles()
            {
                return new Roles();
            }

            public override Role GetRole(int id)
            {
                return new Role();
            }

            public override DataCentersLimits GetDataCentersLimitsOfCurrentEnterprise()
            {
                return new DataCentersLimits();
            }

            public override DataCentersLimits GetDataCentersLimits(Enterprise enterprise)
            {
                return new DataCentersLimits();
            }

            public override DataCentersLimits GetDataCentersLimits(int enterpriseId)
            {
                return new DataCentersLimits();
            }

            public override DataCenterLimits GetDataCenterLimitsOfCurrentEnterprise(int id)
            {
                return new DataCenterLimits();
            }

            public override DataCenterLimits GetDataCenterLimits(Enterprise enterprise, int id)
            {
                return new DataCenterLimits();
            }

            public override DataCenterLimits GetDataCenterLimits(int enterpriseId, int id)
            {
                return new DataCenterLimits();
            }

            public override VirtualMachines GetAllVirtualMachines()
            {
                return new VirtualMachines();
            }

            public override VirtualMachines GetVirtualMachines(VirtualAppliance virtualAppliance)
            {
                return new VirtualMachines();
            }

            public override VirtualMachine GetVirtualMachine(VirtualAppliance virtualAppliance, int id)
            {
                return new VirtualMachine();
            }

            public override VirtualMachines GetVirtualMachines(int virtualDataCenterId, int virtualApplianceId)
            {
                return new VirtualMachines();
            }

            public override VirtualMachine GetVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int id)
            {
                return new VirtualMachine();
            }

            public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId, int dataCenterRepositoryId,
                int virtualMachineTemplateId)
            {
                return new VirtualMachine();
            }

            public override VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate)
            {
                return new VirtualMachine();
            }

            public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref)
            {
                return new VirtualMachine();
            }

            public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId, int dataCenterRepositoryId,
                int virtualMachineTemplateId, VirtualMachineBase virtualMachine)
            {
                return new VirtualMachine();
            }

            public override VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate,
                VirtualMachine virtualMachine)
            {
                return new VirtualMachine();
            }

            public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref,
                VirtualMachineBase virtualMachine)
            {
                return new VirtualMachine();
            }

            public override Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force)
            {
                return new Task();
            }

            public override Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force)
            {
                return new Task();
            }

            public override Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion)
            {
                return new Task();
            }

            public override Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force, bool waitForCompletion)
            {
                return new Task();
            }

            public override Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force)
            {
                return new Task();
            }

            public override Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
                VirtualMachine virtualMachine, bool force)
            {
                return new Task();
            }

            public override Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion)
            {
                return new Task();
            }

            public override Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
                VirtualMachine virtualMachine, bool force, bool waitForCompletion)
            {
                return new Task();
            }

            public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state)
            {
                return new Task();
            }

            public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state)
            {
                return new Task();
            }

            public override Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
                VirtualMachineState state)
            {
                return new Task();
            }

            public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state, bool waitForCompletion)
            {
                return new Task();
            }

            public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state, bool waitForCompletion)
            {
                return new Task();
            }

            public override Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
                VirtualMachineState state, bool waitForCompletion)
            {
                return new Task();
            }

            public override void ProtectVirtualMachine(VirtualMachine virtualMachine)
            {
                // Intentionally do nothing
            }

            public override void ProtectVirtualMachine(VirtualMachine virtualMachine, string protectionCause)
            {
                // Intentionally do nothing
            }

            public override void ProtectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string protectionCause)
            {
                // Intentionally do nothing
            }

            public override void UnprotectVirtualMachine(VirtualMachine virtualMachine)
            {
                // Intentionally do nothing
            }

            public override void UnprotectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
            {
                // Intentionally do nothing
            }

            public override bool DeleteVirtualMachine(VirtualMachine virtualMachine)
            {
                return true;
            }

            public override VmNetworkConfigurations GetNetworkConfigurationsForVm(VirtualMachine virtualMachine)
            {
                return new VmNetworkConfigurations();
            }

            public override VmNetworkConfigurations GetNetworkConfigurationsForVm(int virtualDataCenterId, int virtualApplianceId, 
                int virtualMachineId)
            {
                return new VmNetworkConfigurations();
            }

            public override VmNetworkConfiguration GetNetworkConfigurationForVm(VirtualMachine virtualMachine, int id)
            {
                return new VmNetworkConfiguration();
            }

            public override VmNetworkConfiguration GetNetworkConfigurationForVm(int virtualDataCenterId, int virtualApplianceId,
                int virtualMachineId, int id)
            {
                return new VmNetworkConfiguration();
            }

            public override Nics GetNicsOfVirtualMachine(VirtualMachine virtualMachine)
            {
                return new Nics();
            }

            public override Nics GetNicsOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
            {
                return new Nics();
            }

            public override Tasks GetAllTasksOfVirtualMachine(VirtualMachine virtualMachine)
            {
                return new Tasks();
            }

            public override bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
            {
                return true;
            }

            public override bool DeleteVirtualMachine(VirtualMachine virtualMachine, bool force)
            {
                return true;
            }

            public override bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force)
            {
                return true;
            }

            public override Tasks GetAllTasksOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
            {
                return new Tasks();
            }

            public override Task GetTaskOfVirtualMachine(VirtualMachine virtualMachine, string taskId)
            {
                return new Task();
            }

            public override Task GetTaskOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string taskId)
            {
                return new Task();
            }

            public override VirtualMachineTemplates GetVirtualMachineTemplates(DataCenterRepository dataCenterRepository)
            {
                return new VirtualMachineTemplates();
            }

            public override VirtualMachineTemplates GetVirtualMachineTemplates(int enterpriseId, int dataCenterRepositoryId)
            {
                return new VirtualMachineTemplates();
            }

            public override VirtualMachineTemplate GetVirtualMachineTemplate(DataCenterRepository dataCenterRepository, int id)
            {
                return new VirtualMachineTemplate();
            }

            public override VirtualMachineTemplate GetVirtualMachineTemplate(int enterpriseId, int dataCenterRepositoryId, int id)
            {
                return new VirtualMachineTemplate();
            }

            public override VirtualDataCenters GetVirtualDataCenters()
            {
                return new VirtualDataCenters();
            }

            public override VirtualDataCenter GetVirtualDataCenter(int id)
            {
                return new VirtualDataCenter();
            }

            public override VirtualAppliances GetVirtualAppliances(VirtualDataCenter virtualDataCenter)
            {
                return new VirtualAppliances();
            }

            public override VirtualAppliances GetVirtualAppliances(int virtualDataCenterId)
            {
                return new VirtualAppliances();
            }

            public override VirtualAppliance GetVirtualAppliance(VirtualDataCenter virtualDataCenter, int id)
            {
                return new VirtualAppliance();
            }

            public override VirtualAppliance GetVirtualAppliance(int virtualDataCenterId, int id)
            {
                return new VirtualAppliance();
            }

            public override DataCenterRepositories GetDataCenterRepositoriesOfCurrentEnterprise()
            {
                return new DataCenterRepositories();
            }

            public override DataCenterRepositories GetDataCenterRepositories(Enterprise enterprise)
            {
                return new DataCenterRepositories();
            }

            public override DataCenterRepositories GetDataCenterRepositories(int enterpriseId)
            {
                return new DataCenterRepositories();
            }

            public override DataCenterRepository GetDataCenterRepositoryOfCurrentEnterprise(int id)
            {
                return new DataCenterRepository();
            }

            public override DataCenterRepository GetDataCenterRepository(Enterprise enterprise, int id)
            {
                return new DataCenterRepository();
            }

            public override DataCenterRepository GetDataCenterRepository(int enterpriseId, int id)
            {
                return new DataCenterRepository();
            }

            public override Task WaitForTaskCompletion(Task task, int taskPollingWaitTimeMilliseconds, int taskPollingTimeoutMilliseconds)
            {
                return new Task();
            }

            public override VlanNetworks GetPrivateNetworks(VirtualDataCenter virtualDataCenter)
            {
                return new VlanNetworks();
            }

            public override VlanNetworks GetPrivateNetworks(int virtualDataCenterId)
            {
                return new VlanNetworks();
            }

            public override VlanNetwork GetPrivateNetwork(VirtualDataCenter virtualDataCenter, int id)
            {
                return new VlanNetwork();
            }

            public override VlanNetwork GetPrivateNetwork(int virtualDataCenterId, int id)
            {
                return new VlanNetwork();
            }

            public override PrivateIps GetIpsOfPrivateNetwork(VlanNetwork vlan, bool free)
            {
                return new PrivateIps();
            }

            public override PrivateIps GetIpsOfPrivateNetwork(int virtualDataCenterId, int privateNetworkId, bool free)
            {
                return new PrivateIps();
            }

            public override VlanNetworks GetExternalNetworksOfCurrentEnterprise(int dataCenterLimitsId)
            {
                return new VlanNetworks();
            }

            public override VlanNetworks GetExternalNetworks(int enterpriseId, int dataCenterLimitsId)
            {
                return new VlanNetworks();
            }

            public override VlanNetwork GetExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int id)
            {
                return new VlanNetwork();
            }

            public override VlanNetwork GetExternalNetwork(int enterpriseId, int dataCenterLimitsId, int id)
            {
                return new VlanNetwork();
            }

            public override ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(VlanNetwork vlan, bool free)
            {
                return new ExternalIps();
            }

            public override ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int externalNetworkId, bool free)
            {
                return new ExternalIps();
            }

            public override ExternalIps GetIpsOfExternalNetwork(int enterpriseId, int dataCenterLimitsId, int externalNetworkId, bool free)
            {
                return new ExternalIps();
            }

            public override VlanNetworks GetPublicNetworks(VirtualDataCenter virtualDataCenter)
            {
                return new VlanNetworks();
            }

            public override VlanNetworks GetPublicNetworks(int virtualDataCenterId)
            {
                return new VlanNetworks();
            }

            public override VlanNetwork GetPublicNetwork(VirtualDataCenter virtualDataCenter, int id)
            {
                return new VlanNetwork();

            }

            public override VlanNetwork GetPublicNetwork(int virtualDataCenterId, int id)
            {
                return new VlanNetwork();
            }

            public override PublicIps GetPublicIpsToPurchaseOfPublicNetwork(VirtualDataCenter virtualDataCenter, VlanNetwork vlan)
            {
                return new PublicIps();
            }

            public override PublicIps GetPublicIpsToPurchaseOfPublicNetwork(int virtualDataCenterId, int vlanId)
            {
                return new PublicIps();
            }

            public override PublicIp PurchasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIp)
            {
                return new PublicIp();
            }

            public override PublicIp PurchasePublicIp(int virtualDataCenterId, int publicIpid)
            {
                return new PublicIp();
            }

            public override PublicIp ReleasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIpid)
            {
                return new PublicIp();
            }

            public override PublicIp ReleasePublicIp(int virtualDataCenterId, int publicIpid)
            {
                return new PublicIp();
            }
        }

        private class InvalidAbiquoClient : BaseAbiquoClient
        {
            public override int TenantId
            {
                get { return default(int); }
            }

            public InvalidAbiquoClient(string abiquoApiVersion, int taskPollingWaitTimeMilliseconds, int taskPollingTimeoutMilliseconds)
            {
                AbiquoApiVersion = abiquoApiVersion;
                TaskPollingWaitTimeMilliseconds = taskPollingWaitTimeMilliseconds;
                TaskPollingTimeoutMilliseconds = taskPollingTimeoutMilliseconds;
            }

            public override bool Login(string abiquoApiBaseUri, IAuthenticationInformation authenticationInformation)
            {
                return true;
            }

            public override T InvokeLink<T>(Link link)
            {
                throw new NotImplementedException();
            }

            public override AbiquoBaseDto InvokeLink(Link link)
            {
                throw new NotImplementedException();
            }

            public override Enterprises GetEnterprises()
            {
                throw new NotImplementedException();
            }

            public override Enterprise GetCurrentEnterprise()
            {
                throw new NotImplementedException();
            }

            public override Enterprise GetEnterprise(int id)
            {
                throw new NotImplementedException();
            }

            public override UsersWithRoles GetUsersWithRolesOfCurrentEnterprise()
            {
                throw new NotImplementedException();
            }

            public override UsersWithRoles GetUsersWithRoles(Enterprise enterprise)
            {
                throw new NotImplementedException();
            }

            public override UsersWithRoles GetUsersWithRoles(int enterpriseId)
            {
                throw new NotImplementedException();
            }

            public override User GetUserOfCurrentEnterprise(int id)
            {
                throw new NotImplementedException();
            }

            public override User GetUser(Enterprise enterprise, int id)
            {
                throw new NotImplementedException();
            }

            public override User GetUser(int enterpriseId, int id)
            {
                throw new NotImplementedException();
            }

            public override User GetUserInformation()
            {
                throw new NotImplementedException();
            }

            public override User GetUserInformation(string username)
            {
                throw new NotImplementedException();
            }

            public override User GetUserInformation(int enterpriseId, string username)
            {
                throw new NotImplementedException();
            }

            public override void SwitchEnterprise(Enterprise enterprise)
            {
                throw new NotImplementedException();
            }

            public override void SwitchEnterprise(int id)
            {
                throw new NotImplementedException();
            }

            public override Roles GetRoles()
            {
                throw new NotImplementedException();
            }

            public override Role GetRole(int id)
            {
                throw new NotImplementedException();
            }

            public override DataCentersLimits GetDataCentersLimitsOfCurrentEnterprise()
            {
                throw new NotImplementedException();
            }

            public override DataCentersLimits GetDataCentersLimits(Enterprise enterprise)
            {
                throw new NotImplementedException();
            }

            public override DataCentersLimits GetDataCentersLimits(int enterpriseId)
            {
                throw new NotImplementedException();
            }

            public override DataCenterLimits GetDataCenterLimitsOfCurrentEnterprise(int id)
            {
                throw new NotImplementedException();
            }

            public override DataCenterLimits GetDataCenterLimits(Enterprise enterprise, int id)
            {
                throw new NotImplementedException();
            }

            public override DataCenterLimits GetDataCenterLimits(int enterpriseId, int id)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachines GetAllVirtualMachines()
            {
                throw new NotImplementedException();
            }

            public override VirtualMachines GetVirtualMachines(VirtualAppliance virtualAppliance)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachine GetVirtualMachine(VirtualAppliance virtualAppliance, int id)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachines GetVirtualMachines(int virtualDataCenterId, int virtualApplianceId)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachine GetVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int id)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId,
                int dataCenterRepositoryId, int virtualMachineTemplateId)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int enterpriseId,
                int dataCenterRepositoryId, int virtualMachineTemplateId, VirtualMachineBase virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachine CreateVirtualMachine(VirtualAppliance virtualAppliance, VirtualMachineTemplate virtualMachineTemplate,
                VirtualMachine virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachine CreateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, string virtualMachineTemplateHref,
                VirtualMachineBase virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force)
            {
                throw new NotImplementedException();
            }

            public override Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force)
            {
                throw new NotImplementedException();
            }

            public override Task DeployVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion)
            {
                throw new NotImplementedException();
            }

            public override Task DeployVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force, bool waitForCompletion)
            {
                throw new NotImplementedException();
            }

            public override Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force)
            {
                throw new NotImplementedException();
            }

            public override Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
                VirtualMachine virtualMachine, bool force)
            {
                throw new NotImplementedException();
            }

            public override Task UpdateVirtualMachine(VirtualMachine virtualMachine, bool force, bool waitForCompletion)
            {
                throw new NotImplementedException();
            }

            public override Task UpdateVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
                VirtualMachine virtualMachine, bool force, bool waitForCompletion)
            {
                throw new NotImplementedException();
            }

            public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state)
            {
                throw new NotImplementedException();
            }

            public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state)
            {
                throw new NotImplementedException();
            }

            public override Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
                VirtualMachineState state)
            {
                throw new NotImplementedException();
            }

            public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineStateEnum state, bool waitForCompletion)
            {
                throw new NotImplementedException();
            }

            public override Task ChangeStateOfVirtualMachine(VirtualMachine virtualMachine, VirtualMachineState state, bool waitForCompletion)
            {
                throw new NotImplementedException();
            }

            public override Task ChangeStateOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId,
                VirtualMachineState state, bool waitForCompletion)
            {
                throw new NotImplementedException();
            }

            public override void ProtectVirtualMachine(VirtualMachine virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override void ProtectVirtualMachine(VirtualMachine virtualMachine, string protectionCause)
            {
                throw new NotImplementedException();
            }

            public override void ProtectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string protectionCause)
            {
                throw new NotImplementedException();
            }

            public override void UnprotectVirtualMachine(VirtualMachine virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override void UnprotectVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
            {
                throw new NotImplementedException();
            }

            public override bool DeleteVirtualMachine(VirtualMachine virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
            {
                throw new NotImplementedException();
            }

            public override bool DeleteVirtualMachine(VirtualMachine virtualMachine, bool force)
            {
                throw new NotImplementedException();
            }

            public override bool DeleteVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, bool force)
            {
                throw new NotImplementedException();
            }

            public override VmNetworkConfigurations GetNetworkConfigurationsForVm(VirtualMachine virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override VmNetworkConfigurations GetNetworkConfigurationsForVm(int virtualDataCenterId, int virtualApplianceId,
                int virtualMachineId)
            {
                throw new NotImplementedException();
            }

            public override VmNetworkConfiguration GetNetworkConfigurationForVm(VirtualMachine virtualMachine, int id)
            {
                throw new NotImplementedException();
            }

            public override VmNetworkConfiguration GetNetworkConfigurationForVm(int virtualDataCenterId, int virtualApplianceId,
                int virtualMachineId, int id)
            {
                throw new NotImplementedException();
            }

            public override Nics GetNicsOfVirtualMachine(VirtualMachine virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override Nics GetNicsOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
            {
                throw new NotImplementedException();
            }

            public override Tasks GetAllTasksOfVirtualMachine(VirtualMachine virtualMachine)
            {
                throw new NotImplementedException();
            }

            public override Tasks GetAllTasksOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId)
            {
                throw new NotImplementedException();
            }

            public override Task GetTaskOfVirtualMachine(VirtualMachine virtualMachine, string taskId)
            {
                throw new NotImplementedException();
            }

            public override Task GetTaskOfVirtualMachine(int virtualDataCenterId, int virtualApplianceId, int virtualMachineId, string taskId)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachineTemplates GetVirtualMachineTemplates(DataCenterRepository dataCenterRepository)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachineTemplates GetVirtualMachineTemplates(int enterpriseId, int dataCenterRepositoryId)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachineTemplate GetVirtualMachineTemplate(DataCenterRepository dataCenterRepository, int id)
            {
                throw new NotImplementedException();
            }

            public override VirtualMachineTemplate GetVirtualMachineTemplate(int enterpriseId, int dataCenterRepositoryId, int id)
            {
                throw new NotImplementedException();
            }

            public override VirtualDataCenters GetVirtualDataCenters()
            {
                throw new NotImplementedException();
            }

            public override VirtualDataCenter GetVirtualDataCenter(int id)
            {
                throw new NotImplementedException();
            }

            public override VirtualAppliances GetVirtualAppliances(VirtualDataCenter virtualDataCenter)
            {
                throw new NotImplementedException();
            }

            public override VirtualAppliances GetVirtualAppliances(int virtualDataCenterId)
            {
                throw new NotImplementedException();
            }

            public override VirtualAppliance GetVirtualAppliance(VirtualDataCenter virtualDataCenter, int id)
            {
                throw new NotImplementedException();
            }

            public override VirtualAppliance GetVirtualAppliance(int virtualDataCenterId, int id)
            {
                throw new NotImplementedException();
            }

            public override DataCenterRepositories GetDataCenterRepositoriesOfCurrentEnterprise()
            {
                throw new NotImplementedException();
            }

            public override DataCenterRepositories GetDataCenterRepositories(Enterprise enterprise)
            {
                throw new NotImplementedException();
            }

            public override DataCenterRepositories GetDataCenterRepositories(int enterpriseId)
            {
                throw new NotImplementedException();
            }

            public override DataCenterRepository GetDataCenterRepositoryOfCurrentEnterprise(int id)
            {
                throw new NotImplementedException();
            }

            public override DataCenterRepository GetDataCenterRepository(Enterprise enterprise, int id)
            {
                throw new NotImplementedException();
            }

            public override DataCenterRepository GetDataCenterRepository(int enterpriseId, int id)
            {
                throw new NotImplementedException();
            }

            public override Task WaitForTaskCompletion(Task task, int taskPollingWaitTimeMilliseconds, int taskPollingTimeoutMilliseconds)
            {
                throw new NotImplementedException();
            }

            public override VlanNetworks GetPrivateNetworks(VirtualDataCenter virtualDataCenter)
            {
                throw new NotImplementedException();
            }

            public override VlanNetworks GetPrivateNetworks(int virtualDataCenterId)
            {
                throw new NotImplementedException();
            }

            public override VlanNetwork GetPrivateNetwork(VirtualDataCenter virtualDataCenter, int id)
            {
                throw new NotImplementedException();
            }

            public override VlanNetwork GetPrivateNetwork(int virtualDataCenterId, int id)
            {
                throw new NotImplementedException();
            }

            public override PrivateIps GetIpsOfPrivateNetwork(VlanNetwork vlan, bool free)
            {
                throw new NotImplementedException();
            }

            public override PrivateIps GetIpsOfPrivateNetwork(int virtualDataCenterId, int privateNetworkId, bool free)
            {
                throw new NotImplementedException();
            }

            public override VlanNetworks GetExternalNetworksOfCurrentEnterprise(int dataCenterLimitsId)
            {
                throw new NotImplementedException();
            }

            public override VlanNetworks GetExternalNetworks(int enterpriseId, int dataCenterLimitsId)
            {
                throw new NotImplementedException();
            }

            public override VlanNetwork GetExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int id)
            {
                throw new NotImplementedException();
            }

            public override VlanNetwork GetExternalNetwork(int enterpriseId, int dataCenterLimitsId, int id)
            {
                throw new NotImplementedException();
            }

            public override ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(VlanNetwork vlan, bool free)
            {
                throw new NotImplementedException();
            }

            public override ExternalIps GetIpsOfExternalNetworkOfCurrentEnterprise(int dataCenterLimitsId, int externalNetworkId, bool free)
            {
                throw new NotImplementedException();
            }

            public override ExternalIps GetIpsOfExternalNetwork(int enterpriseId, int dataCenterLimitsId, int externalNetworkId, bool free)
            {
                throw new NotImplementedException();
            }

            public override VlanNetworks GetPublicNetworks(VirtualDataCenter virtualDataCenter)
            {
                throw new NotImplementedException();
            }

            public override VlanNetworks GetPublicNetworks(int virtualDataCenterId)
            {
                throw new NotImplementedException();
            }

            public override VlanNetwork GetPublicNetwork(VirtualDataCenter virtualDataCenter, int id)
            {
                throw new NotImplementedException();
            }

            public override VlanNetwork GetPublicNetwork(int virtualDataCenterId, int id)
            {
                throw new NotImplementedException();
            }

            public override PublicIps GetPublicIpsToPurchaseOfPublicNetwork(VirtualDataCenter virtualDataCenter, VlanNetwork vlan)
            {
                throw new NotImplementedException();
            }

            public override PublicIps GetPublicIpsToPurchaseOfPublicNetwork(int virtualDataCenterId, int vlanId)
            {
                throw new NotImplementedException();
            }

            public override PublicIp PurchasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIp)
            {
                throw new NotImplementedException();
            }

            public override PublicIp PurchasePublicIp(int virtualDataCenterId, int publicIpid)
            {
                throw new NotImplementedException();
            }

            public override PublicIp ReleasePublicIp(VirtualDataCenter virtualDataCenter, PublicIp publicIpid)
            {
                throw new NotImplementedException();
            }

            public override PublicIp ReleasePublicIp(int virtualDataCenterId, int publicIpid)
            {
                throw new NotImplementedException();
            }
        }
    }
}
