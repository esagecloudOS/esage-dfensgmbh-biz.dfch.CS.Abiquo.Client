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
    public enum TaskStateEnum
    {
        /// <summary>
        /// All jobs of the task have been finished successfully
        /// </summary>
        FINISHED_SUCCESSFULLY
        ,
        /// <summary>
        /// A job of the task has failed and the task has been finished unsuccessfully
        /// </summary>
        FINISHED_UNSUCCESSFULLY
        ,
        /// <summary>
        /// The task is yet to be queued
        /// </summary>
        QUEUEING
        ,
        /// <summary>
        /// The task remains enqueued
        /// </summary>
        PENDING
        ,
        /// <summary>
        /// The task is being processed
        /// </summary>
        STARTED
        ,
        /// <summary>
        /// The task execution has been aborted
        /// </summary>
        ABORTED
        ,
        /// <summary>
        /// The task was cancelled. Difference with {@link #ABORTED} is that {@link #CANCELLED} is reserved for user action.
        /// </summary>
        CANCELLED
        ,
        /// <summary>
        /// The task finished {@link #FINISHED_UNSUCCESSFULLY} and the error was acknowledged.
        /// </summary>
        ACK_ERROR
    }
}
