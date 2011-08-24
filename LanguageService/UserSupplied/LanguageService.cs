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
using Microsoft.VisualStudio.Package;
using System.Collections;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Babel
{
  /*
   * The Babel.LanguageService class is needed to register the VS language service.  
   * This class derives from the Babel.BabelLanguageService base class which provides all the necessary 
   * functionality for a babel-based language service.  This class can be used to override and extend that 
   * base class if necessary.
   * 
   * Note that the Babel.BabelLanguageService class derives from the Managed 
   * Package Framework's LanguageService class.
   *     
   * Of special interest is the GUID attribute that is used to uniquely identify this language service.  
   * If this code is copied for a different language service, then the GUID should be regenerated
   * so to not interfere with this sample language service's GUID.
   */

  [Guid("4BFAEA21-B66A-458b-BC32-24457C9178B7")]
  class LanguageService : BabelLanguageService
  {
    public override string GetFormatFilterList()
    {
      return "IronScheme File (*.ss)\n*.ss";
    }

    public override TypeAndMemberDropdownBars CreateDropDownHelper(IVsTextView forView)
    {
      return new Options(this);
    }
  }

  class Options : TypeAndMemberDropdownBars
  {
    public Options(LanguageService s)
      : base(s)
    {

    }

    int sellib, selmem;

    public override int OnItemChosen(int combo, int entry)
    {
      sellib = Math.Max(combo - 1, 0);
      selmem = entry;
      return base.OnItemChosen(combo, entry);
    }

    object last;

    public override bool OnSynchronizeDropdowns(Microsoft.VisualStudio.Package.LanguageService languageService, IVsTextView textView, int line, int col,
      ArrayList dropDownTypes, ArrayList dropDownMembers, ref int selectedType, ref int selectedMember)
    {
      Source s = languageService.GetSource(textView) as Source;

      selectedMember = selmem;
      selectedType = sellib;

      Cons content = s.ParseResult as Cons;

      if (content != null)
      {
        if (content == last)
        {
          return false;
        }
        last = content;

        if (content.cdr == null)
        {
          Cons library = content.car as Cons;
          // (library (name) (export ...) (import ...) body ...)
          for (int i = 0; i < 1; i++)
          {
            library = library.cdr as Cons;
          }

          dropDownTypes.Clear();

          string libname = library.car.ToString();

          dropDownTypes.Add(new DropDownMember(libname, (library.car as CodeElement).Location, 90, DROPDOWNFONTATTR.FONTATTR_BOLD));

          for (int i = 0; i < 3; i++)
          {
            library = library.cdr as Cons;
          }

          AddMembers(dropDownMembers, library);
        }
        else
        {
          dropDownTypes.Clear();

          string libname = "(top-level)";

          dropDownTypes.Add(new DropDownMember(libname, new TextSpan(), 90, DROPDOWNFONTATTR.FONTATTR_BOLD));

          Cons import = content.car as Cons;
          AddMembers(dropDownMembers, content.cdr as Cons);
        }
        return true;

      }
      return false;

    }

    static void AddMembers(ArrayList dropDownMembers, Cons library)
    {
      dropDownMembers.Clear();

      foreach (var def in Cons.GetDefs(library))
      {
        int type = 72;
        if (def is Definition)
        {
          type = (def as Definition).Type;
        }
        else if (def is Library)
        {
          type = 90;
        }
        else if (def is Module)
        {
          type = 90;
        }
        dropDownMembers.Add(new DropDownMember(def.Name, def.Location, type, DROPDOWNFONTATTR.FONTATTR_PLAIN));
      }

      dropDownMembers.Sort();
    }
  }

}
