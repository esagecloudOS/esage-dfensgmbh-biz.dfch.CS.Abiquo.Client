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
using System;
using System.Collections.Generic;
﻿using System.ComponentModel.DataAnnotations;

namespace biz.dfch.CS.Abiquo.Client.v1.Model
{
    public class VirtualMachine : VirtualMachineBase
    {
        [Required]
        [Range(1, Int32.MaxValue)]
        public int Cpu { get; set; }
        
        public int? CoresPerSocket { get; set; }
        
        public string Description { get; set; }

        [Required]
        [Range(0, Int32.MaxValue)]
        public int HighDisponibility { get; set; }

        public int? Id { get; set; }

        public int? IdState { get; set; }

        public int? IdType { get; set; }
        
        public string Keymap { get; set; }

        [Required]
        public string Name { get; set; }
        
        [StringLength(8)]
        public string Password { get; set; }

        [Required]
        [Range(1, Int32.MaxValue)]
        public int Ram { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualMachineStateEnum State { get; set; }
        
        public string Uuid { get; set; }
        
        public string VdrpIP { get; set; }

        public int? VdrpPort { get; set; }

        public bool VdrpEnabled { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualMachineTypeEnum Type { get; set; }
        
        public string Label { get; set; }
        
        public bool Monitored { get; set; }
        
        public string MonitoringLevel { get; set; }

        public Dictionary<string, string> Variables { get; set; }
        
        public bool Protected { get; set; }

        public Dictionary<string, object> Metadata { get; set; }

        public string ProtectedCause { get; set; }

        public RunlistElements RunlistElements { get; set; }
    }
}
