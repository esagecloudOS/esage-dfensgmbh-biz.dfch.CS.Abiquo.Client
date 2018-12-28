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
using biz.dfch.CS.Abiquo.Client.v1;
﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using biz.dfch.CS.Testing.Attributes;

namespace biz.dfch.CS.Abiquo.Client.Tests.v1.Model
{
    [TestClass]
    public class AbiquoLinkBaseDtoTest
    {
        private const string USERS_HREF = "https://abiquo.example.com/api/admin/enterprises/1/users";
        private const string PROPERTIES_HREF = "https://abiquo.example.com/api/admin/enterprises/1/properties";

        [TestMethod]
        [ExpectContractFailure]
        public void GetLinkByRelWithNullRelThrowsContractException()
        {
            // Arrange
            var enterprise = CreateEnterpriseWithLinks();

            // Act
            enterprise.GetLinkByRel(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetLinkByRelWithInexistentRelThrowsContractException()
        {
            // Arrange
            var enterprise = CreateEnterpriseWithLinks();

            // Act
            enterprise.GetLinkByRel("Arbitrary");

            // Assert
        }

        [TestMethod]
        public void GetLinkByRelWithExistingRelReturnsExpectedLink()
        {
            // Arrange
            var enterprise = CreateEnterpriseWithLinks();

            // Act
            var usersLink = enterprise.GetLinkByRel(AbiquoRelations.USERS);

            // Assert
            Assert.IsNotNull(usersLink);
            Assert.AreEqual(AbiquoRelations.USERS, usersLink.Rel);
            Assert.AreEqual(USERS_HREF, usersLink.Href);
            Assert.AreEqual(AbiquoRelations.USERS, usersLink.Title);
            Assert.AreEqual(AbiquoMediaDataTypes.VND_ABIQUO_USERS, usersLink.Type);
        }

        [TestMethod]
        [ExpectContractFailure]
        public void GetLinksByTypeWithNullTypeThrowsContractException()
        {
            // Arrange
            var enterprise = CreateEnterpriseWithLinks();

            // Act
            enterprise.GetLinksByType(null);

            // Assert
        }

        [TestMethod]
        public void GetLinksByTypeWithInexistentTypeReturnsEmptyLinkCollection()
        {
            // Arrange
            var enterprise = CreateEnterpriseWithLinks();

            // Act
            var links = enterprise.GetLinksByType(AbiquoMediaDataTypes.VND_ABIQUO_BACKUP);

            // Assert
            Assert.IsNotNull(links);
            Assert.IsTrue(0 == links.Count);
        }

        [TestMethod]
        public void GetLinksByTypeReturnsLinkCollectionContainingLinksOfSpecifiedType()
        {
            // Arrange
            var enterprise = CreateEnterpriseWithLinks();

            // Act
            var links = enterprise.GetLinksByType(AbiquoMediaDataTypes.VND_ABIQUO_USERS);

            // Assert
            Assert.IsNotNull(links);
            Assert.IsTrue(1 == links.Count);
        }

        [TestMethod]
        public void GetLinksByTypeReturnsLinkCollectionContainingLinksOfSpecifiedType2()
        {
            // Arrange
            var enterprise = CreateEnterpriseWithLinks();
            enterprise.Links.Add(CreateUsersLink());

            // Act
            var links = enterprise.GetLinksByType(AbiquoMediaDataTypes.VND_ABIQUO_USERS);

            // Assert
            Assert.IsNotNull(links);
            Assert.IsTrue(2 == links.Count);
        }

        private Enterprise CreateEnterpriseWithLinks()
        {
            var propertiesLink = new LinkBuilder()
                .BuildRel(AbiquoRelations.PROPERTIES)
                .BuildHref(PROPERTIES_HREF)
                .BuildType(AbiquoMediaDataTypes.VND_ABIQUO_ENTERPRISEPROPERTIES)
                .BuildTitle(AbiquoRelations.PROPERTIES)
                .GetLink();

            var enterprise = new Enterprise()
            {
                Id = 42
                ,
                Name = "Arbitrary Enterprise"
                ,
                Links = new List<Link>() { CreateUsersLink(), propertiesLink }
            };

            return enterprise;
        }

        private Link CreateUsersLink()
        {
            return new LinkBuilder()
                .BuildRel(AbiquoRelations.USERS)
                .BuildHref(USERS_HREF)
                .BuildType(AbiquoMediaDataTypes.VND_ABIQUO_USERS)
                .BuildTitle(AbiquoRelations.USERS).GetLink();
        }
    }
}
