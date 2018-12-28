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

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace biz.dfch.CS.Abiquo.Client.Authentication
{
    public class OAuth2AuthenticationInformation : IAuthenticationInformation
    {
        private readonly string _oAuth2Token;

        public OAuth2AuthenticationInformation(string oAuth2Token)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(oAuth2Token));

            _oAuth2Token = oAuth2Token;
        }

        public IDictionary<string, string> GetAuthorizationHeaders()
        {
            var headerValue = string.Format(Constants.Authentication.BEARER_AUTHORIZATION_HEADER_VALUE_TEMPLATE, _oAuth2Token);

            var headers = new Dictionary<string, string>
            {
                {Constants.Authentication.AUTHORIZATION_HEADER_KEY, headerValue}
            };

            return headers;
        }
    }
}
