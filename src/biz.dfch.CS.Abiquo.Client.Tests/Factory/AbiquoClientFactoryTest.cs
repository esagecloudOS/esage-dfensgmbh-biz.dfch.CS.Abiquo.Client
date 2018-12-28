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
﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using biz.dfch.CS.Abiquo.Client.Factory;
using biz.dfch.CS.Testing.Attributes;

namespace biz.dfch.CS.Abiquo.Client.Tests.Factory
{
    [TestClass]
    public class AbiquoClientFactoryTest
    {
        [TestMethod]
        [ExpectContractFailure]
        public void GetByVersionWithNullVersionThrowsContractException()
        {
            // Arrange
            
            // Act
            var abiquoClient = AbiquoClientFactory.GetByVersion(null);

            // Assert
        }

        [TestMethod]
        public void GetByVersionWithValidVersionReturnsCorrespondingAbiquoClient()
        {
            // Arrange
            
            // Act
            var abiquoClient = AbiquoClientFactory.GetByVersion(AbiquoClientFactory.ABIQUO_CLIENT_VERSION_V1);
            
            // Assert
            Assert.IsNotNull(abiquoClient);
            Assert.AreEqual(typeof(AbiquoClient).FullName, abiquoClient.GetType().FullName);
        }

        [TestMethod]
        public void GetByVersionWithInvalidVersionReturnsNull()
        {
            // Arrange
            
            // Act
            var abiquoClient = AbiquoClientFactory.GetByVersion("vx");

            // Assert
            Assert.IsNull(abiquoClient);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetByCommitHashThrowsNotImplementedException()
        {
            // Arrange
            
            // Act
            var abiquoClient = AbiquoClientFactory.GetByCommitHash("hash");
            
            // Assert
        }
    }
}
