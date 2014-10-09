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
using DotNetNuke.Entities.Content.Workflow.Exceptions;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowManager : ServiceLocator<IWorkflowManager, WorkflowManager>, IWorkflowManager
    {
        private readonly DataProvider _dataProvider;
        private readonly IWorkflowRepository _workflowRepository = WorkflowRepository.Instance;
        private readonly IWorkflowStateRepository _workflowStateRepository = WorkflowStateRepository.Instance;
        private readonly ISystemWorkflowManager _systemWorkflowController = SystemWorkflowManager.Instance;

        public WorkflowManager()
        {
            _dataProvider = DataProvider.Instance();
        }

        public void AddWorkflow(ContentWorkflow workflow)
        {
            _workflowRepository.AddWorkflow(workflow);

            var firstDefaultState = _systemWorkflowController.GetDraftStateDefinition(1);
            var lastDefaultState = _systemWorkflowController.GetPublishedStateDefinition(2);

            firstDefaultState.WorkflowID = workflow.WorkflowID;
            lastDefaultState.WorkflowID = workflow.WorkflowID;

            _workflowStateRepository.AddWorkflowState(firstDefaultState);
            _workflowStateRepository.AddWorkflowState(lastDefaultState);

            workflow.States = new List<ContentWorkflowState>
                              {
                                  firstDefaultState,
                                  lastDefaultState
                              };
        }

        public void AddWorkflowState(ContentWorkflowState state)
        {
            var workflow = _workflowRepository.GetWorkflowByID(state.WorkflowID);
            if (workflow == null)
            {
                throw new WorkflowException("Workflow is not found");
            }
            if (workflow.IsSystem)
            {
                throw new WorkflowException("New states cannot be added to system workflows");
            }

            var lastState = workflow.LastState;
            
            // New States always goes before the last state
            state.Order = lastState.Order;

            lastState.Order++;
            _workflowStateRepository.UpdateWorkflowState(lastState); // Update last state order
            _workflowStateRepository.AddWorkflowState(state);
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
