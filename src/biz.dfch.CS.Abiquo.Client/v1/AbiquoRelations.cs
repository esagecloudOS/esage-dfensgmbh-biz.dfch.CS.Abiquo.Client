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

namespace biz.dfch.CS.Abiquo.Client.v1
{
    public static class AbiquoRelations
    {
        /// <summary>
        /// Entity specific relations
        /// </summary>
        public const string ENTERPRISE = "enterprise";
        public const string IPS = "ips";
        public const string ROLE = "role";
        public const string USERS = "users";
        public const string VIRTUALAPPLIANCE = "virtualappliance";
        public const string VIRTUALDATACENTER = "virtualdatacenter";
        public const string VIRTUALMACHINES = "virtualmachines";
        public const string VIRTUALMACHINETEMPLATE = "virtualmachinetemplate";

        /// <summary>
        /// General relations
        /// </summary>
        public const string EDIT = "edit";
        public const string FIRST = "first";
        public const string LAST = "last";
        public const string PROPERTIES = "properties";
        public const string SELF = "self";
        public const string STATUS = "status";
    }
}
