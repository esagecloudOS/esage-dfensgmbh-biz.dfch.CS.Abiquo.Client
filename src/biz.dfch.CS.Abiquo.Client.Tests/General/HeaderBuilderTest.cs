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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using biz.dfch.CS.Abiquo.Client.General;
using biz.dfch.CS.Abiquo.Client.v1;
using biz.dfch.CS.Abiquo.Client.Communication;
using biz.dfch.CS.Testing.Attributes;

namespace biz.dfch.CS.Abiquo.Client.Tests.General
{
    [TestClass]
    public class HeaderBuilderTest
    {
        [ExpectContractFailure]
        [TestMethod]
        public void BuildAcceptHeaderWithNullValueThrowsContractException()
        {
            // Arrange

            // Act
            new HeaderBuilder().BuildAccept(null);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildAcceptHeaderWithEmptyValueThrowsContractException()
        {
            // Arrange

            // Act
            new HeaderBuilder().BuildAccept(" ");

            // Assert
        }

        [TestMethod]
        public void BuildAcceptHeaderCreatesDictionaryContainingExpectedAcceptHeader()
        {
            // Arrange

            // Act
            var headers = new HeaderBuilder().BuildAccept(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISE).GetHeaders();

            // Assert
            Assert.AreEqual(1, headers.Count);
            Assert.IsTrue(headers.ContainsKey(AbiquoHeaderKeys.ACCEPT_HEADER_KEY));
            Assert.AreEqual(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISE, headers[AbiquoHeaderKeys.ACCEPT_HEADER_KEY]);
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildContentTypeHeaderWithNullValueThrowsContractException()
        {
            // Arrange

            // Act
            new HeaderBuilder().BuildContentType(null);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildContentTypeHeaderWithEmptyValueThrowsContractException()
        {
            // Arrange

            // Act
            new HeaderBuilder().BuildContentType(" ");

            // Assert
        }

        [TestMethod]
        public void BuildContentTypeHeaderCreatesDictionaryContainingExpectedAcceptHeader()
        {
            // Arrange

            // Act
            var headers = new HeaderBuilder().BuildContentType(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISE).GetHeaders();

            // Assert
            Assert.AreEqual(1, headers.Count);
            Assert.IsTrue(headers.ContainsKey(AbiquoHeaderKeys.CONTENT_TYPE_HEADER_KEY));
            Assert.AreEqual(VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISE, headers[AbiquoHeaderKeys.CONTENT_TYPE_HEADER_KEY]);
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildCustomWithNullHeaderKeyThrowsContractException()
        {
            // Arrange

            // Act
            new HeaderBuilder().BuildCustom(null, VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISE);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildCustomWithEmptyHeaderKeyThrowsContractException()
        {
            // Arrange

            // Act
            new HeaderBuilder().BuildCustom(" ", VersionedAbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISE);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildCustomWithNullHeaderValueThrowsContractException()
        {
            // Arrange

            // Act
            new HeaderBuilder().BuildCustom(AbiquoHeaderKeys.ACCEPT_HEADER_KEY, null);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildCustomWithEmptyHeaderValueThrowsContractException()
        {
            // Arrange

            // Act
            new HeaderBuilder().BuildCustom(AbiquoHeaderKeys.ACCEPT_HEADER_KEY, " ");

            // Assert
        }

        [TestMethod]
        public void BuildCustomCreatesDictionaryContainingExpectedHeader()
        {
            // Arrange
            var headerKey = "Arbitrary-Key";
            var headerValue = "ArbitraryValue";

            // Act
            var headers = new HeaderBuilder().BuildCustom(headerKey, headerValue).GetHeaders();

            // Assert
            Assert.AreEqual(1, headers.Count);
            Assert.IsTrue(headers.ContainsKey(headerKey));
            Assert.AreEqual(headerValue, headers[headerKey]);
        }
    }
}
