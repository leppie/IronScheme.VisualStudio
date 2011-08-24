﻿/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using VSLangProj;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;

namespace IronScheme.VisualStudio.Project
{
	/// <summary>
	/// This class extends the ProjectNode in order to represent our project 
	/// within the hierarchy.
	/// </summary>
	[Guid("6FC514F7-6F4D-4FD4-95ED-F37F61E798EE")]
	public class IronSchemeProjectNode : ProjectNode
	{
		#region Enum for image list
		internal enum IronSchemeProjectImageName
		{
			Project = 0,
		}
		#endregion

		#region Constants
		internal const string ProjectTypeName = "IronScheme";
		#endregion

		#region Fields
    private IronSchemeProjectPackage package;
		internal static int imageOffset;
		private static ImageList imageList;
		private VSLangProj.VSProject vsProject;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes the <see cref="IronSchemeProjectNode"/> class.
		/// </summary>
		static IronSchemeProjectNode()
		{
			imageList = Utilities.GetImageList(typeof(IronSchemeProjectNode).Assembly.GetManifestResourceStream("IronScheme.VisualStudio.Project.Resources.IronSchemeProjectImageList.bmp"));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IronSchemeProjectNode"/> class.
		/// </summary>
		/// <param name="package">Value of the project package for initialize internal package field.</param>
    public IronSchemeProjectNode(IronSchemeProjectPackage package)
		{
			this.package = package;

			InitializeImageList();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the image list.
		/// </summary>
		/// <value>The image list.</value>
		public static ImageList ImageList
		{
			get
			{
				return imageList;
			}
			set
			{
				imageList = value;
			}
		}

		protected internal VSLangProj.VSProject VSProject
		{
			get
			{
				if(vsProject == null)
				{
					vsProject = new OAVSProject(this);
				}

				return vsProject;
			}
		}
		#endregion

		#region Overriden implementation
		/// <summary>
		/// Gets the project GUID.
		/// </summary>
		/// <value>The project GUID.</value>
		public override Guid ProjectGuid
		{
			get { return typeof(IronSchemeProjectFactory).GUID; }
		}

		/// <summary>
		/// Gets the type of the project.
		/// </summary>
		/// <value>The type of the project.</value>
		public override string ProjectType
		{
			get { return ProjectTypeName; }
		}

		/// <summary>
		/// Return an imageindex
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		public override int ImageIndex
		{
			get
			{
				return imageOffset + (int)IronSchemeProjectImageName.Project;
			}
		}

		/// <summary>
		/// Returns an automation object representing this node
		/// </summary>
		/// <returns>The automation object</returns>
		public override object GetAutomationObject()
		{
			return new OAIronSchemeProject(this);
		}

		/// <summary>
		/// Creates the file node.
		/// </summary>
		/// <param name="item">The project element item.</param>
		/// <returns></returns>
		public override FileNode CreateFileNode(ProjectElement item)
		{
			IronSchemeProjectFileNode node = new IronSchemeProjectFileNode(this, item);

			node.OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
			node.OleServiceProvider.AddService(typeof(ProjectItem), node.ServiceCreator, false);
			node.OleServiceProvider.AddService(typeof(VSProject), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);

			return node;
		}

		/// <summary>
		/// Generate new Guid value and update it with GeneralPropertyPage GUID.
		/// </summary>
		/// <returns>Returns the property pages that are independent of configuration.</returns>
		protected override Guid[] GetConfigurationIndependentPropertyPages()
		{
			Guid[] result = new Guid[1];
			result[0] = typeof(GeneralPropertyPage).GUID;
			return result;
		}

		/// <summary>
		/// Overriding to provide project general property page.
		/// </summary>
		/// <returns>Returns the GeneralPropertyPage GUID value.</returns>
		protected override Guid[] GetPriorityProjectDesignerPages()
		{
			Guid[] result = new Guid[1];
			result[0] = typeof(GeneralPropertyPage).GUID;
			return result;
		}

    protected override ProjectLoadOption CheckProjectForSecurity(
          ProjectSecurityChecker projectSecurityChecker,
          ProjectSecurityChecker userProjectSecurityChecker)
    {
      return ProjectLoadOption.LoadNormally;
    }


		/// <summary>
		/// Adds the file from template.
		/// </summary>
		/// <param name="source">The source template.</param>
		/// <param name="target">The target file.</param>
		public override void AddFileFromTemplate(string source, string target)
		{
			if(!File.Exists(source))
			{
				throw new FileNotFoundException(string.Format("Template file not found: {0}", source));
			}

			// The class name is based on the new file name
			string className = Path.GetFileNameWithoutExtension(target);

			this.FileTemplateProcessor.AddReplace("%libraryname%", className);
			try
			{
				this.FileTemplateProcessor.UntokenFile(source, target);

				this.FileTemplateProcessor.Reset();
			}
			catch(Exception e)
			{
				throw new FileLoadException("Failed to add template file to project", target, e);
			}
		}
		#endregion

		#region Private implementation
		private void InitializeImageList()
		{
			imageOffset = this.ImageHandler.ImageList.Images.Count;

			foreach(Image img in ImageList.Images)
			{
				this.ImageHandler.AddImage(img);
			}
		}

		private object CreateServices(Type serviceType)
		{
			object service = null;
			if(typeof(VSLangProj.VSProject) == serviceType)
			{
				service = this.VSProject;
			}
			else if(typeof(EnvDTE.Project) == serviceType)
			{
				service = this.GetAutomationObject();
			}
			return service;
		}
		#endregion
	}
}