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

namespace biz.dfch.CS.Abiquo.Client
{
    public static class Constants
    {
        public enum EventId : int
        {
            Login = 4096,
            LoginSucceeded,
            LoginFailed,
            Logout,
            LogoutSucceeded,
            ExecuteRequest,
            GetByVersion,
            WaitForTaskCompletion,
            Invoke,
            InvokeCompleted,
        }

        public static class Authentication
        {
            public const string AUTHORIZATION_HEADER_KEY = "Authorization";
            public const string COOKIE_HEADER_KEY = "Cookie";

            public const string BASIC_AUTHORIZATION_HEADER_VALUE_TEMPLATE = "Basic {0}";
            public const string BEARER_AUTHORIZATION_HEADER_VALUE_TEMPLATE = "Bearer {0}";
        }
    }
}
