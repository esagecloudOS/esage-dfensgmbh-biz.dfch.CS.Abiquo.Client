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
﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using biz.dfch.CS.Abiquo.Client.General;
using biz.dfch.CS.Testing.Attributes;

namespace biz.dfch.CS.Abiquo.Client.Tests.General
{
    [TestClass]
    public class UriHelperTest
    {
        private const string ABIQUO_API_BASE_URI = "https://abiquo.example.com/api/";
        private const string ENTERPRISE_HREF = "https://abiquo.example.com/api/admin/enterprises/1";

        [TestMethod]
        [ExpectContractFailure]
        public void ConcatUriWithInvalidBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ConcatUri(null, AbiquoUriSuffixes.LOGIN);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ConcatUriWithEmptyBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ConcatUri(" ", AbiquoUriSuffixes.LOGIN);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ConcatUriWithNullUriSuffixThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ConcatUri(ABIQUO_API_BASE_URI, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ConcatUriWithEmptyUriSuffixThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ConcatUri(ABIQUO_API_BASE_URI, " ");
            
            // Assert
        }

        [TestMethod]
        public void ConcatUriReturnsValidUri()
        {
            // Arrange
            var expectedUri = "http://abiquo.example.com/api/login";

            // Act

            // Assert
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://abiquo.example.com/api/", "login"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://abiquo.example.com/api/", "/login"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://abiquo.example.com/api/", "login/"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://abiquo.example.com/api/", "/login/"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://abiquo.example.com/api", "login"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://abiquo.example.com/api", "/login"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://abiquo.example.com/api", "login/"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://abiquo.example.com/api", "/login/"));
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateFilterStringWithNullDictionaryThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.CreateFilterString(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void CreateFilterStringWithEmptyDictionaryThrowsContractException()
        {
            // Arrange
            var emptyFilter = new Dictionary<string, object>();

            // Act
            UriHelper.CreateFilterString(emptyFilter);

            // Assert
        }

        [TestMethod]
        public void CreateFilterStringWithReturnsFilterAsString()
        {
            // Arrange
            var expectedFilterString = "maxSize=5&currentPage=1&limit=25";

            var filter = new Dictionary<string, object>()
            {
                {"maxSize", 5},
                {"currentPage", 1},
                {"limit", "25"}
            };

            // Act
            var filterString = UriHelper.CreateFilterString(filter);

            // Assert
            Assert.AreEqual(expectedFilterString, filterString);
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ExtractIdAsIntWithNullValueThrowsContractException()
        {
            // Arrange
            
            // Act
            UriHelper.ExtractIdAsInt(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ExtractIdAsIntWithInvalidUriStringThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractIdAsInt("Arbitrary");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ExtractIdAsIntWithUriNotContainigIdThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractIdAsInt(ABIQUO_API_BASE_URI);

            // Assert
        }

        [TestMethod]
        public void ExtractIdAsIntWithUriContainigIdSucceeds()
        {
            // Arrange

            // Act
            var id = UriHelper.ExtractIdAsInt("https://abiquo.example.com/api/users/155");

            // Assert
            Assert.AreEqual(155, id);
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ExtractLastSegmentAsStringWithNullValueThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractLastSegmentAsString(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void ExtractLastSegmentAsStringWithInvalidUriStringThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractLastSegmentAsString("Arbitrary");

            // Assert
        }

        [TestMethod]
        public void ExtractLastSegmentAsStringWithValidUriStringSucceeds()
        {
            // Arrange

            // Act
            var lastSegment = UriHelper.ExtractLastSegmentAsString("https://abiquo.example.com/api/users/fe5ddc9e-7745-4a4a-99d6-d7598682f8fd");

            // Assert
            Assert.AreEqual("fe5ddc9e-7745-4a4a-99d6-d7598682f8fd", lastSegment);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "baseUri")]
        public void ExtractRelativeUriWithNullBaseUriStringThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractRelativeUri(null, ENTERPRISE_HREF);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "baseUri")]
        public void ExtractRelativeUriWithEmptyBaseUriStringThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractRelativeUri("", ENTERPRISE_HREF);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "Invalid")]
        public void ExtractRelativeUriWithInvalidBaseUriStringThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractRelativeUri("Arbitrary", ENTERPRISE_HREF);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "absoluteUri")]
        public void ExtractRelativeUriWithNullAbsoluteUriStringThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractRelativeUri(ABIQUO_API_BASE_URI, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "absoluteUri")]
        public void ExtractRelativeUriWithEmptyAbsoluteUriStringThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractRelativeUri(ABIQUO_API_BASE_URI, "");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "Invalid")]
        public void ExtractRelativeUriWithInvalidAbsoluteUriStringThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractRelativeUri(ABIQUO_API_BASE_URI, "Arbitrary");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "IsBaseOf")]
        public void ExtractRelativeUriWithBaseUriIsBaseOfAbsoluteUriThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ExtractRelativeUri(ABIQUO_API_BASE_URI, "http://arbitrary.example.com/api/admin/enterprises/1");

            // Assert
        }

        [TestMethod]
        public void ExtractRelativeUriSucceeds()
        {
            // Arrange

            // Act
            var result = UriHelper.ExtractRelativeUri(ABIQUO_API_BASE_URI, ENTERPRISE_HREF);

            // Assert
            Assert.AreEqual("admin/enterprises/1", result);
        }

        [TestMethod]
        public void ExtractRelativeUriSucceeds2()
        {
            // Arrange

            // Act
            var result = UriHelper.ExtractRelativeUri(ABIQUO_API_BASE_URI.TrimEnd('/'), ENTERPRISE_HREF);

            // Assert
            Assert.AreEqual("/admin/enterprises/1", result);
        }

        [TestMethod]
        public void ExtractRelativeUriFromAbsoluteUriWithPortSucceeds()
        {
            // Arrange

            // Act
            var result = UriHelper.ExtractRelativeUri(ABIQUO_API_BASE_URI, "https://abiquo.example.com:443/api/admin/enterprises/1");

            // Assert
            Assert.AreEqual("admin/enterprises/1", result);
        }
    }
}
