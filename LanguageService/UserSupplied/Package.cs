/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace Babel
{
    /*
     * The Babel.Package class is needed to register the VS package and be the entry point for the language service.
     * This class derives from the Babel.BabelPackage base class which provides all the necessary functionality for a
     * babel-based language service.  This class can be used to override and extend that base class if necessary.
     * Note that the Babel.BabelPackage class derives from the Managed Package Framework's Package class.
     *     
     * Of special interest is the GUID attribute that is used to uniquely identify this package.  
     * If this code is copied for a different package, then the GUID should be regenerated
     * so to not interfere with this sample package's GUID.
     */

    [PackageRegistration(UseManagedResourcesOnly=true)]
    [DefaultRegistryRoot(@"Software\Microsoft\VisualStudio\9.0")]
    [ProvideService(typeof(Babel.LanguageService))]
    [ProvideLanguageExtension(typeof(Babel.LanguageService), ".ss")]
    [ProvideLanguageExtension(typeof(Babel.LanguageService), ".sls")]
    [ProvideLanguageExtension(typeof(Babel.LanguageService), ".sps")]
    [ProvideLanguageExtension(typeof(Babel.LanguageService), ".scm")]
    [ProvideLanguageService(typeof(Babel.LanguageService), Configuration.Name, 0,
        CodeSense = true,
        EnableLineNumbers=true,
        DefaultToInsertSpaces=true,
        ShowDropDownOptions=true,
        EnableCommenting = true,
        MatchBraces = true, 
        MatchBracesAtCaret=true,
        ShowCompletion = true,
        ShowMatchingBrace = false,
        AutoOutlining = true,
        EnableAsyncCompletion = true,
        CodeSenseDelay = 0)]
    [ProvideLoadKey("Standard", "1.0.0.0", "IronScheme.VisualStudio", "IronScheme", 104)]
    [InstalledProductRegistration(true, null, null, null)]
    [Guid("0BC23FCA-38BA-4cc2-9C39-1ED1C46172CB")]
    class Package : BabelPackage, Microsoft.VisualStudio.Shell.Interop.IVsInstalledProduct
    {
      #region Overriden Implentation
      /// <summary>
      /// Initialization of the package; this method is called right after the package is sited, so this is the place
      /// where you can put all the initilaization code that rely on services provided by VisualStudio.
      /// </summary>
      protected override void Initialize()
      {
        base.Initialize();
      }
      #endregion

      #region IVsInstalledProduct Members

      public int IdBmpSplash(out uint pIdBmp)
      {
        pIdBmp = 300;
        return VSConstants.S_OK;
      }

      public int IdIcoLogoForAboutbox(out uint pIdIco)
      {
        pIdIco = 400;
        return VSConstants.S_OK;
      }

      public int OfficialName(out string pbstrName)
      {
        pbstrName = "IronScheme.VisualStudio";
        return VSConstants.S_OK;
      }

      public int ProductDetails(out string pbstrProductDetails)
      {
        pbstrProductDetails = "IronScheme Language plugin for VS2008";
        return VSConstants.S_OK;
      }

      public int ProductID(out string pbstrPID)
      {
        pbstrPID = "1.0";
        return VSConstants.S_OK;
      }

      #endregion
    }

}
