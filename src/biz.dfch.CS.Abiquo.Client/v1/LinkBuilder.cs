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
using System.Diagnostics.Contracts;
using biz.dfch.CS.Abiquo.Client.v1.Model;

namespace biz.dfch.CS.Abiquo.Client.v1
{
    public class LinkBuilder
    {
        private Link _link;

        public LinkBuilder()
        {
            _link = new Link();
        }

        public LinkBuilder BuildRel(string rel)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(rel));

            _link.Rel = rel;
            return this;
        }

        public LinkBuilder BuildHref(string href)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(href));

            _link.Href = href;
            return this;
        }
        
        public LinkBuilder BuildHref(Uri uri)
        {
            Contract.Requires(null != uri);

            _link.Href = uri.AbsoluteUri;
            return this;
        }

        public LinkBuilder BuildTitle(string title)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));

            _link.Title = title;
            return this;
        }
        
        public LinkBuilder BuildType(string type)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(type));

            _link.Type = type;
            return this;
        }

        public Link GetLink()
        {
            Contract.Ensures(null != Contract.Result<Link>());
            Contract.Ensures(Contract.Result<Link>().IsValid());

            return _link;
        }
    }
}
