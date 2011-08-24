%using Babel;
%using Babel.Parser;

%namespace Babel.Lexer

%option stack, minimize

%{
      
       internal void LoadYylval()
       {
           //yylloc = new LexLocation(tokLin, tokCol, tokLin, tokECol);
           //yylval.str = yytext;

       }
       
       public override void yyerror(string s, params object[] a)
       {
           if (handler != null) handler.AddError(s, tokLin, tokCol, tokLin, tokECol);
       }
       
void FixLineNum(string text)
{
  if (text.EndsWith("\n"))
  {
    lNum--;
  }
}

int diff()
{
  return chr == -1 ? 0 : -1;
}

void FixDiff()
{
  if (chr == 32)
  {
    string t = yytext;
    char last = t[t.Length - 1];
    switch (last)
    {
      case ' ':
      case ')':
      case ']':
        break;
       default:
        chr = -1;
        break;
    }
  }
}

public int MakeSymbol()
{
  string t = yytext;
  FixDiff();
  FixLineNum(t);
  t = t.Substring(0, t.Length + diff());
  yylval.str = t;
  yyless(t.Length);
  yylloc = new LexLocation(yyline,yycol,yyline,yycol + yyleng);
  if (ls != null)
  {
    var imports = ls.GetImports();
    return SymbolCache.Lookup(t, imports);
  }
  else
  if (parser == null)
  {
    return Symbols.Get(t);
  }
  else
  {
    return SymbolCache.Lookup(t, parser.imports);
  }
}

public int MakeBoolean()
{
  string t = yytext;
  FixDiff();
  FixLineNum(t);
  t = t.Substring(0, t.Length + diff());
  yylval.str = t.ToLower();
  yyless(t.Length);
  yylloc = new LexLocation(yyline,yycol,yyline,yycol + yyleng);
  return (int)Tokens.LITERAL;
}

public int MakeNumber()
{
  string t = yytext;
  FixDiff();
  FixLineNum(t);
  t = t.Substring(0, t.Length + diff());
  yylval.str = t;
  yyless(t.Length);
  yylloc = new LexLocation(yyline,yycol,yyline,yycol + yyleng);
  return (int)Tokens.NUMBER;
}

public int MakeChar()
{
  string t = yytext;
  FixDiff();
  FixLineNum(t);
  t = t.Substring(0, t.Length + diff());
  yylval.str = t;
  yyless(t.Length);
  yylloc = new LexLocation(yyline,yycol,yyline,yycol + yyleng);
  return (int)Tokens.CHARACTER;
}


public int Make(Tokens token)
{
  yylval.str = yytext.Trim();
  yylloc = new LexLocation(yyline,yycol,yyline,yycol + yylval.str.Length);
  return (int)token;
}

internal Parser.Parser parser;
internal LineScanner ls;

/*



*/
       
%}




