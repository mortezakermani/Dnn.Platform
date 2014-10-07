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

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Entities.Content.Workflow
{
    public interface IContentWorkflowController
    {
        void StartWorkflow(int workflowID, int itemID, int userID);

        [Obsolete("Obsoleted in Platform 7.4.0")]
        void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID);

        void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID, string source, params string[] parameters);

        void DiscardState(int itemID, string subject, string body, string comment, int portalID, int userID);

        bool IsWorkflowCompleted(int itemID);

        bool IsWorkflowOnDraft(int itemID);
        
        void SendWorkflowNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment,
                              int userID);

        void DiscardWorkflow(int contentItemId, string comment, int portalId, int userId);

        void CompleteWorkflow(int contentItemId, string comment, int portalId, int userId);

        [Obsolete("Obsoleted in Platform 7.4.0")]
        string ReplaceNotificationTokens(string text, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, int portalID, int userID, string comment = "");
        
        ContentWorkflowSource GetWorkflowSource(int workflowId, string sourceName);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowRepository")]
        IEnumerable<ContentWorkflow> GetWorkflows(int portalID);

        [Obsolete("Obsoleted in Platform 7.4.0")]
        ContentWorkflow GetDefaultWorkflow(int portalID);
        
        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowController")]
        ContentWorkflow GetWorkflowByID(int workflowID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowController")]
        ContentWorkflow GetWorkflow(ContentItem item);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowController")]
        void AddWorkflow(ContentWorkflow workflow);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowController")]
        void UpdateWorkflow(ContentWorkflow workflow);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowLogController")]
        IEnumerable<ContentWorkflowLog> GetWorkflowLogs(int workflowId, int contentItemId);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowLogController")]
        void DeleteWorkflowLogs(int workflowID, int contentItemID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowStateController")]
        IEnumerable<ContentWorkflowState> GetWorkflowStates(int workflowID);
        
        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowStateController")]
        ContentWorkflowState GetWorkflowStateByID(int stateID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowStateController")]
        void AddWorkflowState(ContentWorkflowState state);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowStateController")]
        void UpdateWorkflowState(ContentWorkflowState state);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowStatePermissionsController")]
        IEnumerable<ContentWorkflowStatePermission> GetWorkflowStatePermissionByState(int stateID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowStatePermissionsController")]
        void AddWorkflowStatePermission(ContentWorkflowStatePermission permission, int lastModifiedByUserID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowStatePermissionsController")]
        void UpdateWorkflowStatePermission(ContentWorkflowStatePermission permission, int lasModifiedByUserId);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowStatePermissionsController")]
        void DeleteWorkflowStatePermission(int workflowStatePermissionID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowSecurityController")]
        bool IsAnyReviewer(int portalID, int userID, int workflowID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowSecurityController")]
        bool IsAnyReviewer(int workflowID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowSecurityController")]
        bool IsCurrentReviewer(int portalId, int userID, int itemID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowSecurityController")]
        bool IsCurrentReviewer(int itemID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowSecurityController")]
        bool IsReviewer(int portalId, int userID, int stateID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowSecurityController")]
        bool IsReviewer(int stateID);

        [Obsolete("Obsoleted in Platform 7.4.0. Use instead IWorkflowLogController")]
        void AddWorkflowLog(ContentItem item, string action, string comment, int userID);

        [Obsolete("Obsoleted in Platform 7.4.0")]
        void CreateDefaultWorkflows(int portalId);
    }
}