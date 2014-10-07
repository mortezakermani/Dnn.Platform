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
using System.ComponentModel.DataAnnotations;

namespace DotNetNuke.Entities.Content.Workflow
{
    [PetaPoco.PrimaryKey("StateID")]
    public class ContentWorkflowState 
    {
        public int StateID { get; set; }

        public int WorkflowID { get; set; }

        [Required]
        [StringLength(40)]
        public string StateName { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; }

        public bool IsSystem { get; set; }

        public bool SendNotification { get; set; }

        public bool SendNotificationToAdministrators { get; set; }

        [Obsolete("Obsoleted in Platform 7.4.0")]
        [PetaPoco.Ignore]
        public bool SendEmail { get; set; }

        [Obsolete("Obsoleted in Platform 7.4.0")]
        [PetaPoco.Ignore]
        public bool SendMessage { get; set; } 

        [Obsolete("Obsoleted in Platform 7.4.0")]
        public bool IsDisposalState { get; set; }

        [Obsolete("Obsoleted in Platform 7.4.0")]
        [PetaPoco.Ignore]
        public string OnCompleteMessageSubject { get; set; }

        [Obsolete("Obsoleted in Platform 7.4.0")]
        [PetaPoco.Ignore]
        public string OnCompleteMessageBody { get; set; }

        [Obsolete("Obsoleted in Platform 7.4.0")]
        [PetaPoco.Ignore]
        public string OnDiscardMessageSubject { get; set; }

        [Obsolete("Obsoleted in Platform 7.4.0")]
        [PetaPoco.Ignore]
        public string OnDiscardMessageBody { get; set; }
    }
}
