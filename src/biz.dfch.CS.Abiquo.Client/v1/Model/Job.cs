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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace biz.dfch.CS.Abiquo.Client.v1.Model
{
    public class Job : AbiquoLinkBaseDto
    {
        public string Description { get; set; }

        [Required]
        public string Id { get; set; }
        
        public string ParentTaskId { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public JobStateEnum RollbackState { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public JobStateEnum State { get; set; }

        [Required]
        [Range(1, Int64.MaxValue)]
        public long Timestamp { get; set; }

        [Required]
        [Range(1, Int64.MaxValue)]
        public long CreationTimestamp { get; set; }

        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public JobTypeEnum Type { get; set; }
    }
}
