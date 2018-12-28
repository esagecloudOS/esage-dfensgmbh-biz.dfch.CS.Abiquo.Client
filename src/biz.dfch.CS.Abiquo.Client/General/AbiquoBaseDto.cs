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
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using biz.dfch.CS.Commons;

namespace biz.dfch.CS.Abiquo.Client.General
{
    public abstract class AbiquoBaseDto : BaseDto
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings;

        static AbiquoBaseDto()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None
                ,
                // As the Abiquo deserializer does not ignore case sensitivity
                // the C# properties, that start with a upper case letter have to be
                // changed to start with a lowercase letter when serialized to JSON
                ContractResolver = new CamelCasePropertyNamesContractResolver()
                ,
                MissingMemberHandling = MissingMemberHandling.Error
                ,
                // Properties, that are not initialized will not be serialized
                NullValueHandling = NullValueHandling.Ignore
                ,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        internal static void SetJsonSerializerMissingMemberHandling(MissingMemberHandling missingMemberHandling)
        {
            _jsonSerializerSettings.MissingMemberHandling = missingMemberHandling;
        }

        public override string SerializeObject()
        {
            return JsonConvert.SerializeObject(this, _jsonSerializerSettings);
        }

        public new static object DeserializeObject(string value, Type type)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));
            Contract.Requires(null != type);

            return JsonConvert.DeserializeObject(value, type, _jsonSerializerSettings);
        }

        public new static T DeserializeObject<T>(string value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));

            return JsonConvert.DeserializeObject<T>(value, _jsonSerializerSettings);
        }
    }
}
