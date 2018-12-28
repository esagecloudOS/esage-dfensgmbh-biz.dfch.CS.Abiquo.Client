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
using System.Diagnostics.Contracts;

namespace biz.dfch.CS.Abiquo.Client.General
{
    public class FilterBuilder
    {
        private Dictionary<string, object> _filter;

        public FilterBuilder()
        {
            _filter = new Dictionary<string, object>();
        }

        public FilterBuilder BuildFilterPart(string filterKey, object filterValue)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filterKey));
            Contract.Requires(null != filterValue);

            _filter.Add(filterKey, filterValue);

            return this;
        }

        public Dictionary<string, object> GetFilter()
        {
            Contract.Ensures(null != Contract.Result<Dictionary<string, object>>());

            return _filter;
        }
    }
}
