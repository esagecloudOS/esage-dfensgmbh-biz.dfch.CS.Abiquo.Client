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

namespace biz.dfch.PS.Abiquo.Client
{
    /// <summary>
    /// Public constants used by this module
    /// </summary>
    public static class Constants
    {
        private const int EVENT_ID_OFFSET = 64;

        /// <summary>
        /// Index for all cmdlets in this module
        /// </summary>
        public enum EventId
        {
            /// <summary>
            /// Enter-Server
            /// </summary>
            EnterServer = 16384,
            /// <summary>
            /// Login Failed AggregateException
            /// </summary>
            EnterServerFailed,
            /// <summary>
            /// Import-Configuration
            /// </summary>
            ImportConfiguration = EnterServer + EVENT_ID_OFFSET,
            /// <summary>
            /// Get-Machine
            /// </summary>
            GetMachine = ImportConfiguration + EVENT_ID_OFFSET,
            /// <summary>
            /// GetMachineIdNotFound
            /// </summary>
            GetMachineIdNotFound,
            /// <summary>
            /// GetMachineNameNotFound
            /// </summary>
            GetMachineNameNotFound,
            /// <summary>
            /// GetMachineVirtualDataCenterOrVirtualApplianceNotFound
            /// </summary>
            GetMachineVirtualDataCenterOrVirtualApplianceNotFound,

            /// <summary>
            /// GetEnterprise
            /// </summary>
            GetEnterprise = GetMachine + EVENT_ID_OFFSET,
            /// <summary>
            /// GetEnterpriseIdNotFound
            /// </summary>
            GetEnterpriseIdNotFound,
            /// <summary>
            /// GetEnterpriseNameNotFound
            /// </summary>
            GetEnterpriseNameNotFound,

            /// <summary>
            /// GetVirtualDataCenter
            /// </summary>
            GetVirtualDataCenter = GetEnterprise + EVENT_ID_OFFSET,
            /// <summary>
            /// GetVirtualDataCenterIdNotFound
            /// </summary>
            GetVirtualDataCenterIdNotFound,
            /// <summary>
            /// GetVirtualDataCenterNameNotFound
            /// </summary>
            GetVirtualDataCenterNameNotFound,

            /// <summary>
            /// GetVirtualAppliance
            /// </summary>
            GetVirtualAppliance = GetVirtualDataCenter + EVENT_ID_OFFSET,
            /// <summary>
            /// GetVirtualApplianceIdNotFound
            /// </summary>
            GetVirtualApplianceIdNotFound,
            /// <summary>
            /// GetVirtualApplianceNameNotFound
            /// </summary>
            GetVirtualApplianceNameNotFound,
            /// <summary>
            /// GetVirtualApplianceVirtualDataCenterNotFound
            /// </summary>
            GetVirtualApplianceVirtualDataCenterNotFound,

        }
    }
}
