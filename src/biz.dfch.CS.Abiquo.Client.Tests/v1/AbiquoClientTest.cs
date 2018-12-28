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
using System.Net.Http;
using System.Net.Http.Headers;
using biz.dfch.CS.Abiquo.Client.Authentication;
﻿using biz.dfch.CS.Abiquo.Client.Communication;
﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using biz.dfch.CS.Abiquo.Client.Factory;
﻿using biz.dfch.CS.Abiquo.Client.v1;
﻿using HttpMethod = biz.dfch.CS.Commons.Rest.HttpMethod;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using biz.dfch.CS.Commons.Rest;
using biz.dfch.CS.Testing.Attributes;

namespace biz.dfch.CS.Abiquo.Client.Tests.v1
{
    [TestClass]
    public class AbiquoClientTest
    {
        private const string ABIQUO_API_BASE_URI = "https://abiquo.example.com/api/";
        private const string USERNAME = "ArbitraryUsername";
        private const string PASSWORD = "ArbitraryPassword";
        private const string AUTH_COOKIE_VALUE = "auth=ABC123";

        private static readonly string SET_COOKIE_HEADER_VALUE_1 = string.Format("{0}; Expires=Fri, 31-Dec-2016 23:59:59 GMT; Path=/; Secure; HttpOnly", AUTH_COOKIE_VALUE);
        private const string SET_COOKIE_HEADER_VALUE_2 = "ABQSESSIONID=1234567891234567891; Expires=Fri, 30-Dec-2016 23:59:59 GMT; Path=/; Secure; HttpOnly";

        private delegate bool TryGetValuesDelegate(string name, out IEnumerable<string> values);

        [TestMethod]
        public void AbiquoClientVersionMatchesSpecifiedVersion()
        {
            // Arrange

            // Act
            var abiquoClient = new AbiquoClient();

            // Assert
            Assert.AreEqual(AbiquoClient.ABIQUO_API_VERSION, abiquoClient.AbiquoApiVersion);
        }

