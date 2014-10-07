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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow
{
    //TODO: add metadata info
    internal class WorkflowStateController : ServiceLocator<IWorkflowStateController, WorkflowStateController>, IWorkflowStateController
    {
        public IEnumerable<ContentWorkflowState> GetWorkflowStates(int workflowID)
        {
            return CBO.FillCollection<ContentWorkflowState>(DataProvider.Instance().GetContentWorkflowStates(workflowID));
        }

        public void AddWorkflowState(ContentWorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNullOrEmpty("state", "StateName", state.StateName);

            var id = DataProvider.Instance().AddContentWorkflowState(state.WorkflowID,
                                                                state.StateName,
                                                                state.Order,
                                                                state.IsActive,
                                                                state.SendEmail,
                                                                state.SendMessage,
                                                                state.IsDisposalState,
                                                                state.OnCompleteMessageSubject,
                                                                state.OnCompleteMessageBody,
                                                                state.OnDiscardMessageSubject,
                                                                state.OnDiscardMessageBody);
            state.StateID = id;
        }

        public void UpdateWorkflowState(ContentWorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNegative("state", "StateID", state.StateID);
            Requires.PropertyNotNullOrEmpty("state", "StateName", state.StateName);

            DataProvider.Instance().UpdateContentWorkflowState(state.StateID,
                                                                state.StateName,
                                                                state.Order,
                                                                state.IsActive,
                                                                state.SendEmail,
                                                                state.SendMessage,
                                                                state.IsDisposalState,
                                                                state.OnCompleteMessageSubject,
                                                                state.OnCompleteMessageBody,
                                                                state.OnDiscardMessageSubject,
                                                                state.OnDiscardMessageBody);
        }

        public void DeleteWorkflowState(ContentWorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNegative("state", "StateID", state.StateID);

            DataProvider.Instance().DeleteContentWorkflowState(state.StateID);
        }

        protected override Func<IWorkflowStateController> GetFactory()
        {
            return () => new WorkflowStateController();
        }
    }
}
