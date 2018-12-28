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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace biz.dfch.CS.Abiquo.Client.v1
{
    public static class AbiquoMediaDataTypes
    {
        private const string APPLICATION_TYPE_JSON = "+json";

        public const string VND_ABIQUO_ACCEPTEDREQUEST = "application/vnd.abiquo.acceptedrequest" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ACCEPTEDREQUEST2 = "application/vnd.abiquo.acceptedrequest" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_APPLICATIONS = "application/vnd.abiquo.applications" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_APPLICATION = "application/vnd.abiquo.application" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_BACKUPS = "application/vnd.abiquo.backups" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_BACKUP = "application/vnd.abiquo.backup" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_CATEGORY = "application/vnd.abiquo.category" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_CATEGORIES = "application/vnd.abiquo.categories" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_CLOUDUSAGE = "application/vnd.abiquo.cloudusage" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_CLOUDUSAGES = "application/vnd.abiquo.cloudusages" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_COSTCODE = "application/vnd.abiquo.costcode" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_COSTCODES = "application/vnd.abiquo.costcodes" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_COSTCODECURRENCY = "application/vnd.abiquo.costcodecurrency" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_COSTCODECURRENCIES = "application/vnd.abiquo.costcodecurrencies" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_CURRENCY = "application/vnd.abiquo.currency" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_CURRENCIES = "application/vnd.abiquo.currencies" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DATACENTER = "application/vnd.abiquo.datacenter" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DATACENTERS = "application/vnd.abiquo.datacenters" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_LIMIT = "application/vnd.abiquo.limit" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_LIMITS = "application/vnd.abiquo.limits" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DATACENTERREPOSITORY = "application/vnd.abiquo.datacenterrepository" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DATACENTERREPOSITORIES = "application/vnd.abiquo.datacenterrepositories" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DATACENTERRESOURCES = "application/vnd.abiquo.datacenterresources" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DATACENTERSRESOURCES = "application/vnd.abiquo.datacentersresources" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DATASTORE = "application/vnd.abiquo.datastore" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DATASTORES = "application/vnd.abiquo.datastores" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DHCPOPTION = "application/vnd.abiquo.dhcpoption" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DHCPOPTIONS = "application/vnd.abiquo.dhcpoptions" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_HARDDISK = "application/vnd.abiquo.harddisk" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_HARDDISKS = "application/vnd.abiquo.harddisks" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DISKFORMATTYPES = "application/vnd.abiquo.diskformattypes" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_DISKFORMATTYPE = "application/vnd.abiquo.diskformattype" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ENTERPRISE = "application/vnd.abiquo.enterprise" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ENTERPRISES = "application/vnd.abiquo.enterprises" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ENTERPRISEEXCLUSIONRULE = "application/vnd.abiquo.enterpriseexclusionrule" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ENTERPRISEEXCLUSIONRULES = "application/vnd.abiquo.enterpriseexclusionrules" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ENTERPRISEPROPERTIES = "application/vnd.abiquo.enterpriseproperties" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ENTERPRISERESOURCES = "application/vnd.abiquo.enterpriseresources" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ENTERPRISESRESOURCES = "application/vnd.abiquo.enterprisesresources" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ERROR = "application/vnd.abiquo.error" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ERRORS = "application/vnd.abiquo.errors" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_EXTERNALIP = "application/vnd.abiquo.externalip" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_EXTERNALIPS = "application/vnd.abiquo.externalips" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_FITPOLICYRULES = "application/vnd.abiquo.fitpolicyrules" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_FITPOLICYRULE = "application/vnd.abiquo.fitpolicyrule" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_FSM = "application/vnd.abiquo.fsm" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_FSMS = "application/vnd.abiquo.fsms" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_HYPERVISORTYPE = "application/vnd.abiquo.hypervisortype" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_HYPERVISORTYPES = "application/vnd.abiquo.hypervisortypes" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_INITIATORMAPPING = "application/vnd.abiquo.initiatormapping" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_INITIATORMAPPINGS = "application/vnd.abiquo.initiatormappings" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_IP = "application/vnd.abiquo.ip" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_IPS = "application/vnd.abiquo.ips" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_JOB = "application/vnd.abiquo.job" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_JOBS = "application/vnd.abiquo.jobs" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_LICENSE = "application/vnd.abiquo.license" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_LICENSES = "application/vnd.abiquo.licenses" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_LINKS = "application/vnd.abiquo.links" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_LOGICSERVER = "application/vnd.abiquo.logicserver" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_LOGICSERVERS = "application/vnd.abiquo.logicservers" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_MACHINE = "application/vnd.abiquo.machine" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_MACHINES = "application/vnd.abiquo.machines" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_MACHINELOADRULE = "application/vnd.abiquo.machineloadrule" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_MACHINELOADRULES = "application/vnd.abiquo.machineloadrules" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_MACHINESTATE = "application/vnd.abiquo.machinestate" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_NIC = "application/vnd.abiquo.nic" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_NICS = "application/vnd.abiquo.nics" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ORGANIZATION = "application/vnd.abiquo.organization" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ORGANIZATIONS = "application/vnd.abiquo.organizations" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRICINGCOSTCODE = "application/vnd.abiquo.pricingcostcode" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRICINGCOSTCODES = "application/vnd.abiquo.pricingcostcodes" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRICINGTEMPLATE = "application/vnd.abiquo.pricingtemplate" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRICINGTEMPLATES = "application/vnd.abiquo.pricingtemplates" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRICINGTIER = "application/vnd.abiquo.pricingtier" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRICINGTIERS = "application/vnd.abiquo.pricingtiers" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRIVATEIP = "application/vnd.abiquo.privateip" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRIVATEIPS = "application/vnd.abiquo.privateips" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRIVILEGE = "application/vnd.abiquo.privilege" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PRIVILEGES = "application/vnd.abiquo.privileges" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PUBLICIP = "application/vnd.abiquo.publicip" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_PUBLICIPS = "application/vnd.abiquo.publicips" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_RACK = "application/vnd.abiquo.rack" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_RACKS = "application/vnd.abiquo.racks" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_REMOTESERVICE = "application/vnd.abiquo.remoteservice" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_REMOTESERVICES = "application/vnd.abiquo.remoteservices" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ROLE = "application/vnd.abiquo.role" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ROLES = "application/vnd.abiquo.roles" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ROLELDAP = "application/vnd.abiquo.roleldap" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ROLESLDAP = "application/vnd.abiquo.rolesldap" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ROLEWITHLDAP = "application/vnd.abiquo.rolewithldap" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_EXTENDED_RUNLIST = "application/vnd.abiquo.extended-runlist" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_EXTENDED_RUNLISTS = "application/vnd.abiquo.extended-runlists" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_SEEOTHER = "application/vnd.abiquo.seeother" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_STORAGEDEVICE = "application/vnd.abiquo.storagedevice" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_STORAGEDEVICES = "application/vnd.abiquo.storagedevices" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_STORAGEPOOL = "application/vnd.abiquo.storagepool" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_STORAGEPOOLS = "application/vnd.abiquo.storagepools" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_STORAGEPOOLWITHTIER = "application/vnd.abiquo.storagepoolwithtier" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_STORAGEPOOLSWITHTIER = "application/vnd.abiquo.storagepoolswithtier" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_STORAGEPOOLWITHDEVICE = "application/vnd.abiquo.storagepoolwithdevice" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_STORAGEPOOLSWITHDEVICE = "application/vnd.abiquo.storagepoolswithdevice" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_SYSTEMPROPERTY = "application/vnd.abiquo.systemproperty" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_SYSTEMPROPERTIES = "application/vnd.abiquo.systemproperties" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_TASK = "application/vnd.abiquo.task" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_TASKS = "application/vnd.abiquo.tasks" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_TEMPLATEDEFINITION = "application/vnd.abiquo.templatedefinition" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_TEMPLATEDEFINITIONS = "application/vnd.abiquo.templatedefinitions" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_TEMPLATEDEFINITIONLIST = "application/vnd.abiquo.templatedefinitionlist" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_TEMPLATEDEFINITIONLISTS = "application/vnd.abiquo.templatedefinitionlists" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_TIER = "application/vnd.abiquo.tier" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_TIERS = "application/vnd.abiquo.tiers" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_UCSRACK = "application/vnd.abiquo.ucsrack" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_UCSRACKS = "application/vnd.abiquo.ucsracks" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_USER = "application/vnd.abiquo.user" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_USERS = "application/vnd.abiquo.users" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_USERWITHROLES = "application/vnd.abiquo.userwithroles" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_USERSWITHROLES = "application/vnd.abiquo.userswithroles" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALAPPLIANCE = "application/vnd.abiquo.virtualappliance" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALAPPLIANCES = "application/vnd.abiquo.virtualappliances" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALAPPLIANCESTATE = "application/vnd.abiquo.virtualappliancestate" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALAPPLIANCEPRICE = "application/vnd.abiquo.virtualapplianceprice" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALAPPRESOURCES = "application/vnd.abiquo.virtualappresources" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALAPPSRESOURCES = "application/vnd.abiquo.virtualappsresources" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALDATACENTER = "application/vnd.abiquo.virtualdatacenter" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALDATACENTERS = "application/vnd.abiquo.virtualdatacenters" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALDATACENTERRESOURCES = "application/vnd.abiquo.virtualdatacenterresources" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALDATACENTERSRESOURCES = "application/vnd.abiquo.virtualdatacentersresources" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINE = "application/vnd.abiquo.virtualmachine" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINES = "application/vnd.abiquo.virtualmachines" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINETASK = "application/vnd.abiquo.virtualmachinetask" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINEWITHNODE = "application/vnd.abiquo.virtualmachinewithnode" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINESWITHNODE = "application/vnd.abiquo.virtualmachineswithnode" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINEWITHNODEEXTENDED = "application/vnd.abiquo.virtualmachinewithnodeextended" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINESWITHNODEEXTENDED = "application/vnd.abiquo.virtualmachineswithnodeextended" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINETEMPLATE = "application/vnd.abiquo.virtualmachinetemplate" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINETEMPLATES = "application/vnd.abiquo.virtualmachinetemplates" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINESTATE = "application/vnd.abiquo.virtualmachinestate" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINEINSTANCE = "application/vnd.abiquo.virtualmachineinstance" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VLAN = "application/vnd.abiquo.vlan" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VLANS = "application/vnd.abiquo.vlans" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VLANTAGAVAILABILITY = "application/vnd.abiquo.vlantagavailability" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINENETWORKCONFIGURATION = "application/vnd.abiquo.virtualmachinenetworkconfiguration" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_VIRTUALMACHINENETWORKCONFIGURATIONS = "application/vnd.abiquo.virtualmachinenetworkconfigurations" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ISCSIVOLUME = "application/vnd.abiquo.iscsivolume" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ISCSIVOLUMES = "application/vnd.abiquo.iscsivolumes" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ISCSIVOLUMEWITHVIRTUALMACHINE = "application/vnd.abiquo.iscsivolumewithvirtualmachine" + APPLICATION_TYPE_JSON;
        public const string VND_ABIQUO_ISCSIVOLUMESWITHVIRTUALMACHINE = "application/vnd.abiquo.iscsivolumeswithvirtualmachine" + APPLICATION_TYPE_JSON;
    }
}
