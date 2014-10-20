using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Entities.Content.Workflow
{
    [Obsolete("Obsoleted in Platform 7.4.0.")]
    public interface IContentWorkflowAction
    {
        string GetAction(string[] parameters);
    }
}
