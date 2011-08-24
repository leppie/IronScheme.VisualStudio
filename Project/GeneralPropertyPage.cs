/// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;

namespace IronScheme.VisualStudio.Project
{
	/// <summary>
	/// This class implements general property page for the project type.
	/// </summary>
	[ComVisible(true)]
	[Guid("5F9F1697-2E61-4c10-9AD2-94FA2A9BAAE9")]
	public class GeneralPropertyPage : SettingsPage
	{
		#region Fields
		private string assemblyName;
		private OutputType outputType;
		private string defaultNamespace;
		private string startupObject;
		private string applicationIcon;
		private PlatformType targetPlatform = PlatformType.v2;
		private string targetPlatformLocation;
		#endregion Fields

		#region Constructors
		/// <summary>
		/// Explicitly defined default constructor.
		/// </summary>
		public GeneralPropertyPage()
		{
			this.Name = Resources.GetString(Resources.GeneralCaption);
		}
		#endregion

		#region Properties
		[ResourcesCategoryAttribute(Resources.AssemblyName)]
		[LocDisplayName(Resources.AssemblyName)]
		[ResourcesDescriptionAttribute(Resources.AssemblyNameDescription)]
		/// <summary>
		/// Gets or sets Assembly Name.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public string AssemblyName
		{
			get { return this.assemblyName; }
			set { this.assemblyName = value; this.IsDirty = true; }
		}

		[ResourcesCategoryAttribute(Resources.Application)]
		[LocDisplayName(Resources.OutputType)]
		[ResourcesDescriptionAttribute(Resources.OutputTypeDescription)]
		/// <summary>
		/// Gets or sets OutputType.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public OutputType OutputType
		{
			get { return this.outputType; }
			set { this.outputType = value; this.IsDirty = true; }
		}

		[ResourcesCategoryAttribute(Resources.Application)]
		[LocDisplayName(Resources.DefaultNamespace)]
		[ResourcesDescriptionAttribute(Resources.DefaultNamespaceDescription)]
		/// <summary>
		/// Gets or sets Default Namespace.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public string DefaultNamespace
		{
			get { return this.defaultNamespace; }
			set { this.defaultNamespace = value; this.IsDirty = true; }
		}

		[ResourcesCategoryAttribute(Resources.Application)]
		[LocDisplayName(Resources.StartupObject)]
		[ResourcesDescriptionAttribute(Resources.StartupObjectDescription)]
		/// <summary>
		/// Gets or sets Startup Object.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public string StartupObject
		{
			get { return this.startupObject; }
			set { this.startupObject = value; this.IsDirty = true; }
		}

		[ResourcesCategoryAttribute(Resources.Application)]
		[LocDisplayName(Resources.ApplicationIcon)]
		[ResourcesDescriptionAttribute(Resources.ApplicationIconDescription)]
		/// <summary>
		/// Gets or sets Application Icon.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public string ApplicationIcon
		{
			get { return this.applicationIcon; }
			set { this.applicationIcon = value; this.IsDirty = true; }
		}

		[ResourcesCategoryAttribute(Resources.Project)]
		[LocDisplayName(Resources.ProjectFile)]
		[ResourcesDescriptionAttribute(Resources.ProjectFileDescription)]
		/// <summary>
		/// Gets the path to the project file.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public string ProjectFile
		{
			get { return Path.GetFileName(this.ProjectMgr.ProjectFile); }
		}

		[ResourcesCategoryAttribute(Resources.Project)]
		[LocDisplayName(Resources.ProjectFolder)]
		[ResourcesDescriptionAttribute(Resources.ProjectFolderDescription)]
		/// <summary>
		/// Gets the path to the project folder.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public string ProjectFolder
		{
			get { return Path.GetDirectoryName(this.ProjectMgr.ProjectFolder); }
		}