delimiter              "\r\n"|[\[\]\(\)\";#\r\n\t ]
but_delimiter          [^\[\]\(\)\";#\r\n\t ]
numbut_delimiter       [^\[\]\(\)\";#\r\n\t i]

line_comment           (";"[^\n]*)|("#!"[^\n]*)

ignore_datum           "#;"

comment_start          "#|"
comment_end            "|#"

white_space            [ \t]
new_line               "\r\n"|\r|\n

digit                  [0-9]
digit2                 [01]
digit8                 [0-7]
digit10                {digit}
digit16                {digit10}|[a-fA-F]

letter                 [[:IsLetter:]]
idescape               ("\\x"{digit16}+";")
idinitial              ("->"|({letter})|{idescape}|[!$%*/:<=>?~_^&])
subsequent             ({idinitial})|{digit}|[\.\+@]|"-"|"[]"
identifier             (({idinitial})({subsequent})*)|"+"|"..."|"-"

good_id                {identifier}{delimiter}
bad_id                 {identifier}{but_delimiter}




radix2                 #[bB]
radix8                 #[oO]
radix10                (#[dD])?
radix16                #[xX]

exactness              (#[iIeE])?

sign                   ("-"|"+")?

exponentmarker         [eEsSfFdDlL]

suffix                 ({exponentmarker}{sign}({digit10})+)?

prefix2                ({radix2}{exactness})|({exactness}{radix2})
prefix8                ({radix8}{exactness})|({exactness}{radix8})
prefix10               ({radix10}{exactness})|({exactness}{radix10})
prefix16               ({radix16}{exactness})|({exactness}{radix16})

uinteger2              ({digit2})+
uinteger8              ({digit8})+
uinteger10             ({digit10})+
uinteger16             ({digit16})+

decimal10              (({uinteger10}{suffix})|("."({digit10})+{suffix})|(({digit10})+"."({digit10})*{suffix}))

ureal2                 (({uinteger2})|({uinteger2}"/"{uinteger2}))
ureal8                 (({uinteger8})|({uinteger8}"/"{uinteger8}))
ureal10                (({decimal10})|({uinteger10}"/"{uinteger10}))
ureal16                (({uinteger16})|({uinteger16}"/"{uinteger16}))

naninf                 ("nan.0"|"inf.0")

real2                  ({sign}{ureal2}|"+"{naninf}|"-"{naninf})
real8                  ({sign}{ureal8}|"+"{naninf}|"-"{naninf})
real10                 ({sign}{ureal10}|"+"{naninf}|"-"{naninf})
real16                 ({sign}{ureal16}|"+"{naninf}|"-"{naninf})

complex2               ({real2}|({real2}"@"{real2})|({real2}"+"{ureal2}"i")|({real2}"-"{ureal2}"i")|({real2}"+i")|({real2}"-i")|("+"{ureal2}"i")|("-"{ureal2}"i")|("+i")|("-i")|({real2}"+"{naninf}"i")|({real2}"-"{naninf}"i")|("+"{naninf}"i")|("-"{naninf}"i"))
complex8               ({real8}|({real8}"@"{real8})|({real8}"+"{ureal8}"i")|({real8}"-"{ureal8}"i")|({real8}"+i")|({real8}"-i")|("+"{ureal8}"i")|("-"{ureal8}"i")|("+i")|("-i")|({real8}"+"{naninf}"i")|({real8}"-"{naninf}"i")|("+"{naninf}"i")|("-"{naninf}"i"))
complex10              ({real10}|({real10}"@"{real10})|({real10}"+"{ureal10}"i")|({real10}"-"{ureal10}"i")|({real10}"+i")|({real10}"-i")|("+"{ureal10}"i")|("-"{ureal10}"i")|("+i")|("-i")|({real10}"+"{naninf}"i")|({real10}"-"{naninf}"i")|("+"{naninf}"i")|("-"{naninf}"i"))
complex16              ({real16}|({real16}"@"{real16})|({real16}"+"{ureal16}"i")|({real16}"-"{ureal16}"i")|({real16}"+i")|({real16}"-i")|("+"{ureal16}"i")|("-"{ureal16}"i")|("+i")|("-i")|({real16}"+"{naninf}"i")|({real16}"-"{naninf}"i")|("+"{naninf}"i")|("-"{naninf}"i"))

num2                   ({prefix2}{complex2})
num8                   ({prefix8}{complex8})
num10                  ({prefix10}{complex10})
num16                  ({prefix16}{complex16})

number                 ({num2}|{num8}|{num10}|{num16})

good_number            {number}{delimiter}
bad_number             {number}{numbut_delimiter}+

good_dot               "."{delimiter}?
bad_dot                "."{but_delimiter}+


single_char            [^\n ]
character              {single_char}
char_hex_esc_seq       (#\\x({digit16})+)
char_esc_seq           (#\\("nul"|alarm|backspace|tab|linefeed|newline|vtab|page|return|esc|space|delete))
character_literal      ((#\\({character})?)|{char_hex_esc_seq}|{char_esc_seq})

good_char              {character_literal}{delimiter}
bad_char               {character_literal}{but_delimiter}+

single_string_char     [^\\\"]
string_esc_seq         (\\[\"\\abfnrtv])
hex_esc_seq            (\\x({digit16})+";")
string_continuation    (\\{new_line})
reg_string_char        {string_continuation}|{single_string_char}|{string_esc_seq}|{hex_esc_seq}
string_literal         \"({reg_string_char})*\"

atoms                  (#[TtFf])
good_atoms             {atoms}{delimiter}
bad_atoms              {atoms}{but_delimiter}+



%x ML_COMMENT

%%


{white_space}+        { return (int)Tokens.LEX_WHITE; }
{new_line}            { return (int)Tokens.LEX_WHITE; }
                     
{comment_start}       { yy_push_state(ML_COMMENT);  return (int)Tokens.LEX_COMMENT;}                      
{line_comment}        { return (int)Tokens.LEX_COMMENT; }


<ML_COMMENT>[^\n\|]+          { return (int)Tokens.LEX_COMMENT; }
<ML_COMMENT>{comment_end}     { yy_pop_state();  return (int)Tokens.LEX_COMMENT;}
<ML_COMMENT>"|"               { return (int)Tokens.LEX_COMMENT; }

{ignore_datum}        { return (int)Tokens.LEX_COMMENT; }
 
{good_atoms}          { return MakeBoolean(); } 

{good_char}           { return MakeChar(); }                      
{string_literal}      { return Make(Tokens.STRING); }
{good_number}         { return MakeNumber(); }

"["                   { return Make(Tokens.LBRACK); }                     
"]"                   { return Make(Tokens.RBRACK); } 

"("                   { return Make(Tokens.LBRACE); }                     
"#("                  { return Make(Tokens.VECTORLBRACE); }                     
"#vu8("               { return Make(Tokens.BYTEVECTORLBRACE); }                     
")"                   { return Make(Tokens.RBRACE); } 

"`"                   { return Make(Tokens.QUASIQUOTE); }
"'"                   { return Make(Tokens.QUOTE); }
",@"                  { return Make(Tokens.UNQUOTESPLICING); }
","                   { return Make(Tokens.UNQUOTE);}


"#'"                  { return Make(Tokens.SYNTAX);}
"#`"                  { return Make(Tokens.QUASISYNTAX);}
"#,@"                 { return Make(Tokens.UNSYNTAXSPLICING);}
"#,"                  { return Make(Tokens.UNSYNTAX);}

{good_id}          { return MakeSymbol(); }
{good_dot}            { yyless(1); return Make(Tokens.DOT); }

{bad_dot}             { yyerror("bad dot|" + yytext); return (int)Tokens.LEX_ERROR; }
                          
{bad_id}              { yyerror("bad identifier|" + yytext); return (int)Tokens.LEX_ERROR; }
{bad_atoms}           { yyerror("bad boolean|" + yytext); return (int)Tokens.LEX_ERROR; }
{bad_number}           { yyerror("bad number|" + yytext); return (int)Tokens.LEX_ERROR; } 
{bad_char}             { yyerror("bad char|" + yytext); return (int)Tokens.LEX_ERROR; } 
.                      { yyerror("bad input|" + yytext); return (int)Tokens.LEX_ERROR; } 




%{
                      LoadYylval();
%}

%%