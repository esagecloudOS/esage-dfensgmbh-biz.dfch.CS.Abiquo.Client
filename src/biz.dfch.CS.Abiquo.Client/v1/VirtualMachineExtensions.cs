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
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using biz.dfch.CS.Abiquo.Client.Communication;
using biz.dfch.CS.Abiquo.Client.v1.Model;

namespace biz.dfch.CS.Abiquo.Client.v1
{
    public static class VirtualMachineExtensions
    {
        private const int MATCH_COUNT = 4;
        private static readonly string _pattern = string.Format(AbiquoUriSuffixes.VIRTUALMACHINE_BY_VIRTUALDATACENTER_ID_AND_VIRTUALAPLLIANCE_ID_AND_VIRTUALMACHINE_ID, @"(\d+)", @"(\d+)", @"(\d+)");

        public static Tuple<int, int, int> ExtractIds(this VirtualMachine virtualMachine)
        {
            Contract.Requires(null != virtualMachine);
            Contract.Requires(virtualMachine.Id.HasValue);
            Contract.Ensures(null != Contract.Result<Tuple<int, int, int>>());
            Contract.Ensures(virtualMachine.Id.Value == Contract.Result<Tuple<int, int, int>>().Item3);

            var link = virtualMachine.GetLinkByRel(AbiquoRelations.EDIT);

            return ExtractIds(link);
        }

        public static Tuple<int, int, int> ExtractIds(Link link)
        {
            Contract.Requires(null != link);
            Contract.Requires(AbiquoRelations.EDIT == link.Rel);
            Contract.Ensures(null != Contract.Result<Tuple<int, int, int>>());

            var match = Regex.Match(link.Href, _pattern);
            Contract.Assert(match.Success);
            Contract.Assert(MATCH_COUNT == match.Groups.Count);

            var virtualDataCenterId = int.Parse(match.Groups[1].Value);
            var virtualApplianceId = int.Parse(match.Groups[2].Value);
            var virtualMachineId = int.Parse(match.Groups[3].Value);

            var result = new Tuple<int, int, int>(virtualDataCenterId, virtualApplianceId, virtualMachineId);
            return result;
        }
    }
}
