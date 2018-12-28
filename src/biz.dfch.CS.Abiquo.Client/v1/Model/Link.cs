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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using biz.dfch.CS.Abiquo.Client.General;

namespace biz.dfch.CS.Abiquo.Client.v1.Model
{
    public class Link : AbiquoV1BaseDto
    {
        [Required]
        public string Rel { get; set; }

        [Required]
        public string Href { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Hreflang { get; set; }

        public string DiskController { get; set; }

        public string DiskControllerType { get; set; }

        public string DiskLabel { get; set; }

        public string Length { get; set; }

        public string GetUriSuffix()
        {
            Contract.Requires(Uri.IsWellFormedUriString(Href, UriKind.Absolute));

            var uriSuffix = Href.Substring(Href.IndexOf("/api", StringComparison.InvariantCultureIgnoreCase) + 4);

            return uriSuffix;
        }
    }
}
