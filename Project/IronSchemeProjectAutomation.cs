/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Project;

namespace IronScheme.VisualStudio.Project
{
	[ComVisible(true)]
	public class OAIronSchemeProject : OAProject
	{
		#region Constructors
		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="project">Custom project.</param>
		public OAIronSchemeProject(IronSchemeProjectNode project)
			: base(project)
		{
		}
		#endregion
	}

	[ComVisible(true)]
	[Guid("D7EDB436-6F5A-4EF4-9E3F-67C15C2FA30E")]
	public class OAIronSchemeProjectFileItem : OAFileItem
	{
		#region Constructors
		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="project">Automation project.</param>
		/// <param name="node">Custom file node.</param>
		public OAIronSchemeProjectFileItem(OAProject project, FileNode node)
			: base(project, node)
		{
		}
		#endregion
	}
}
