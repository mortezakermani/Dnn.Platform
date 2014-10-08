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

using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowManager : ServiceLocator<IWorkflowManager, WorkflowManager>, IWorkflowManager
    {
        private readonly DataProvider _dataProvider;

        public WorkflowManager()
        {
            _dataProvider = DataProvider.Instance();
        }

        public IEnumerable<ContentItem> GetWorkflowUsage(int workflowId, int pageIndex, int pageSize)
        {
            return CBO.FillCollection<ContentItem>(_dataProvider.GetContentWorkflowUsage(workflowId, pageIndex, pageSize));
        }

        public int GetWorkflowUsageCount(int workflowId)
        {
            return _dataProvider.GetContentWorkflowUsageCount(workflowId);
        }

        protected override System.Func<IWorkflowManager> GetFactory()
        {
            return () => new WorkflowManager();
        }
    }
}
