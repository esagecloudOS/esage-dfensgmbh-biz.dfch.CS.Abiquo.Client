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
using biz.dfch.CS.Testing.Attributes;

namespace biz.dfch.CS.Abiquo.Client.Tests.General
{
    [TestClass]
    public class FilterBuilderTest
    {
        private const string FORCE_FILTER_KEY = "force";
        private const string FORCE_FILTER_VALUE = "force";

        [TestMethod]
        [ExpectContractFailure]
        public void BuildFilterPartWithNullFilterKeyThrowsContractException()
        {
            // Arrange

            // Act
            new FilterBuilder().BuildFilterPart(null, 42);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void BuildFilterPartWithEmptyFilterKeyThrowsContractException()
        {
            // Arrange

            // Act
            new FilterBuilder().BuildFilterPart(" ", FORCE_FILTER_VALUE);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure]
        public void BuildFilterPartWithNullFilterValueThrowsContractException()
        {
            // Arrange

            // Act
            new FilterBuilder().BuildFilterPart(FORCE_FILTER_KEY, null);

            // Assert
        }

        [TestMethod]
        public void BuildFilterReturnsDictionaryContainingExpectedFilterParts()
        {
            // Arrange

            // Act
            var filter = new FilterBuilder().BuildFilterPart(FORCE_FILTER_KEY, FORCE_FILTER_VALUE).GetFilter();

            // Assert
            Assert.AreEqual(1, filter.Count);
            Assert.IsTrue(filter.ContainsKey(FORCE_FILTER_KEY));
            Assert.AreEqual(FORCE_FILTER_VALUE, filter[FORCE_FILTER_KEY]);
        }
    }
}
