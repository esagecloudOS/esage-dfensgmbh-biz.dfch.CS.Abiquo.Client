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

using System.ComponentModel.DataAnnotations;
using biz.dfch.CS.Abiquo.Client.General;

namespace biz.dfch.CS.Abiquo.Client.v1.Model
{
    public class User : AbiquoLinkBaseDto
    {
        [Required]
        public bool Active { get; set; }

        [Required]
        public string AuthType { get; set; }
        
        public string AvailableVirtualDatacenters { get; set; }
        
        public string Description { get; set; }
        
        public string Email { get; set; }

        public int Id { get; set; }
        
        public string Locale { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Nick { get; set; }
        
        public string Password { get; set; }
        
        public string Surname { get; set; }

        public string OldPassword { get; set; }

        public bool Locked { get; set; }

        public bool FirstLogin { get; set; }

        public Privileges Privileges { get; set; }

        public string PublicSshKey { get; set; }
    }
}
