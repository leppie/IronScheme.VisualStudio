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
using Microsoft.VisualStudio.Package;
using Babel.Parser;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Babel
{
	public static partial class Configuration
	{
        public const string Name = "IronScheme";

        static CommentInfo myCInfo;
        public static CommentInfo MyCommentInfo { get { return myCInfo; } }

        static Configuration()
        {
            myCInfo.BlockEnd = "|#";
            myCInfo.BlockStart = "#|";
            myCInfo.LineStart = ";";
            myCInfo.UseLineComments = true;

            // default colors - currently, these need to be declared
            CreateColor("Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Identifier", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("String", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Number", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK);
            TokenColor text = CreateColor("Text", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);

            TokenColor error = CreateColor("Error", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, false, false);

            TokenColor proc = CreateColor("User Types", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK);

            TokenColor subform = CreateColor("Auxilliary syntax", COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK);
            TokenColor record = CreateColor("Record", COLORINDEX.CI_PURPLE, COLORINDEX.CI_USERTEXT_BK);
            TokenColor delimiter = CreateColor("Delimiter", COLORINDEX.CI_DARKBLUE, COLORINDEX.CI_USERTEXT_BK);

            //
            // map tokens to color classes
            //
            ColorToken((int)Tokens.SYMBOL, TokenType.Identifier, TokenColor.Identifier, TokenTriggers.ParameterStart);
            ColorToken((int)Tokens.CHARACTER, TokenType.Literal, TokenColor.String, TokenTriggers.MemberSelect);
            ColorToken((int)Tokens.STRING, TokenType.String, TokenColor.String, TokenTriggers.Parameter);
            ColorToken((int)Tokens.NUMBER, TokenType.Literal, TokenColor.Number, TokenTriggers.Parameter);
            ColorToken((int)Tokens.LITERAL, TokenType.Literal, TokenColor.Number, TokenTriggers.Parameter);

            ColorToken((int)Tokens.FORM, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.SUBFORM, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.RECORD, TokenType.Keyword, record, TokenTriggers.None);
            
            ColorToken((int)Tokens.DEFINE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.DEFINECONDITIONTYPE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.DEFINEENUMERATION, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.DEFINERECORDTYPE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.DEFINESYNTAX, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.LIBRARY, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.IMPORT, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.EXPORT, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.MODULE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);

            ColorToken((int)Tokens.PROCEDURE, TokenType.Identifier, proc, TokenTriggers.None);

            ColorToken((int)Tokens.LBRACE, TokenType.Delimiter, delimiter, TokenTriggers.MatchBraces | TokenTriggers.ParameterStart);
            ColorToken((int)Tokens.RBRACE, TokenType.Delimiter, delimiter, TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd);
            ColorToken((int)Tokens.LBRACK, TokenType.Delimiter, delimiter, TokenTriggers.MatchBraces | TokenTriggers.ParameterStart);
            ColorToken((int)Tokens.RBRACK, TokenType.Delimiter, delimiter, TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd);

            ColorToken((int)Tokens.VECTORLBRACE, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.BYTEVECTORLBRACE, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.SYNTAX, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.UNSYNTAX, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.UNSYNTAXSPLICING, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.QUOTE, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.QUASISYNTAX, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.QUASIQUOTE, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.UNQUOTE, TokenType.Delimiter, delimiter, TokenTriggers.None);
            ColorToken((int)Tokens.UNQUOTESPLICING, TokenType.Delimiter, delimiter, TokenTriggers.None);



            //// Extra token values internal to the scanner
            ColorToken((int)Tokens.LEX_WHITE, TokenType.Text, text, TokenTriggers.ParameterNext);
            ColorToken((int)Tokens.LEX_ERROR, TokenType.Text, error, TokenTriggers.None);
            ColorToken((int)Tokens.LEX_COMMENT, TokenType.Comment, TokenColor.Comment, TokenTriggers.None);

        }
    }
}