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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;
using System.Linq;
using IronScheme.VisualStudio;

namespace Babel
{
	class AuthoringScope : Microsoft.VisualStudio.Package.AuthoringScope
	{
    ParseRequest req;
    Source source;
    string imports;

		internal AuthoringScope(object parseResult, List<Identifier> ids, ParseRequest req, Source source, string imports)
		{
			this.parseResult = parseResult;
      this.ids = ids;
      this.req = req;
      this.imports = imports;
		}

		object parseResult;
    List<Identifier> ids;




    int GetGlyph(SymbolBinding x)
    {
      switch (x.Type)
      {
        case BindingType.Procedure:
          return 72;
        case BindingType.Syntax:
          return 36;
        case BindingType.Record:
          return 0;
      }

      return 0;
    }



		// ParseReason.QuickInfo
		public override string GetDataTipText(int line, int col, out TextSpan span)
		{
      // not 1 based...
			return FindQuickInfo(line, col, out span);
		}

    string FindQuickInfo(int line, int col, out TextSpan span)
    {
      var simp = imports;

      var id = ids.SingleOrDefault(x => x.Location.iStartLine <= line &&
                                        x.Location.iEndLine >= line &&
                                        x.Location.iStartIndex <= col &&
                                        x.Location.iEndIndex >= col);

      if (id != null && !string.IsNullOrEmpty(simp))
      {
        span = id.Location;

        var sb = new SymbolBindingService();

        var bi = sb.GetBindings(simp).SingleOrDefault(x => x.Name == id.Name);
        if (bi != null)
        {
          if (bi.Type == BindingType.Procedure)
          {
            var pi = sb.GetProcedureInfo(id.Name, simp);

            return bi.Type + Environment.NewLine + string.Join(Environment.NewLine, pi.Forms);
          }
          else
          {
            return bi.Type + Environment.NewLine + bi.Name;
          }
        }
      }

      span = new TextSpan();
      return "unknown";
    }

		// ParseReason.CompleteWord
		// ParseReason.DisplayMemberList
		// ParseReason.MemberSelect
		// ParseReason.MemberSelectAndHilightBraces
		public override Microsoft.VisualStudio.Package.Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
		{
			IList<Declaration> declarations;
			switch (reason)
			{
				case ParseReason.CompleteWord:
          string s;
          int r = view.GetTextStream(line, info.StartIndex, line, col, out s);
					declarations = FindCompletions(s, line, col);
					break;
				case ParseReason.DisplayMemberList:
				case ParseReason.MemberSelect:
				case ParseReason.MemberSelectAndHighlightBraces:
					declarations = FindMembers(line, col);
					break;
				default:
					throw new ArgumentException("reason");
			}

			return new Declarations(declarations);
		}

    IList<Babel.Declaration> FindCompletions(string text, int line, int col)
    {
      var simp = imports;

      if (string.IsNullOrEmpty(simp))
      {
        return new List<Babel.Declaration>();
      }

      var sb = new SymbolBindingService();
      var bindings = sb.GetBindings(simp);

      if (text.StartsWith("("))
      {
        return bindings.Select(x => new Declaration(x.Name, x.Name, GetGlyph(x), x.Name)).OrderBy(x => x.DisplayText).ToList();
      }

      return bindings.Where(x => x.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase)).
        Select(x => new Declaration(x.Name, x.Name, GetGlyph(x), x.Name)).OrderBy(x => x.DisplayText).ToList();
    }

    IList<Babel.Declaration> FindMembers(int line, int col)
    {
      // ManagedMyC.Parser.AAST aast = result as ManagedMyC.Parser.AAST;
      List<Babel.Declaration> members = new List<Babel.Declaration>();

      //foreach (string state in aast.startStates.Keys)
      //    members.Add(new Declaration(state, state, 0, state));

      return members;
    }





		// ParseReason.GetMethods
		public override Microsoft.VisualStudio.Package.Methods GetMethods(int line, int col, string name)
		{
			return new Methods(FindMethods(line, col, name));
		}

    IList<Babel.Method> FindMethods(int line, int col, string name)
    {
      return new List<Babel.Method>();
    }

		// ParseReason.Goto
		public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
		{
			// throw new System.NotImplementedException();
			span = new TextSpan();
			return null;
		}
	}
}