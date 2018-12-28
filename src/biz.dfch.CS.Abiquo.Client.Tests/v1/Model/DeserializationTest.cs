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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using biz.dfch.CS.Abiquo.Client.v1.Model;
using System.IO;
using biz.dfch.CS.Abiquo.Client.General;

namespace biz.dfch.CS.Abiquo.Client.Tests.v1.Model
{
    [TestClass]
    public class DeserializationTest
    {
        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void DeserializationOfEnterpriseSucceeds()
        {
            // Arrange
            var enterpriseJsonFileContent = File.ReadAllText(@"..\..\JsonSamples\Enterprise.json");

            // Act
            var enterprise = AbiquoBaseDto.DeserializeObject<Enterprise>(enterpriseJsonFileContent);

            // Assert
            Assert.IsNotNull(enterprise.Links);
            Assert.AreEqual(17, enterprise.Links.Count);
            Assert.AreEqual(1, enterprise.Id);
            Assert.AreEqual("Arbitrary Enterprise", enterprise.Name);
            Assert.IsFalse(enterprise.Workflow);
            Assert.IsFalse(enterprise.TwoFactorAuthenticationMandatory);
            Assert.AreEqual(0, enterprise.RamSoftLimitInMb);
            Assert.AreEqual(0, enterprise.RamHardLimitInMb);
            Assert.IsFalse(enterprise.IsReservationRestricted);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void DeserializationOfEnterprisesSucceeds()
        {
            // Arrange
            var enterprisesJsonFileContent = File.ReadAllText(@"..\..\JsonSamples\Enterprises.json");

            // Act
            var enterprises = AbiquoBaseDto.DeserializeObject<Enterprises>(enterprisesJsonFileContent);

            // Assert
            Assert.IsNotNull(enterprises.Links);
            Assert.AreEqual(2, enterprises.Links.Count);
            Assert.IsNotNull(enterprises.Collection);
            Assert.AreEqual(enterprises.TotalSize, enterprises.Collection.Count);

            var enterprise = enterprises.Collection.FirstOrDefault();
            Assert.IsNotNull(enterprise);
            Assert.AreEqual(1, enterprise.Id);
            Assert.AreEqual("Arbitrary Enterprise", enterprise.Name);
            Assert.AreEqual(0, enterprise.RepositorySoftInMb);
            Assert.AreEqual(0, enterprise.RepositoryHardInMb);
            Assert.IsFalse(enterprise.Workflow);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void DeserializationOfUserSucceeds()
        {
            // Arrange
            var userJsonFileContent = File.ReadAllText(@"..\..\JsonSamples\User.json");

            // Act
            var user = AbiquoBaseDto.DeserializeObject<User>(userJsonFileContent);

            // Assert
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Links);
            Assert.AreEqual(6, user.Links.Count);

            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("Cloud", user.Name);
            Assert.AreEqual("en_US", user.Locale);
            Assert.IsTrue(user.Active);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void DeserializationOfUsersWithRolesSucceeds()
        {
            // Arrange
            var usersWithRolesJsonFileContent = File.ReadAllText(@"..\..\JsonSamples\UsersWithRoles.json");

            // Act
            var usersWithRoles = AbiquoBaseDto.DeserializeObject<UsersWithRoles>(usersWithRolesJsonFileContent);

            // Assert
            Assert.IsNotNull(usersWithRoles.Links);
            Assert.AreEqual(2, usersWithRoles.Links.Count);
            Assert.IsNotNull(usersWithRoles.Collection);
            Assert.AreEqual(usersWithRoles.TotalSize, usersWithRoles.Collection.Count);

            var userWithRoles = usersWithRoles.Collection.FirstOrDefault();
            Assert.IsNotNull(userWithRoles);
            Assert.AreEqual(1, userWithRoles.Id);
            Assert.AreEqual("Cloud", userWithRoles.Name);
            Assert.AreEqual("", userWithRoles.Email);
            Assert.AreEqual("en_US", userWithRoles.Locale);
            Assert.IsFalse(userWithRoles.FirstLogin);
            Assert.IsTrue(userWithRoles.Active);

            var enterprise = userWithRoles.Enterprise;
            Assert.IsNotNull(enterprise);
            Assert.AreEqual(1, enterprise.Id);
            Assert.AreEqual("Arbitrary Enterprise", enterprise.Name);
            Assert.AreEqual(0, enterprise.RepositorySoftInMb);
            Assert.AreEqual(0, enterprise.RepositoryHardInMb);
            Assert.IsFalse(enterprise.Workflow);

            var role = userWithRoles.Role;
            Assert.IsNotNull(role);
            Assert.IsFalse(role.Blocked);
            Assert.AreEqual(1, role.Id);
            Assert.AreEqual("CLOUD_ADMIN", role.Name);

            var privileges = role.Privileges;
            Assert.IsNotNull(privileges);
            Assert.IsNotNull(privileges.Links);
            Assert.AreEqual(0, privileges.Links.Count);
            Assert.IsNotNull(privileges.Collection);
            Assert.AreEqual(83, privileges.Collection.Count);

            var privilege = role.Privileges.Collection.FirstOrDefault();
            Assert.IsNotNull(privilege);
            Assert.IsNotNull(privilege.Links);
            Assert.AreEqual(1, privilege.Links.Count);
            Assert.AreEqual(1, privilege.Id);
            Assert.AreEqual("ENTERPRISE_ENUMERATE", privilege.Name);
        }
    }
}
