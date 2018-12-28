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

namespace biz.dfch.CS.Abiquo.Client.v1.Model
{
    public enum VirtualMachineStateEnum
    {
        /// <summary>
        /// The virtual machine only exists in Abiquo and has not yet a physical machine assigned.
        /// </summary>
        NOT_ALLOCATED
        ,
        /// <summary>
        /// The virtual machine does not exists in the hypervisor but has physical machine assigned.
        /// </summary>
        ALLOCATED
        ,
        /// <summary>
        /// The virtual machine exists in the hypervisor.
        /// </summary>
        CONFIGURED
        ,
        /// <summary>
        /// The virtual machine exists in the hypervisor and is ON.
        /// </summary>
        ON
        ,
        /// <summary>
        /// The virtual machine exists in the hypervisor and is SUSPENDED.
        /// </summary>
        PAUSED
        ,
        /// <summary>
        /// The virtual machine exists in the hypervisor and is OFF.
        /// </summary>
        OFF
        ,
        /// <summary>
        /// Some operation is being performed on the virtual machine.
        /// </summary>
        LOCKED
        ,
        /// <summary>
        /// Abiquo does know the actual state of the virtual machine. But it exists in the hypervisor.
        /// </summary>
        UNKNOWN
    }
}
