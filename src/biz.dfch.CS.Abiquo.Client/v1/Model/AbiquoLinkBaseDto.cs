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

namespace biz.dfch.CS.Abiquo.Client.v1.Model
{
    /// <summary>
    /// Base DTO for Abiquo objects that contains Id, Name and Links
    /// </summary>
    public abstract class AbiquoLinkBaseDto : AbiquoV1BaseDto
    {
        public List<Link> Links { get; set; }

        public Link GetLinkByRel(string rel)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(rel));
            Contract.Ensures(null != Contract.Result<Link>());

            return Links.Find(l => l.Rel == rel);
        }

        public ICollection<Link> GetLinksByType(string type)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(type));
            Contract.Ensures(null != Contract.Result<ICollection<Link>>());

            return Links.FindAll(l => l.Type == type);
        }
    }
}
