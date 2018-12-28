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
using biz.dfch.CS.Abiquo.Client.v1;
using biz.dfch.CS.Testing.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace biz.dfch.CS.Abiquo.Client.Tests.v1
{
    [TestClass]
    public class LinkBuilderTest
    {
        private const string HREF = "http://example.com/api/users/1";
        private const string TITLE = "users";

        [ExpectContractFailure]
        [TestMethod]
        public void BuildHrefWithNullStringThrowsContractException()
        {
            // Arrange

            // Act
            new LinkBuilder().BuildHref(href: null);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildHrefWithNullUriThrowsContractException()
        {
            // Arrange

            // Act
            new LinkBuilder().BuildHref(uri: null);

            // Assert
        }

        [TestMethod]
        public void BuildHrefWithValidUriSucceeds()
        {
            // Arrange

            // Act
            new LinkBuilder().BuildHref(new Uri(HREF));

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildRelWithNullValueThrowsContractException()
        {
            // Arrange

            // Act
            new LinkBuilder().BuildRel(null);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildTitleWithNullValueThrowsContractException()
        {
            // Arrange

            // Act
            new LinkBuilder().BuildTitle(null);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildTypeWithNullValueThrowsContractException()
        {
            // Arrange

            // Act
            new LinkBuilder().BuildType(null);

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildLinkWithoutRequiredRelThrowsContractException()
        {
            // Arrange

            // Act
            new LinkBuilder().BuildHref(HREF).GetLink();

            // Assert
        }

        [ExpectContractFailure]
        [TestMethod]
        public void BuildLinkWithoutRequiredHrefThrowsContractException()
        {
            // Arrange

            // Act
            new LinkBuilder().BuildRel(AbiquoRelations.VIRTUALMACHINETEMPLATE).GetLink();

            // Assert
        }

        [TestMethod]
        public void BuildLinkWithRequiredPropertiesSucceeds()
        {
            // Arrange

            // Act
            var link = new LinkBuilder().BuildHref(HREF).BuildRel(AbiquoRelations.VIRTUALMACHINETEMPLATE).GetLink();

            // Assert
            Assert.IsNotNull(link);
            Assert.AreEqual(HREF, link.Href);
            Assert.AreEqual(AbiquoRelations.VIRTUALMACHINETEMPLATE, link.Rel);
            Assert.IsNull(link.Title);
            Assert.IsNull(link.Type);
        }

        [TestMethod]
        public void BuildLinkWithAllPossiblePropertiesSucceeds()
        {
            // Arrange

            // Act
            var link = new LinkBuilder()
                .BuildHref(HREF)
                .BuildRel(AbiquoRelations.VIRTUALMACHINETEMPLATE)
                .BuildTitle(TITLE)
                .BuildType(VersionedAbiquoMediaDataTypes.VND_ABIQUO_USER)
                .GetLink();

            // Assert
            Assert.IsNotNull(link);
            Assert.AreEqual(HREF, link.Href);
            Assert.AreEqual(AbiquoRelations.VIRTUALMACHINETEMPLATE, link.Rel);
            Assert.AreEqual(TITLE, link.Title);
            Assert.AreEqual(VersionedAbiquoMediaDataTypes.VND_ABIQUO_USER, link.Type);
        }
    }
}
