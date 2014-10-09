#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    //TODO: add metadata info
    //TODO: add entities and validation
    internal class WorkflowLogRepository : ServiceLocator<IWorkflowLogRepository, WorkflowLogRepository> , IWorkflowLogRepository
    {
        public IEnumerable<ContentWorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {
            return CBO.FillCollection<ContentWorkflowLog>(DataProvider.Instance().GetContentWorkflowLogs(contentItemId, workflowId));
        }

        public void DeleteWorkflowLogs(int contentItemId, int workflowId)
        {
            DataProvider.Instance().DeleteContentWorkflowLogs(contentItemId, workflowId);
        }

        public void AddWorkflowLog(int contentItemId, int workflowId, string action, string comment, int userId)
        {
            DataProvider.Instance().AddContentWorkflowLog(action, comment, userId, workflowId, contentItemId);
        }

        protected override Func<IWorkflowLogRepository> GetFactory()
        {
            return () => new WorkflowLogRepository();
        }
    }
}
