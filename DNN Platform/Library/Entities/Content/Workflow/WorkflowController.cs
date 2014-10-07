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
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow
{
    // TODO: add interface metadata documentation
    internal class WorkflowController : ServiceLocator<IWorkflowController, WorkflowController>, IWorkflowController
    {
        private readonly IWorkflowStateController _stateController = WorkflowStateController.Instance;

        #region Constructor
        public WorkflowController()
        {
            
        }

        public WorkflowController(IWorkflowStateController stateController)
        {
            _stateController = stateController;
        }
        #endregion

        public IEnumerable<ContentWorkflow> GetSystemWorkflows(int portalId)
        {
            return CBO.FillCollection<ContentWorkflow>(DataProvider.Instance().GetContentWorkflows(portalId)).Where(w => w.IsSystem); 
        }

        public ContentWorkflow GetWorkflowByID(int workflowId)
        {
            var workflow = CBO.FillObject<ContentWorkflow>(DataProvider.Instance().GetContentWorkflow(workflowId));
            if (workflow != null)
            {
                workflow.States = _stateController.GetWorkflowStates(workflowId);
                return workflow;
            }
            return null;
        }

        public ContentWorkflow GetWorkflow(ContentItem item)
        {
            var state = _stateController.GetWorkflowStateByID(item.StateID);
            if (state == null) return null;
            return GetWorkflowByID(state.WorkflowID);
        }

        // TODO: validation
        public void AddWorkflow(ContentWorkflow workflow)
        {
            var id = DataProvider.Instance().AddContentWorkflow(workflow.PortalID, workflow.WorkflowName, workflow.Description, workflow.IsDeleted, workflow.StartAfterCreating, workflow.StartAfterEditing, workflow.DispositionEnabled);
            workflow.WorkflowID = id;
        }

        // TODO: validation
        public void UpdateWorkflow(ContentWorkflow workflow)
        {
            DataProvider.Instance().UpdateContentWorkflow(workflow.WorkflowID, workflow.WorkflowName, workflow.Description, workflow.IsDeleted, workflow.StartAfterCreating, workflow.StartAfterEditing, workflow.DispositionEnabled);
        }

        public void DeleteWorkflow(ContentWorkflow workflow)
        {
            // TODO: Implement it
            // TODO: verify that the workflow is not in use (some content items are associated with the workflow)
        }

        public IEnumerable<ContentWorkflow> GetWorkflows(int portalId)
        {
            return CBO.FillCollection<ContentWorkflow>(DataProvider.Instance().GetContentWorkflows(portalId));
        }

        protected override Func<IWorkflowController> GetFactory()
        {
            return () => new WorkflowController();
        }
    }
}
