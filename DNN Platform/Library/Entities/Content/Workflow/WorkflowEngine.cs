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
using System.Globalization;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow.Exceptions;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow
{
    // TODO: add metadata doc
    public class WorkflowEngine : ServiceLocator<IWorkflowEngine, WorkflowEngine>, IWorkflowEngine
    {
        #region Members
        private readonly IContentController _contentController;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowStateRepository _workflowStateRepository;
        private readonly IWorkflowLogRepository _workflowLogRepository;
        private readonly IUserController _userController;
        private readonly IWorkflowSecurity _workflowSecurity;
        #endregion

        public WorkflowEngine()
        {
            _contentController = new ContentController();
            _workflowRepository = WorkflowRepository.Instance;
            _workflowStateRepository = WorkflowStateRepository.Instance;
            _workflowLogRepository = WorkflowLogRepository.Instance;
            _workflowSecurity = WorkflowSecurity.Instance;
            _userController = UserController.Instance;
        }

        #region Private Methods

        private void UpdateContentItemWorkflowState(int stateId, ContentItem item)
        {
            item.StateID = stateId;
            _contentController.UpdateContentItem(item);
        }
        
        #region Log Workflow utilities
        private void AddWorkflowCommentLog(ContentItem contentItem, int userId, string userComment)
        {
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }
            AddWorkflowLog(contentItem, ContentWorkflowLogType.CommentProvided, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, ContentWorkflowLogType logType, int userId, string userComment = null)
        {
            var workflow = GetWorkflow(contentItem);
            var logTypeText = GetWorkflowActionComment(logType);
            var state = workflow.States.FirstOrDefault(s => s.StateID == contentItem.StateID);
            var actionText = GetWorkflowActionText(logType);

            var logComment = ReplaceNotificationTokens(logTypeText, workflow, contentItem, state, userId, userComment);

            _workflowLogRepository.AddWorkflowLog(contentItem.ContentItemId, workflow.WorkflowID, actionText, logComment, userId);
        }

        private static string GetWorkflowActionText(ContentWorkflowLogType logType)
        {
            var logName = Enum.GetName(typeof(ContentWorkflowLogType), logType);
            return Localization.GetString(logName + ".Action");
        }

        private static string GetWorkflowActionComment(ContentWorkflowLogType logType)
        {
            var logName = Enum.GetName(typeof(ContentWorkflowLogType), logType);
            return Localization.GetString(logName + ".Comment");
        }

        private string ReplaceNotificationTokens(string text, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, int userId, string comment = "")
        {
            var user = _userController.GetUserById(workflow.PortalID, userId);
            var datetime = DateTime.UtcNow;
            var result = text.Replace("[USER]", user != null ? user.DisplayName : "");
            result = result.Replace("[DATE]", datetime.ToString("F", CultureInfo.CurrentCulture));
            result = result.Replace("[STATE]", state != null ? state.StateName : "");
            result = result.Replace("[WORKFLOW]", workflow.WorkflowName);
            result = result.Replace("[CONTENT]", item != null ? item.ContentTitle : "");
            result = result.Replace("[COMMENT]", !String.IsNullOrEmpty(comment) ? comment : "");
            return result;
        }

        private ContentWorkflowState GetNextWorkflowState(ContentWorkflow workflow, int stateId)
        {
            ContentWorkflowState nextState = null;
            var states = workflow.States.OrderBy(s => s.Order);
            int index;

            // locate the current state
            for (index = 0; index < states.Count(); index++)
            {
                if (states.ElementAt(index).StateID == stateId)
                {
                    break;
                }
            }

            index = index + 1;
            if (index < states.Count())
            {
                nextState = states.ElementAt(index);
            }
            return nextState ?? workflow.FirstState;
        }

        private ContentWorkflowState GetPreviousWorkflowState(ContentWorkflow workflow, int stateId)
        {
            ContentWorkflowState previousState = null;
            var states = workflow.States.OrderBy(s => s.Order);
            int index;

            // locate the current state
            for (index = 0; index < states.Count(); index++)
            {
                if (states.ElementAt(index).StateID == stateId)
                {
                    previousState = states.ElementAt(index - 1);
                    break;
                }
            }

            return previousState ?? workflow.FirstState;
        }
        #endregion

        #endregion

        #region Public Methods
        public ContentWorkflow GetWorkflow(ContentItem contentItem)
        {
            var state = _workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            return state == null ? null : _workflowRepository.GetWorkflowByID(state.WorkflowID);
        }

        public void StartWorkflow(int workflowId, int contentItemId, int userId)
        {
            var contentItem = _contentController.GetContentItem(contentItemId);
            var workflow = GetWorkflow(contentItem);

            //If already exists a started workflow
            if (workflow != null && !IsWorkflowComplete(contentItem))
            {
                //TODO; Study if is need to throw an exception
                return;
            }

            if (workflow == null || workflow.WorkflowID != workflowId)
            {
                workflow = _workflowRepository.GetWorkflowByID(workflowId);
            }

            UpdateContentItemWorkflowState(workflow.FirstState.StateID, contentItem);

            //Delete previous logs
            _workflowLogRepository.DeleteWorkflowLogs(workflowId, contentItemId);

            AddWorkflowLog(contentItem, ContentWorkflowLogType.WorkflowStarted, userId);
            AddWorkflowLog(contentItem, ContentWorkflowLogType.StateInitiated, userId);
        }

        public void CompleteState(int contentItemId, string subject, string body, string comment, int portalId, int userId, string source, params string[] parameters)
        {
            var item = _contentController.GetContentItem(contentItemId);
            var workflow = GetWorkflow(item);
            if (workflow == null || IsWorkflowComplete(item))
            {
                return;
            }

            if (!_workflowSecurity.HasStateReviewerPermission(workflow.PortalID, userId, item.StateID))
            {
                throw new WorkflowSecurityException();
            }

            var currentState = _workflowStateRepository.GetWorkflowStateByID(item.StateID);
            
            AddWorkflowCommentLog(item, userId, comment);
                
            AddWorkflowLog(item, 
                currentState.StateID == workflow.FirstState.StateID 
                    ? ContentWorkflowLogType.DraftCompleted 
                    : ContentWorkflowLogType.StateCompleted, userId);

            var nextState = GetNextWorkflowState(workflow, item.StateID);
            UpdateContentItemWorkflowState(nextState.StateID, item);
                
            AddWorkflowLog(item,
                nextState.StateID == workflow.LastState.StateID
                    ? ContentWorkflowLogType.WorkflowApproved
                    : ContentWorkflowLogType.StateInitiated, userId);

            // TODO: manage complete workflow from here

            // TODO: Review notifications
            // SendNotification(new PortalSettings(portalId), workflow, item, currentState, subject, body, comment, nextState.StateID, userId, source, parameters);
        }

        public void DiscardState(int contentItemId, string subject, string body, string comment, int portalId, int userId)
        {
            var item = _contentController.GetContentItem(contentItemId);
            var workflow = GetWorkflow(item);
            if (workflow == null)
            {
                return;
            }

            if (!_workflowSecurity.HasStateReviewerPermission(portalId, userId, item.StateID))
            {
                throw new WorkflowSecurityException();
            }

            var currentState = _workflowStateRepository.GetWorkflowStateByID(item.StateID);

            if ((workflow.FirstState.StateID != currentState.StateID) && (workflow.LastState.StateID != currentState.StateID))
            {
                var previousState = GetPreviousWorkflowState(workflow, item.StateID);
                UpdateContentItemWorkflowState(previousState.StateID, item);

                // Log
                AddWorkflowCommentLog(item, userId, comment);
                AddWorkflowLog(item, ContentWorkflowLogType.StateDiscarded, userId);
                AddWorkflowLog(item, ContentWorkflowLogType.StateInitiated, userId);

                // SendNotification(new PortalSettings(portalId), workflow, item, currentState, subject, body, comment, previousStateID, userId, null, null);

                // TODO: manage discard workflow from here
            }
        }

        public bool IsWorkflowComplete(int contentItemId)
        {
            var item = _contentController.GetContentItem(contentItemId);
            return IsWorkflowComplete(item);
        }

        public bool IsWorkflowComplete(ContentItem contentItem)
        {
            var workflow = GetWorkflow(contentItem);
            if (workflow == null) return true; // If item has not workflow, then it is considered as completed

            return contentItem.StateID == Null.NullInteger || workflow.LastState.StateID == contentItem.StateID;
        }

        public bool IsWorkflowOnDraft(int contentItemId)
        {
            var contentItem = _contentController.GetContentItem(contentItemId); //Ensure DB values
            return IsWorkflowOnDraft(contentItem);
        }

        public bool IsWorkflowOnDraft(ContentItem contentItem)
        {
            var workflow = GetWorkflow(contentItem);
            if (workflow == null) return false; // If item has not workflow, then it is not on Draft
            return contentItem.StateID == workflow.FirstState.StateID;
        }

        public void DiscardWorkflow(int contentItemId, string comment, int userId)
        {
            var item = _contentController.GetContentItem(contentItemId);
            var workflow = GetWorkflow(item);
            UpdateContentItemWorkflowState(workflow.LastState.StateID, item);

            // Logs
            AddWorkflowCommentLog(item, userId, comment);
            AddWorkflowLog(item, ContentWorkflowLogType.WorkflowDiscarded, userId);
        }

        public void CompleteWorkflow(int contentItemId, string comment, int userId)
        {
            var item = _contentController.GetContentItem(contentItemId);
            var workflow = GetWorkflow(item);
            UpdateContentItemWorkflowState(workflow.LastState.StateID, item);

            // Logs
            AddWorkflowCommentLog(item, userId, comment);
            AddWorkflowLog(item, ContentWorkflowLogType.WorkflowApproved, userId);
        }
        #endregion

        protected override Func<IWorkflowEngine> GetFactory()
        {
            return () => new WorkflowEngine();
        }
    }
}
