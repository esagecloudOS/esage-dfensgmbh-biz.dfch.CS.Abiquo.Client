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
using System.ComponentModel;
using biz.dfch.CS.Abiquo.Client.Authentication;
using biz.dfch.CS.Commons.Converters;

namespace biz.dfch.CS.Abiquo.Client.Tests
{
    public class AbiquoSettings : EnvironmentVariableBaseDto
    {
        public const string ABIQUO_API_BASE_URI = @"https://abiquo.example.com/api/";
        public const string ABIQUO_USERNAME = "admin";
        public const string ABIQUO_PASSWORD = "xabiquo";
        public const int  ABIQUO_TENANT_ID = 1;
        
        [EnvironmentVariable("ABIQUO_API_BASE_URI")]
        [DefaultValue(ABIQUO_API_BASE_URI)]
        public string AbiquoApiBaseUri { get; set; }
        
        [EnvironmentVariable("ABIQUO_USERNAME")]
        [DefaultValue(ABIQUO_USERNAME)]
        public string Username { get; set; }
        
        [EnvironmentVariable("ABIQUO_PASSWORD")]
        [DefaultValue(ABIQUO_PASSWORD)]
        public string Password { get; set; }
        
        [EnvironmentVariable("ABIQUO_TENANT_ID")]
        [DefaultValue(ABIQUO_TENANT_ID)]
        public int TenantId { get; set; }
    }
        
    internal class IntegrationTestEnvironment
    {
        static IntegrationTestEnvironment()
        {
            var settings = new AbiquoSettings();
            settings.Import();

            AbiquoApiBaseUri = settings.AbiquoApiBaseUri;
            Username = settings.Username;
            Password = settings.Password;
            TenantId = settings.TenantId;

            AuthenticationInformation = new BasicAuthenticationInformation(Username, Password);
        }

        public static string AbiquoApiBaseUri { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static int TenantId { get; set; }

        public static IAuthenticationInformation AuthenticationInformation { get; set; }
    }
}
