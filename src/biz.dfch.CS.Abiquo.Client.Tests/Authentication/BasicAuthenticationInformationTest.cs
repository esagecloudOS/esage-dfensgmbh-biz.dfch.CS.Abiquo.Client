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

using biz.dfch.CS.Abiquo.Client.Authentication;
using biz.dfch.CS.Testing.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace biz.dfch.CS.Abiquo.Client.Tests.Authentication
{
    [TestClass]
    public class BasicAuthenticationInformationTest
    {
        private const string USERNAME = "ArbitraryUsername";
        private const string PASSWORD = "ArbitraryPassword";

        [TestMethod]
        [ExpectContractFailure]
        public void CreateBasicAuthenticationInformationWithNullUsernameThrowsContractException()
        {
            // Arrange
            
            // Act
            new BasicAuthenticationInformation(null, PASSWORD);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateBasicAuthenticationInformationWithEmptyUsernameThrowsContractException()
        {
            // Arrange
            
            // Act
            new BasicAuthenticationInformation(" ", PASSWORD);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateBasicAuthenticationInformationWithNullPasswordThrowsContractException()
        {
            // Arrange
            
            // Act
            new BasicAuthenticationInformation(USERNAME, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateBasicAuthenticationInformationWithEmptyPasswordThrowsContractException()
        {
            // Arrange
            
            // Act
            new BasicAuthenticationInformation(USERNAME, " ");

            // Assert
        }

        [TestMethod]
        public void GetAuthenticationHeadersReturnsBasicAuthenticationHeader()
        {
            // Arrange
            var basicAuthInfo = new BasicAuthenticationInformation(USERNAME, PASSWORD);

            // Act
            var authHeaders = basicAuthInfo.GetAuthorizationHeaders();

            // Assert
            Assert.IsNotNull(authHeaders);
            Assert.AreEqual(1, authHeaders.Keys.Count);

            Assert.AreEqual("Basic QXJiaXRyYXJ5VXNlcm5hbWU6QXJiaXRyYXJ5UGFzc3dvcmQ=", authHeaders[Client.Constants.Authentication.AUTHORIZATION_HEADER_KEY]);
        }
    }
}