        [TestMethod]
        [ExpectContractFailure]
        public void LoginWithNullAuthenticationInformationThrowsContractException()
        {
            // Arrange
            var abiquoClient = AbiquoClientFactory.GetByVersion(AbiquoClientFactory.ABIQUO_CLIENT_VERSION_V1);

            // Act
            abiquoClient.Login(ABIQUO_API_BASE_URI, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void LoginWithNullAbiquoApiBaseUriThrowsContractException()
        {
            // Arrange
            var abiquoClient = AbiquoClientFactory.GetByVersion(AbiquoClientFactory.ABIQUO_CLIENT_VERSION_V1);
            var basicAuthInfo = new BasicAuthenticationInformation(USERNAME, PASSWORD);

            // Act
            abiquoClient.Login(null, basicAuthInfo);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void LoginWithEmptyAbiquoApiBaseUriThrowsContractException()
        {
            // Arrange
            var abiquoClient = AbiquoClientFactory.GetByVersion(AbiquoClientFactory.ABIQUO_CLIENT_VERSION_V1);
            var basicAuthInfo = new BasicAuthenticationInformation(USERNAME, PASSWORD);

            // Act
            abiquoClient.Login(" ", basicAuthInfo);

            // Assert
        }

        [TestMethod]
        public void LoginWithValidAuthenticationInformationReturnsTrue()
        {
            // Arrange
            var abiquoClient = AbiquoClientFactory.GetByVersion(AbiquoClientFactory.ABIQUO_CLIENT_VERSION_V1);

            var expectedRequestUri = string.Format("{0}{1}", ABIQUO_API_BASE_URI.TrimEnd('/'), AbiquoUriSuffixes.LOGIN);
            var basicAuthInfo = new BasicAuthenticationInformation(USERNAME, PASSWORD);
            var user = new User()
            {
                Nick = USERNAME
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
            Mock.Arrange(() => restCallExecutor
                .Invoke(HttpMethod.Get, expectedRequestUri, basicAuthInfo.GetAuthorizationHeaders(), null))
                    .IgnoreInstance()
                    .Returns(user.SerializeObject())
                    .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
                .IgnoreInstance()
                .Returns(responseHeaders)
                .OccursOnce();

            // Act
            var loginSucceeded = abiquoClient.Login(ABIQUO_API_BASE_URI, basicAuthInfo);
            
            // Assert
            Assert.IsTrue(loginSucceeded);

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void LoginWithInvalidAuthenticationInformationReturnsFalse()
        {
            // Arrange
            var abiquoClient = AbiquoClientFactory.GetByVersion(AbiquoClientFactory.ABIQUO_CLIENT_VERSION_V1);

            var expectedRequestUri = string.Format("{0}{1}", ABIQUO_API_BASE_URI.TrimEnd('/'), AbiquoUriSuffixes.LOGIN);
            var basicAuthInfo = new BasicAuthenticationInformation(USERNAME, PASSWORD);

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor
                .Invoke(HttpMethod.Get, expectedRequestUri, basicAuthInfo.GetAuthorizationHeaders(), null))
                    .IgnoreInstance()
                    .Throws<HttpRequestException>()
                    .OccursOnce();

            // Act
            var loginSucceeded = abiquoClient.Login(ABIQUO_API_BASE_URI, basicAuthInfo);

            // Assert
            Assert.IsFalse(loginSucceeded);

            Mock.Assert(restCallExecutor);
        }

        [TestMethod]
        public void LogoutResetsAuthenticationInformationApiBaseUriCurrentUserInformationSessionTokenAndSetsLoggedInToFalse()
        {
            // Arrange
            var responseHeaders = Mock.Create<HttpResponseHeaders>();

            var expectedRequestUri = string.Format("{0}{1}", ABIQUO_API_BASE_URI.TrimEnd('/'), AbiquoUriSuffixes.LOGIN);
            var abiquoClient = AbiquoClientFactory.GetByVersion(AbiquoClientFactory.ABIQUO_CLIENT_VERSION_V1);
            var basicAuthInfo = new BasicAuthenticationInformation(USERNAME, PASSWORD);
            var user = new User()
            {
                Nick = USERNAME
            };

            IEnumerable<string> cookieHeaderValues;
            Mock.Arrange(() => responseHeaders.TryGetValues(AbiquoHeaderKeys.SET_COOKIE_HEADER_KEY, out cookieHeaderValues))
                .Returns(new TryGetValuesDelegate((string name, out IEnumerable<string> values) =>
                {
                    values = new List<string>() {SET_COOKIE_HEADER_VALUE_1, SET_COOKIE_HEADER_VALUE_2};

                    return true;
                }))
                .OccursOnce();

            var restCallExecutor = Mock.Create<RestCallExecutor>();
            Mock.Arrange(() => restCallExecutor
                .Invoke(HttpMethod.Get, expectedRequestUri, basicAuthInfo.GetAuthorizationHeaders(), null))
                    .IgnoreInstance()
                    .Returns(user.SerializeObject())
                    .OccursOnce();

            Mock.Arrange(() => restCallExecutor.GetResponseHeaders())
               .IgnoreInstance()
               .Returns(responseHeaders)
               .OccursOnce();

            // Act
            var loginSucceeded = abiquoClient.Login(ABIQUO_API_BASE_URI, basicAuthInfo);

            Assert.IsTrue(loginSucceeded);
            Assert.AreEqual(true, abiquoClient.IsLoggedIn);
            Assert.AreEqual(basicAuthInfo, abiquoClient.AuthenticationInformation);
            Assert.AreEqual(USERNAME, user.Nick);
            Assert.AreEqual(ABIQUO_API_BASE_URI, abiquoClient.AbiquoApiBaseUri);
            Assert.AreEqual(AUTH_COOKIE_VALUE, abiquoClient.SessionToken);

            abiquoClient.Logout();

            // Assert
            Assert.AreEqual(false, abiquoClient.IsLoggedIn);
            Assert.IsNull(abiquoClient.AuthenticationInformation);
            Assert.IsNull(abiquoClient.AbiquoApiBaseUri);
            Assert.IsNull(abiquoClient.CurrentUserInformation);
            Assert.IsNull(abiquoClient.SessionToken);

            Mock.Assert(responseHeaders);
            Mock.Assert(restCallExecutor);
        }
    }
}
