%using Microsoft.VisualStudio.TextManager.Interop
%using System.Linq
%namespace Babel.Parser
%valuetype LexValue
%partial




%{

ErrorHandler handler = null;
public void SetHandler(ErrorHandler hdlr) { handler = hdlr; }
internal void CallHdlr(string msg, LexLocation val)
{
    handler.AddError(msg, val.sLin, val.sCol, val.eCol - val.sCol);
}
internal TextSpan MkTSpan(LexLocation s) { return TextSpan(s.sLin, s.sCol, s.eLin, s.eCol); }

internal void Match(LexLocation lh, LexLocation rh) 
{
    DefineMatch(MkTSpan(lh), MkTSpan(rh)); 
}

public object result = null;
internal List<Identifier> ids = new List<Identifier>();
Cons _imports = null;
internal string imports 
{ 
  get 
  { 
    return string.Join(" ", Enum(_imports).Select(x => x.ToString()).ToArray()); 
  } 
}

static IEnumerable<object> Enum(Cons c)
{
  while (c != null)
  {
    yield return c.car;
    c = c.cdr as Cons;
  }
}


static readonly object Ignore = new object();

static Cons Last(Cons c)
{
  while (c.cdr != null)
  {
    c = c.cdr as Cons;
  }
  return c;
}

static Cons Append(Cons c, Cons t)
{
  if (c == null || c.car == Ignore)
  {
    return t;
  }
  if (t == null || t.car == Ignore)
  {
    return c;
  }
  Last(c).cdr = t;
  return c;
}

Identifier Add(Identifier id)
{
  ids.Add(id);
  return id;
}



 
    
%}
    
%union {
    public string str;
    internal object value;
    internal Cons list;
}

%token LBRACE RBRACE LBRACK RBRACK QUOTE QUASIQUOTE UNQUOTE UNQUOTESPLICING VECTORLBRACE DOT BYTEVECTORLBRACE
%token UNSYNTAX SYNTAX UNSYNTAXSPLICING QUASISYNTAX IGNOREDATUM
%token SYMBOL LITERAL STRING NUMBER CHARACTER 
%token LEX_WHITE LEX_COMMENT LEX_ERROR
%token FORM SUBFORM RECORD PROCEDURE LIBRARY IMPORT EXPORT DEFINE DEFINESYNTAX DEFINECONDITIONTYPE DEFINERECORDTYPE DEFINEENUMERATION MODULE

%type<list> exprlist list lists
%type<value> expr
%type<str> definesym symbol

%start file

%%

file 
    : exprlist  EOF                             { result = $1; }
    ;
    
   
list
    : LBRACE exprlist RBRACE                    { Match(@1, @3); $$ = $2; }   
    | LBRACK exprlist RBRACK                    { Match(@1, @3); $$ = $2; }     
    | LBRACE exprlist expr DOT expr RBRACE      { Match(@1, @6); $$ = Append($2, new Cons($3,$5));}     
    | LBRACK exprlist expr DOT expr RBRACK      { Match(@1, @6); $$ = Append($2, new Cons($3,$5));}   
    | LBRACE IMPORT lists RBRACE                { Match(@1, @4); _imports = Append(_imports, $3);}   
    | specexpr expr 
    ;
    
lists
    :                                          { $$ = null; }
    | lists list                               { $$ = Append($1, new Cons($2)); }
    ;

exprlist
    :                                           { $$ = null; }             
    | exprlist expr                             { $$ = Append($1, new Cons($2)); }
    ;
    
symbol
    : SYMBOL                                    
    | FORM                                       
    | PROCEDURE                                  
    | EXPORT
    | LIBRARY
    | MODULE
    | SUBFORM
    | RECORD
    ;
    
definesym
    : DEFINE
    | DEFINESYNTAX
    | DEFINECONDITIONTYPE
    | DEFINEENUMERATION
    | DEFINERECORDTYPE
    ;        

    
expr
    : list                                        { $$ = $1;}
    | symbol                                      { $$ = Add(new Identifier($1) {Location = MkTSpan(@1)}); }
    | STRING                                      
    | definesym                                   { $$ = new DefineLocation($1) {Location = MkTSpan(@1)}; Add(new Identifier($1) {Location = MkTSpan(@1)}); }
    | NUMBER                                      { $$ = new Number($1.str); }
    | LITERAL                                     
    | CHARACTER                                   
    | VECTORLBRACE exprlist RBRACE                { Match(@1, @3); } 
    | BYTEVECTORLBRACE exprlist RBRACE            { Match(@1, @3); } 
    | IGNOREDATUM expr
    ; 

specexpr
    : QUOTE                                      
    | UNQUOTESPLICING                            
    | QUASIQUOTE                                 
    | UNQUOTE                                    
    | SYNTAX                                     
    | UNSYNTAXSPLICING                           
    | QUASISYNTAX                                
    | UNSYNTAX                                   
    
    ;    

%%



