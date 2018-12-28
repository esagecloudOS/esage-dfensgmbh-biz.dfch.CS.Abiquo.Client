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

using biz.dfch.CS.Abiquo.Client.General;
﻿using biz.dfch.CS.Abiquo.Client.v1;
using biz.dfch.CS.Testing.Attributes;
﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace biz.dfch.CS.Abiquo.Client.Tests.v1.Model
{
    [TestClass]
    public class LinkTest
    {
        private const string ABIQUO_BASE_URI = "https://192.168.1.1:443/api/";

        [TestMethod]
        [ExpectContractFailure]
        public void GetUriSuffixForInvalidHrefThrowsContractException()
        {
            // Arrange
            var link = new LinkBuilder().BuildHref("InvalidHref").BuildRel(AbiquoRelations.SELF).GetLink();

            // Act
            link.GetUriSuffix();

            // Assert
        }

        [TestMethod]
        public void GetUriSuffixReturnsUriWithoutAbiquoBaseUri()
        {
            // Arrange
            var uriSuffix = "/admin/users/42";
            var href = UriHelper.ConcatUri(ABIQUO_BASE_URI, uriSuffix);
            var link = new LinkBuilder().BuildHref(href).BuildRel(AbiquoRelations.SELF).GetLink();

            // Act
            var resultingUriSuffix = link.GetUriSuffix();

            // Assert
            Assert.AreEqual(uriSuffix, resultingUriSuffix);
        }
    }
}
