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
 
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
﻿using System.ComponentModel.DataAnnotations;

namespace biz.dfch.CS.Abiquo.Client.v1.Model
{
    public class VirtualMachineTemplate : AbiquoLinkBaseDto
    {
        public bool ChefEnabled { get; set; }
        
        public int CostCode { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CpuRequired { get; set; }
        
        public int CoresPerSocket { get; set; }
        
        public string CreationDate { get; set; }
        
        public string CreationUser { get; set; }
        
        public string Description { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public EthernetDriverTypeEnum EthernetDriverType { get; set; }
        
        public string IconUrl { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
        
        public string LoginPassword { get; set; }
        
        public string LoginUser { get; set; }

        [Required]
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OsTypeEnum OsType { get; set; }
        
        public string OsVersion { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int RamRequired { get; set; }

        [Required]
        public bool Shared { get; set; }
        
        public string State { get; set; }
        
        public Dictionary<string, string> Variables { get; set; }
        
        public bool EnableCpuHotAdd { get; set; }
        
        public bool EnableRamHotAdd { get; set; }
        
        public bool EnableDisksHotReconfigure { get; set; }
        
        public bool EnableNicsHotReconfigure { get; set; }
        
        public bool EnableRemoteAccessHotReconfigure { get; set; }

        public int CpuMin { get; set; }
        
        public int CpuMax { get; set; }
        
        public int RamMin { get; set; }
        
        public int RamMax { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DiskControllerTypeEnum DiskControllerType { get; set; }

        public string DiskController { get; set; }

        public long DiskFileSize{ get; set; }

        public string DiskFormatType { get; set; }

        public long HdRequired { get; set; }

        public string Path { get; set; }
    }
}
