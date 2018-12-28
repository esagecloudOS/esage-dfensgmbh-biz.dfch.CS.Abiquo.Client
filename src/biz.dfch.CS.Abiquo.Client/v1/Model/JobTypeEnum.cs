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
    public enum JobTypeEnum
    {
        // Virtual factory
        CONFIGURE
        ,
        DECONFIGURE
        ,
        RECONFIGURE
        ,
        POWER_ON
        ,
        POWER_OFF
        ,
        PAUSE
        ,
        RESUME
        ,
        RESET
        ,
        INSTANCE
        ,
        REFRESH
        ,
        SHUTDOWN
        ,
        REGISTER_VM
        ,

        // Virtual image template
        DOWNLOAD
        ,
        PROMOTE
        ,
        COPY_DISK
        ,
        EXPORT_TO_PRIVATE
        ,
        EXPORT_TO_PUBLIC
        ,

        // Conversion manager
        DISK_CONVERSION
        ,
        DUMP_DISK_TO_VOLUME
        ,
        DUMP_VOLUME_TO_DISK,

        // Scheduler
        SCHEDULE
        ,
        FREE_RESOURCES
        ,
        UPDATE_RESOURCES
        ,

        // Public cloud region
        NETWORK
        ,
        PRIVATE_NETWORKS
        ,
        EXTERNAL_NETWORKS
        ,
        FLOATING_IPS
        ,
        FIREWALLS
        ,
        LOAD_BALANCERS
        ,
        VIRTUAL_MACHINES
        ,
        CHECK_DEPENDENCIES
        ,

        // Virtual Appliance Spec
        SPEC_CHEF_VALIDATION
        ,
        SPEC_PRIVATE_NETWORKS
        ,
        SPEC_PUBLIC_NETWORKS
        ,
        SPEC_FIREWALLS
        ,
        SPEC_LOAD_BALANCER
        ,
        SPEC_VOLUMES
        ,
        SPEC_VIRTUAL_MACHINES
        ,
        SPEC_EXTERNAL_NETWORKS
        ,
        SPEC_FLOATING_IPS
        ,

        // Virtual machine action plan
        SEND_EMAIL
    }
}
