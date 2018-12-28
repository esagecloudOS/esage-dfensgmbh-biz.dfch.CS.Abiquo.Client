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
using System.Diagnostics.Contracts;
using System.Text;

namespace biz.dfch.CS.Abiquo.Client.Authentication
{
    public class BasicAuthenticationInformation : IAuthenticationInformation
    {
        private readonly string _username;
        private readonly string _password;

        public BasicAuthenticationInformation(string username, string password)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(username));
            Contract.Requires(!string.IsNullOrWhiteSpace(password));

            _username = username;
            _password = password;
        }

        public IDictionary<string, string> GetAuthorizationHeaders()
        {
            var headers = new Dictionary<string, string>
            {
                {Constants.Authentication.AUTHORIZATION_HEADER_KEY, CreateBasicAuthorizationHeaderValue()}
            };

            return headers;
        }

        private string CreateBasicAuthorizationHeaderValue()
        {
            var plainText = string.Format("{0}:{1}", _username, _password);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var base64EncodedAuthorizationHeaderValue = Convert.ToBase64String(plainTextBytes);

            return string.Format(Constants.Authentication.BASIC_AUTHORIZATION_HEADER_VALUE_TEMPLATE, base64EncodedAuthorizationHeaderValue);
        }
    }
}
