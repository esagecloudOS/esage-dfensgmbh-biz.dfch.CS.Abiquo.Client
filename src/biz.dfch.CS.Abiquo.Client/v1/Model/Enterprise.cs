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

namespace biz.dfch.CS.Abiquo.Client.v1.Model
{
    public class Enterprise : LimitsBaseDto
    {
        public string ChefClient { get; set; }

        public string ChefClientCertificate { get; set; }

        public string ChefUrl { get; set; }

        public string ChefValidator { get; set; }

        public string ChefValidatorCertificate { get; set; }

        public int Id { get; set; }

        public int IdPricingTemplate { get; set; }
        
        public bool IsReservationRestricted { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0, Int64.MaxValue)]
        public long RepositoryHardInMb { get; set; }

        [Required]
        [Range(0, Int64.MaxValue)]
        public long RepositorySoftInMb { get; set; }

        public bool Workflow { get; set; }

        public bool TwoFactorAuthenticationMandatory { get; set; }

        public string Theme { get; set; }
    }
}