		[ResourcesCategoryAttribute(Resources.Project)]
		[LocDisplayName(Resources.OutputFile)]
		[ResourcesDescriptionAttribute(Resources.OutputFileDescription)]
		/// <summary>
		/// Gets the output file name depending on current OutputType.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public string OutputFile
		{
			get
			{
				switch(this.outputType)
				{
					case OutputType.Exe:
					case OutputType.WinExe:
						{
							return this.assemblyName + ".exe";
						}

					default:
						{
							return this.assemblyName + ".dll";
						}
				}
			}
		}

		[ResourcesCategoryAttribute(Resources.Project)]
		[LocDisplayName(Resources.TargetPlatform)]
		[ResourcesDescriptionAttribute(Resources.TargetPlatformDescription)]
		/// <summary>
		/// Gets or sets Target Platform PlatformType.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public PlatformType TargetPlatform
		{
			get { return this.targetPlatform; }
			set { this.targetPlatform = value; IsDirty = true; }
		}

		[ResourcesCategoryAttribute(Resources.Project)]
		[LocDisplayName(Resources.TargetPlatformLocation)]
		[ResourcesDescriptionAttribute(Resources.TargetPlatformLocationDescription)]
		/// <summary>
		/// Gets or sets Target Platform Location.
		/// </summary>
		/// <remarks>IsDirty flag was switched to true.</remarks>
		public string TargetPlatformLocation
		{
			get { return this.targetPlatformLocation; }
			set { this.targetPlatformLocation = value; IsDirty = true; }
		}

		#endregion

		#region Overriden Implementation
		/// <summary>
		/// Returns class FullName property value.
		/// </summary>
		public override string GetClassName()
		{
			return this.GetType().FullName;
		}

		/// <summary>
		/// Bind properties.
		/// </summary>
		protected override void BindProperties()
		{
			if(this.ProjectMgr == null)
			{
				return;
			}

			this.assemblyName = this.ProjectMgr.GetProjectProperty("AssemblyName", true);

			string outputType = this.ProjectMgr.GetProjectProperty("OutputType", false);

			if(outputType != null && outputType.Length > 0)
			{
				try
				{
					this.outputType = (OutputType)Enum.Parse(typeof(OutputType), outputType);
				}
				catch(ArgumentException)
				{
				}
			}

			this.defaultNamespace = this.ProjectMgr.GetProjectProperty("RootNamespace", false);
			this.startupObject = this.ProjectMgr.GetProjectProperty("StartupObject", false);
			this.applicationIcon = this.ProjectMgr.GetProjectProperty("ApplicationIcon", false);

			string targetPlatform = this.ProjectMgr.GetProjectProperty("TargetPlatform", false);

			if(targetPlatform != null && targetPlatform.Length > 0)
			{
				try
				{
					this.targetPlatform = (PlatformType)Enum.Parse(typeof(PlatformType), targetPlatform);
				}
				catch(ArgumentException)
				{
				}
			}

			this.targetPlatformLocation = this.ProjectMgr.GetProjectProperty("TargetPlatformLocation", false);
		}

		/// <summary>
		/// Apply Changes on project node.
		/// </summary>
		/// <returns>E_INVALIDARG if internal ProjectMgr is null, otherwise applies changes and return S_OK.</returns>
		protected override int ApplyChanges()
		{
			if(this.ProjectMgr == null)
			{
				return VSConstants.E_INVALIDARG;
			}

			this.ProjectMgr.SetProjectProperty("AssemblyName", this.assemblyName);
			this.ProjectMgr.SetProjectProperty("OutputType", this.outputType.ToString());
			this.ProjectMgr.SetProjectProperty("RootNamespace", this.defaultNamespace);
			this.ProjectMgr.SetProjectProperty("StartupObject", this.startupObject);
			this.ProjectMgr.SetProjectProperty("ApplicationIcon", this.applicationIcon);
			this.ProjectMgr.SetProjectProperty("TargetPlatform", this.targetPlatform.ToString());
			this.ProjectMgr.SetProjectProperty("TargetPlatformLocation", this.targetPlatformLocation);
			this.IsDirty = false;

			return VSConstants.S_OK;
		}
		#endregion
	}
}