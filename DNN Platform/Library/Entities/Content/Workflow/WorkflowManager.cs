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
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Exceptions;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowManager : ServiceLocator<IWorkflowManager, WorkflowManager>, IWorkflowManager
    {
        private readonly DataProvider _dataProvider;
        private readonly IWorkflowRepository _workflowRepository = WorkflowRepository.Instance;
        private readonly IWorkflowStateRepository _workflowStateRepository = WorkflowStateRepository.Instance;
        private readonly ISystemWorkflowManager _systemWorkflowManager = SystemWorkflowManager.Instance;

        public WorkflowManager()
        {
            _dataProvider = DataProvider.Instance();
        }

        public void AddWorkflow(ContentWorkflow workflow)
        {
            _workflowRepository.AddWorkflow(workflow);

            var firstDefaultState = _systemWorkflowManager.GetDraftStateDefinition(1);
            var lastDefaultState = _systemWorkflowManager.GetPublishedStateDefinition(2);

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
                throw new WorkflowDoesNotExistException();
            }
            if (workflow.IsSystem)
            {
                throw new WorkflowException("New states cannot be added to system workflows"); //TODO: localize error message
            }

            var lastState = workflow.LastState;
            
            // New States always goes before the last state
            state.Order = lastState.Order;

            lastState.Order++;
            _workflowStateRepository.UpdateWorkflowState(lastState); // Update last state order
            _workflowStateRepository.AddWorkflowState(state);
        }

        public void DeleteWorkflowState(ContentWorkflowState state)
        {
            var workflow = _workflowRepository.GetWorkflowByID(state.WorkflowID);
            if (workflow == null)
            {
                throw new WorkflowDoesNotExistException();
            }

            var stateToDelete = _workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (stateToDelete.IsSystem)
            {
                throw new WorkflowException("System workflow state cannot be deleted"); // TODO: Localize error message
            }

            if (GetWorkflowUsageCount(workflow.WorkflowID) > 0)
            {
                throw new WorkflowException(Localization.GetString("WorkflowInUsageException", Localization.ExceptionsResourceFile));   
            }
            
            _workflowStateRepository.DeleteWorkflowState(stateToDelete);
            
            // Reorder states order
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ContentWorkflowState>();
                rep.Update("SET [Order] = [Order] - 1 WHERE WorkflowId = @0 AND [Order] > @1", stateToDelete.WorkflowID, stateToDelete.Order);
            }
        }

        public void UpdateWorkflowState(ContentWorkflowState state)
        {
            var workflowState = _workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (workflowState == null)
            {
                throw new WorkflowDoesNotExistException();
            }
            // TODO: check if remove this code. We can make Order as internal
            // We do not allow change Order property
            state.Order = workflowState.Order;

            _workflowStateRepository.UpdateWorkflowState(state);
        }

        public void MoveWorkflowStateDown(int stateId)
        {
            var workflow = _workflowStateRepository.GetWorkflowStateByID(stateId);
            
            if (GetWorkflowUsageCount(workflow.WorkflowID) > 0)
            {
                throw new WorkflowException(Localization.GetString("WorkflowInUsageException", Localization.ExceptionsResourceFile));
            }

            var states = _workflowStateRepository.GetWorkflowStates(workflow.WorkflowID).ToArray();

            if (states.Length == 3)
            {
                throw new WorkflowException("Workflow state cannot be moved"); // TODO: localize
            }
            
            ContentWorkflowState stateToMoveUp = null;
            ContentWorkflowState stateToMoveDown = null;

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].StateID != stateId) continue;

                // First and Second workflow state cannot be moved down
                if (i <= 1)
                {
                    throw new WorkflowException("Workflow state cannot be moved"); // TODO: localize
                }

                stateToMoveUp = states[i - 1];
                stateToMoveDown = states[i];
                break;
            }

            if (stateToMoveUp == null || stateToMoveDown == null)
            {
                throw new WorkflowException("Workflow state cannot be moved"); // TODO: localize
            }

            var orderTmp = stateToMoveDown.Order;
            stateToMoveDown.Order = stateToMoveUp.Order;
            stateToMoveUp.Order = orderTmp;

            _workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            _workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
        }

        public void MoveWorkflowStateUp(int stateId)
        {
            var workflow = _workflowStateRepository.GetWorkflowStateByID(stateId);
            
            if (GetWorkflowUsageCount(workflow.WorkflowID) > 0)
            {
                throw new WorkflowException(Localization.GetString("WorkflowInUsageException", Localization.ExceptionsResourceFile));
            }

            var states = _workflowStateRepository.GetWorkflowStates(workflow.WorkflowID).ToArray();
            
            if (states.Length == 3)
            {
                throw new WorkflowException("Workflow state cannot be moved"); // TODO: localize
            }

            ContentWorkflowState stateToMoveUp = null;
            ContentWorkflowState stateToMoveDown = null;

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].StateID != stateId) continue;

                // Last and Next to Last workflow state cannot be moved up
                if (i >= states.Length - 2)
                {
                    throw new WorkflowException("Workflow state cannot be moved"); // TODO: localize
                }

                stateToMoveUp = states[i];
                stateToMoveDown = states[i + 1];
                break;
            }

            if (stateToMoveUp == null || stateToMoveDown == null)
            {
                throw new WorkflowException("Workflow state cannot be moved"); // TODO: localize
            }

            var orderTmp = stateToMoveDown.Order;
            stateToMoveDown.Order = stateToMoveUp.Order;
            stateToMoveUp.Order = orderTmp;

            _workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            _workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
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
