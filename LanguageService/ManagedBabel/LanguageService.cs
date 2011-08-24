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
using System.Runtime.InteropServices;

namespace Babel
{
	public abstract class BabelLanguageService : Microsoft.VisualStudio.Package.LanguageService
	{
		#region Custom Colors
		public override int GetColorableItem(int index, out IVsColorableItem item)
		{
			if (index <= Configuration.ColorableItems.Count)
			{
				item = Configuration.ColorableItems[index - 1];
				return Microsoft.VisualStudio.VSConstants.S_OK;
			}
			else
			{
				throw new ArgumentNullException("index");
			}
		}

		public override int GetItemCount(out int count)
		{
			count = Configuration.ColorableItems.Count;
			return Microsoft.VisualStudio.VSConstants.S_OK;
		}
		#endregion

		#region MPF Accessor and Factory specialisation
		private LanguagePreferences preferences;
		public override LanguagePreferences GetLanguagePreferences()
		{
			if (this.preferences == null)
			{
				this.preferences = new LanguagePreferences(this.Site,
														typeof(Babel.LanguageService).GUID,
														this.Name);
				this.preferences.Init();
			}

			return this.preferences;
		}

		public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
		{
			return new Source(this, buffer, this.GetColorizer(buffer));
		}

    Dictionary<IVsTextBuffer, IScanner> linescanners = new Dictionary<IVsTextBuffer, IScanner>();

		public override IScanner GetScanner(IVsTextLines buffer)
		{
      IScanner scanner;
      if (!linescanners.TryGetValue(buffer, out scanner))
      {
        scanner = linescanners[buffer] = new LineScanner(this, buffer);
      }
			return scanner;
		}
		#endregion

		public override void OnIdle(bool periodic)
		{
			// from IronPythonLanguage sample
			// this appears to be necessary to get a parse request with ParseReason = Check?
			Source src = (Source) GetSource(this.LastActiveTextView);
			if (src != null && src.LastParseTime >= Int32.MaxValue >> 12)
			{
				src.LastParseTime = 0;
			}
			base.OnIdle(periodic);
		}

    public override int ValidateBreakpointLocation(IVsTextBuffer buffer, int line, int col, TextSpan[] pCodeSpan)
    {
      pCodeSpan[0] = new TextSpan { iStartLine = line, iStartIndex = col, iEndLine = line, iEndIndex = col };
      return VSConstants.E_NOTIMPL;
      //return base.ValidateBreakpointLocation(buffer, line, col, pCodeSpan);
    }

    string[] lines = {};
    List<Identifier> ids = new List<Identifier>();
    string imports = null;

    Dictionary<IVsTextLines, string> fileimports = new Dictionary<IVsTextLines, string>();

    internal string GetImports(IVsTextLines buffer)
    {
      string c;
      fileimports.TryGetValue(buffer, out c);
      return c;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    static extern bool RedrawWindow(IntPtr hwnd, IntPtr rcUpdate, IntPtr hrgnUpdate, int flags);
 
    public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource(ParseRequest req)
    {
      Source source = (Source)this.GetSource(req.FileName);
      bool yyparseResult = false;

      // req.DirtySpan seems to be set even though no changes have occurred
      // source.IsDirty also behaves strangely
      // might be possible to use source.ChangeCount to sync instead

      if (req.DirtySpan.iStartIndex != req.DirtySpan.iEndIndex
          || req.DirtySpan.iStartLine != req.DirtySpan.iEndLine)
      {
        Babel.Parser.ErrorHandler handler = new Babel.Parser.ErrorHandler();
        Babel.Lexer.Scanner scanner = new Babel.Lexer.Scanner(); // string interface
        Parser.Parser parser = new Parser.Parser();  // use noarg constructor

        parser.scanner = scanner;
        scanner.parser = parser;
        scanner.Handler = handler;
        parser.SetHandler(handler);

        lines = req.Text.Split('\n');

        scanner.SetSource(req.Text.Replace("\n", "\r\n"), 0);

        parser.MBWInit(req);
        yyparseResult = parser.Parse();

        ids = parser.ids;

        // store the parse results
        // source.ParseResult = aast;
        if (parser.result != null)
        {
          source.ParseResult = parser.result;
        }

        var simp = parser.imports;

        if (imports != simp)
        {
          var handle = req.View.GetWindowHandle();
          RedrawWindow(handle, IntPtr.Zero, IntPtr.Zero, 113);
        }

        imports = simp;

        var tl = source.GetTextLines();

        fileimports[tl] = imports;

        source.Braces = parser.Braces;

        // for the time being, just pull errors back from the error handler
        if (handler.ErrNum > 0)
        {
          foreach (Babel.Parser.Error error in handler.SortedErrorList())
          {
            TextSpan span = new TextSpan();
            span.iStartLine = span.iEndLine = error.line - 1;
            span.iStartIndex = error.column;
            span.iEndIndex = error.column + error.length;
            req.Sink.AddError(req.FileName, error.message, span, Severity.Error);
          }
        }
      }

      switch (req.Reason)
      {
        case ParseReason.Check:
        case ParseReason.HighlightBraces:
        case ParseReason.MatchBraces:
        case ParseReason.MemberSelectAndHighlightBraces:
          // send matches to sink
          // this should (probably?) be filtered on req.Line / col
          if (source.Braces != null)
          {
            foreach (TextSpan[] brace in source.Braces)
            {
              if (brace.Length == 2)
                req.Sink.MatchPair(brace[0], brace[1], 1);
              else if (brace.Length >= 3)
                req.Sink.MatchTriple(brace[0], brace[1], brace[2], 1);
            }
          }
          break;
        case ParseReason.MethodTip:

          break;
        default:
          break;
      }

      return new AuthoringScope(source.ParseResult, ids, req, source, imports);
    }

		public override string Name
		{
			get { return Configuration.Name; }
		}
	}
}