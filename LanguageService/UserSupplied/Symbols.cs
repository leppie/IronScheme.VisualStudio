using System;
using System.Collections.Generic;
using System.Text;
using Babel.Parser;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Linq;
using IronScheme;
using IronScheme.VisualStudio;

namespace Babel
{

  abstract class CodeElement
  {
    public string Name;
    public TextSpan Location;

    static int GetGlyph(string type)
    {
      switch (type)
      {
        case "define-syntax":
          return 60;
        case "define-record-type":
          return 0;
        case "define-enumeration":
          return 18;
        case "define-condition-type":
          return 107;
        case "library":
          return 90;
        case "module":
          return 54;
        default:
          return 72;
      }
    }

    public static IEnumerable<CodeElement> GetDefs(object defs)
    {
      Cons body = defs as Cons;

      while (body != null)
      {
        Cons c = body.car as Cons;
        if (c != null)
        {
          if (c.car is DefineLocation)
          {
            string defname = c.car.ToString();
            if (defname != null && (defname.StartsWith("define")))
            {
              object name = ((Cons)c.cdr).car;
              object defbody = ((Cons)c.cdr).cdr;
              string sname = null;
              if (name is Cons)
              {
                Identifier id = ((Cons)name).car as Identifier;
                sname = id.Name;
                Definition d = new Definition(sname, defname == "define" ? defbody : null);
                d.Location = id.Location;
                d.Type = GetGlyph(defname);
                yield return d;
              }
              else
              {
                Identifier id = name as Identifier;
                if (id != null)
                {
                  sname = id.Name;
                  Definition d = new Definition(sname, defname == "define" ? defbody : null);
                  d.Location = id.Location;
                  d.Type = GetGlyph(defname);
                  yield return d;
                }
              }
            }
          }
        }

        if (body.car is Library || body.car is Module)
        {
          yield return body.car as CodeElement;
        }

        body = body.cdr as Cons;
      }
      yield break;
    }

    public static string GetName(object list)
    {
      List<string> ll = new List<string>();
      Cons c = list as Cons;
      while (c != null)
      {
        ll.Add(c.car.ToString());
        c = c.cdr as Cons;
      }
      return string.Join(" ", ll.ToArray());
    }

    public override string ToString()
    {
      return Name;
    }

  }

  class Cons : CodeElement
  {
    public object car;
    public object cdr;

    public Cons(object car)
    {
      this.car = car;
    }

    public Cons(object car, object cdr)
    {
      this.car = car;
      this.cdr = cdr;
    }

    static string WriteFormat(object o)
    {
      if (o == null)
      {
        return "()";
      }
      else return o.ToString();
    }

    public override string ToString()
    {
      List<string> v = new List<string>();
      Cons s = this;

      while (s != null)
      {
        v.Add(WriteFormat(s.car));
        if (s.cdr != null && !(s.cdr is Cons))
        {
          v.Add(".");
          v.Add(WriteFormat(s.cdr));
          break;
        }
        s = s.cdr as Cons;
      }
      return string.Format("({0})", string.Join(" ", v.ToArray()));
    }
  }


  abstract class CodeContainerElement : CodeElement
  {
    public readonly List<CodeElement> Items = new List<CodeElement>();

    public void Add(CodeElement e)
    {
      Items.Add(e);
    }
  }

  class Library : CodeContainerElement
  {

    public Library(object name, object body)
    {
      Name = GetName(name);
      foreach (CodeElement d in GetDefs(body))
      {
        Add(d);
      }
    }

  }

  class Module : CodeContainerElement
  {

    public Module(object name, object body)
    {
      Name = GetName(name);
      foreach (CodeElement d in GetDefs(body))
      {
        Add(d);
      }
    }

  }

  class TopLevel : CodeContainerElement
  {
    public TopLevel(object body)
    {
      Name = "toplevel";
      foreach (CodeElement d in GetDefs(body))
      {
        Add(d);
      }
    }

  }

  class Definition : CodeElement
  {
    public Definition(string name, object body)
    {
      Name = name;
      /*    foreach (ICodeElement d in GetDefs(body))
          {
            Add(d);
          }*/
    }

    public int Type = 72;
  }

  class DefineLocation : CodeElement
  {
    public DefineLocation(string name)
    {
      Name = name;
    }
  }

  class Identifier : CodeElement
  {
    public Identifier(string name)
    {
      Name = name;
    }
  }

  class Number : CodeElement
  {
    string Value;
    public Number(string value)
    {
      Value = value;
    }

    public override string ToString()
    {
      return Value;
    }
  }

#if UNUSED
  static class SymbolCache
  {
    static Dictionary<string, Dictionary<string, int>> cache = new Dictionary<string, Dictionary<string, int>>();

    static Dictionary<string, int> symbols = new Dictionary<string,int>();

    static SymbolCache ()
	  {
      symbols.Add("define", (int)Tokens.DEFINE);
      symbols.Add("define-syntax", (int)Tokens.DEFINESYNTAX);
      symbols.Add("define-condition-type", (int)Tokens.DEFINECONDITIONTYPE);
      symbols.Add("define-enumeration", (int)Tokens.DEFINEENUMERATION);
      symbols.Add("define-record-type", (int)Tokens.DEFINERECORDTYPE);
      symbols.Add("import", (int)Tokens.IMPORT);
      symbols.Add("library", (int)Tokens.LIBRARY);
      symbols.Add("export", (int)Tokens.EXPORT);
      symbols.Add("module", (int)Tokens.MODULE);
	  }

    static IEnumerable<object> Enum(Cons c)
    {
      while (c != null)
      {
        yield return c.car;
        c = c.cdr as Cons;
      }
    }

    public static int Lookup(string s, string imports)
    {
      var simp = imports;
      
      int i;
      
      if (symbols.TryGetValue(s, out i))
      {
        return i;
      }

      if (string.IsNullOrEmpty(simp))
      {
        return (int)Tokens.SYMBOL;
      }

      Dictionary<string, int> values;
      if (!cache.TryGetValue(simp, out values))
      {
        values = new Dictionary<string, int>();

        var sbs = new SymbolBindingService();

        foreach (var b in sbs.GetBindings(simp))
        {
          switch (b.Type)
          {
            case BindingType.Procedure:
              values.Add(b.Name, (int)Tokens.PROCEDURE);
              break;
            case BindingType.Record:
              values.Add(b.Name, (int)Tokens.RECORD);
              break;
            case BindingType.Syntax:
              values.Add(b.Name, (int)Tokens.FORM);
              break;
          }
        }

        cache[simp] = values;
      }

      if (values.TryGetValue(s, out i))
      {
        return i;
      }

      return (int)Tokens.SYMBOL;
    }
  }

#endif

  static class Symbols
  {
    static Dictionary<string, int> symbols = new Dictionary<string, int>();

    #region Lotsa text
    static string[] forms = 
    {
      "lambda", "set!", "quote", "if", "cond", "case", "do", "unless", "when", "let", "let*", "letrec", "letrec*", "assert", "syntax-case", "syntax-rules", "eol-style", "error-handling-mode", "buffer-mode", "file-options", "time", "include", "include-into", "parameterize",
      "case-lambda", "begin", "or", "and", "letrec-syntax", "let-syntax", "unquote", "quasiquote", "unquote-splicing", "let-values", "syntax", "delay",
      "unsyntax", "unsyntax-splicing", "quasisyntax", "with-syntax", "identifier-syntax", "endianness", "guard", "record-constructor-descriptor", "record-type-descriptor", "let*-values",
      "clr-static-event-add!", "clr-static-event-remove!", "clr-event-add!", "clr-event-remove!", "clr-clear-usings", "clr-using", "clr-reference", "clr-is", "clr-foreach", "clr-cast", "clr-call", "clr-static-call", "clr-field-get", "clr-field-set!", "clr-static-field-get", "clr-static-field-set!", "clr-prop-get", "clr-prop-set!", "clr-static-prop-get", "clr-static-prop-set!", "clr-indexer-get", "clr-indexer-set!", "clr-new", "clr-new-array",
      "trace-lambda","trace-define","trace-define-syntax","trace-let-syntax","trace-letrec-syntax","fluid-let-syntax"
    };

    static string[] subform = 
    {
      "rename", "except", "only", "else", "=>", "mutable", "immutable", "fields", "nongenerative", "parent", "protocol", "sealed", "opaque", "parent-rtd", "...", "_", "prefix"
    };

    static string[] records = 
    {
      "&condition","&message", "&warning", "&serious","&error","&violation","&assertion","&irritants","&who","&implementation-restriction","&lexical","&syntax","&undefined",
      "&non-continuable","&i/o","&i/o-read","&i/o-write","&i/o-invalid-position","&i/o-filename","&i/o-file-protection","&i/o-file-is-read-only","&i/o-file-already-exists",
      "&i/o-file-does-not-exist","&i/o-port","&i/o-decoding","&i/o-encoding","&no-infinities","&no-nans"
    };

    static string[] procs = 
    {
"car", "cdr", "eq?", "eqv?", "equal?", "not", "null?", "pair?", "cons", "map", "append", "list", "vector", "list?", "vector?", "vector-ref", "apply", "error", "cons*", "call-with-values", "values"
,"call/cc", "call-wit-current-continuation", "memv", "assv", "memq", "assq", "assoc", "member", "void", "boolean?", "number?", "char?", "cadr", "cddr", "newline", "display", "read", "write", "make-parameter", "time-it"
,"+", "-", "*", "/", "length", "vector-length", "=", "vector-set!", "<=", "<", ">", ">=", "integer->char", "char->integer", "for-each", "char<=?", "string->list", "symbol->string", "procedure?"
,"string-append", "string?", "with-input-from-file", "with-output-from-file", "dynamic-wind", "list->vector", "make-vector", "vector->list", "string->symbol", "gensym", "symbol?", "environment?"
,"call-with-current-continuation", "string-ci>?", "string-ci>=?", "string-ci<?", "string-ci<=?", "char-whitespace?", "char-upper-case?", "char-lower-case?", "char-title-case?", "char-fold-case?"
,"char-upcase", "char-downcase", "string-upcase", "string-titlecase", "string-normalize-nfkd", "string-normalize-nfkc", "string-normalize-nfd", "string-normalize-nfc", "string-foldcase", "string-downcase", "string-ci=?", "char-numeric?", "char-general-category", "char-titlecase", "char-foldcase", "char-ci>?", "char-ci>=?", "char-ci=?", "char-ci<?", "char-ci<=?", "char-alphabetic?", "make-variable-transformer", "identifier?", "generate-temporaries", "free-identifier=?", "syntax->datum", "datum->syntax", "bound-identifier=?", "record-type-descriptor?", "record-predicate", "record-mutator", "record-constructor", "record-accessor", "make-record-type-descriptor", "make-record-constructor-descriptor", "record?", "record-type-uid", "record-type-sealed?", "record-type-parent", "record-type-opaque?", "record-type-name", "record-type-generative?", "record-type-field-names", "record-rtd", "record-field-mutable?", "delete-file", "file-exists?", "vector-sort!", "vector-sort", "list-sort", "symbol-hash", "string-ci-hash", "string-hash", "equal-hash", "hashtable-equivalence-function", "make-hashtable", "hashtable-hash-function", "make-eqv-hashtable", "make-eq-hashtable", "hashtable?", "hashtable-update!", "hashtable-size", "hashtable-set!", "hashtable-ref", "hashtable-mutable?", "hashtable-keys", "hashtable-entries", "hashtable-delete!", "hashtable-copy", "hashtable-contains?", "hashtable-clear!", "write-char", "with-output-to-file", "read-char", "peek-char", "open-output-file", "open-input-file", "close-output-port", "close-input-port", "eof-object?", "eof-object", "current-error-port", "current-output-port", "current-input-port", "output-port?", "input-port?", "utf-8-codec", "utf-16-codec", "transcoder-error-handling-mode", "transcoder-eol-style", "transcoder-codec", "transcoded-port", "textual-port?", "string->bytevector", "standard-output-port", "standard-input-port", "standard-error-port", "set-port-position!", "put-u8", "put-string", "put-datum", "put-char", "put-bytevector", "port?", "port-transcoder", "port-position", "port-has-set-port-position!?", "port-has-port-position?", "port-eof?", "output-port-buffer-mode", "open-string-output-port", "open-string-input-port", "open-file-output-port", "open-file-input/output-port", "open-file-input-port", "open-bytevector-output-port", "open-bytevector-input-port", "native-transcoder", "native-eol-style", "make-transcoder", "latin-1-codec", "make-i/o-write-error", "make-i/o-read-error", "make-i/o-port-error", "make-i/o-invalid-position-error", "make-i/o-filename-error", "make-i/o-file-protection-error", "make-i/o-file-is-read-only-error", "make-i/o-file-does-not-exist-error", "make-i/o-file-already-exists-error", "make-i/o-error", "make-i/o-encoding-error", "make-i/o-decoding-error", "make-custom-textual-output-port", "make-custom-textual-input/output-port", "make-custom-textual-input-port", "make-custom-binary-output-port", "make-custom-binary-input/output-port", "make-custom-binary-input-port", "make-bytevector", "lookahead-u8", "lookahead-char", "i/o-write-error?", "i/o-read-error?", "i/o-port-error?", "i/o-invalid-position-error?", "i/o-filename-error?", "i/o-file-protection-error?", "i/o-file-is-read-only-error?", "i/o-file-does-not-exist-error?", "i/o-file-already-exists-error?", "i/o-error?", "i/o-error-port", "i/o-error-filename", "i/o-encoding-error?", "i/o-encoding-error-char", "i/o-decoding-error?", "get-u8", "get-string-n!", "get-string-n", "get-string-all", "get-line", "get-datum", "get-char", "get-bytevector-some", "get-bytevector-n!", "get-bytevector-n", "get-bytevector-all", "flush-output-port", "close-port", "exit", "command-line", "remove", "remv", "remp", "remq", "partition"
,"call-with-input-file", "call-with-output-file", "memp", "exists", "for-all", "fold-right", "fold-left", "find", "filter", "assp", "call-with-string-output-port", "call-with-port", "call-with-bytevector-output-port", "bytevector->string", "buffer-mode?", "binary-port?", "with-exception-handler", "raise-continuable", "raise", "make-enumeration", "enum-set=?", "enum-set-universe", "enum-set-union", "enum-set-subset?", "enum-set-projection", "enum-set-member?", "enum-set-intersection", "enum-set-indexer", "enum-set-difference", "enum-set-constructor", "enum-set-complement", "enum-set->list", "who-condition?", "warning?", "violation?", "undefined-violation?", "syntax-violation?", "syntax-violation-subform", "syntax-violation-form", "syntax-violation", "simple-conditions", "serious-condition?", "non-continuable-violation?", "message-condition?", "make-who-condition", "make-warning", "make-violation", "make-undefined-violation", "make-syntax-violation", "make-serious-condition", "make-non-continuable-violation", "make-message-condition", "make-lexical-violation", "make-irritants-condition", "make-implementation-restriction-violation", "make-error", "make-assertion-violation", "lexical-violation?", "irritants-condition?", "implementation-restriction-violation?", "error?", "condition-who", "condition-predicate", "condition-message", "condition-irritants", "condition-accessor", "condition", "assertion-violation?", "condition?", "utf32->string", "utf16->string", "utf8->string", "uint-list->bytevector", "u8-list->bytevector", "string->utf8", "string->utf32", "string->utf16", "sint-list->bytevector", "native-endianness", "bytevector?", "bytevector=?", "bytevector-uint-set!", "bytevector-uint-ref", "bytevector-u8-set!", "bytevector-u8-ref", "bytevector-u64-set!", "bytevector-u64-ref", "bytevector-u64-native-set!", "bytevector-u64-native-ref", "bytevector-u32-set!", "bytevector-u32-ref", "bytevector-u32-native-set!", "bytevector-u32-native-ref", "bytevector-u16-set!", "bytevector-u16-ref", "bytevector-u16-native-set!", "bytevector-u16-native-ref", "bytevector-sint-set!", "bytevector-sint-ref", "bytevector-s8-set!", "bytevector-s8-ref", "bytevector-s64-set!", "bytevector-s64-ref", "bytevector-s64-native-set!", "bytevector-s64-native-ref", "bytevector-s32-set!", "bytevector-s32-ref", "bytevector-s32-native-set!", "bytevector-s32-native-ref", "bytevector-s16-set!", "bytevector-s16-ref", "bytevector-s16-native-set!", "bytevector-s16-native-ref", "bytevector-length", "bytevector-ieee-single-set!", "bytevector-ieee-single-ref", "bytevector-ieee-single-native-set!", "bytevector-ieee-single-native-ref", "bytevector-ieee-double-set!", "bytevector-ieee-double-ref", "bytevector-ieee-double-native-set!", "bytevector-ieee-double-native-ref", "bytevector-fill!", "bytevector-copy!", "bytevector-copy", "bytevector->uint-list", "bytevector->u8-list", "bytevector->sint-list", "no-nans-violation?", "no-infinities-violation?", "make-no-nans-violation", "make-no-infinities-violation", "real->flonum", "flzero?", "fltruncate", "fltan", "flsqrt", "flsin", "flround", "flpositive?", "flonum?", "flodd?", "flnumerator", "flnegative?", "flnan?", "flmod0", "flmod", "flmin", "flmax", "fllog", "flinteger?", "flinfinite?", "flfloor", "flfinite?", "flexpt", "flexp", "fleven?", "fldiv0-and-mod0", "fldiv0", "fldiv-and-mod", "fldiv", "fldenominator", "flcos", "flceiling", "flatan", "flasin", "flacos", "flabs", "fl>?", "fl>=?", "fl=?", "fl<?", "fl<=?", "fl/", "fl-", "fl+", "fl*", "fixnum->flonum", "fxzero?", "fxxor", "fxrotate-bit-field", "fxreverse-bit-field", "fxpositive?", "fxodd?", "fxnot", "fxnegative?", "fxmod0", "fxmod", "fxmin", "fxmax"
,"fxlength", "fxior", "fxif", "fxfirst-bit-set", "fxeven?", "fxdiv0-and-mod0", "fxdiv0", "fxdiv-and-mod", "fxdiv", "fxcopy-bit-field", "fxcopy-bit", "fxbit-set?", "fxbit-field", "fxbit-count", "fxarithmetic-shift-right", "fxarithmetic-shift-left", "fxarithmetic-shift", "fxand", "fx>?", "fx>=?", "fx=?", "fx<?", "fx<=?", "fx-/carry", "fx-", "fx+/carry", "fx+", "fx*/carry", "fx*", "greatest-fixnum", "least-fixnum", "fixnum-width", "fixnum?", "bitwise-rotate-bit-field", "bitwise-reverse-bit-field", "bitwise-length", "bitwise-if", "bitwise-first-bit-set", "bitwise-copy-bit-field", "bitwise-copy-bit", "bitwise-bit-set?", "bitwise-bit-field", "bitwise-bit-count", "bitwise-xor", "bitwise-ior", "bitwise-and", "bitwise-not", "bitwise-arithmetic-shift-right", "bitwise-arithmetic-shift-left", "bitwise-arithmetic-shift", "zero?", "vector-map", "vector-for-each", "vector-fill!", "truncate", "tan", "symbol=?", "substring", "string>?", "string>=?", "string=?", "string<?", "string<=?", "string-ref", "string-length", "string-for-each", "string-copy", "string->number", "string", "sqrt", "sin", "round", "reverse", "real?", "real-valued?", "real-part", "rationalize", "rational?", "rational-valued?", "positive?", "odd?", "numerator", "number->string", "negative?", "nan?", "min", "max", "make-string", "make-rectangular", "make-polar", "magnitude", "log"
,"list-tail", "list-ref", "list->string", "lcm", "integer?", "integer-valued?", "infinite?", "inexact?", "inexact", "imag-part", "gcd", "floor", "finite?", "expt", "exp", "exact?", "exact-integer-sqrt", "exact", "even?", "div0-and-mod0", "mod0", "div0", "div-and-mod", "mod", "div", "denominator", "cos", "complex?", "char>?", "char>=?", "char=?", "char<?", "ceiling", "cadaar", "cddddr", "cdddar", "cddadr", "cddaar", "cdaddr", "cdadar", "cdaadr", "cdaaar", "cadddr", "caddar", "cadadr", "cddaar", "caaddr", "caadar", "caaadr", "caaaar", "cdddr", "cddar", "cdadr", "cdaar", "caddr", "cadar", "caadr", "caaar", "cdar", "caar", "boolean=?", "atan", "assertion-violation", "asin", "angle", "acos", "abs", "scheme-report-environment", "quotient", "null-environment", "remainder", "modulo", "inexact->exact", "force", "exact->inexact", "eval"
,"environment", "set-cdr!", "string-set!", "exit", "set-car!", "string-fill!", "command-line", "make-variable-transformer", "identifier?", "generate-temporaries", "free-identifier=?", "syntax->datum", "datum->syntax", "bound-identifier=?", "syntax-violation", "delete-file", "file-exists?", "make-i/o-write-error", "make-i/o-read-error", "make-i/o-port-error", "make-i/o-invalid-position-error", "make-i/o-filename-error", "make-i/o-file-protection-error", "make-i/o-file-is-read-only-error", "make-i/o-file-does-not-exist-error", "make-i/o-file-already-exists-error", "make-i/o-error", "i/o-write-error?", "i/o-read-error?", "i/o-port-error?", "i/o-invalid-position-error?", "i/o-filename-error?", "i/o-file-protection-error?", "i/o-file-is-read-only-error?", "i/o-file-does-not-exist-error?", "i/o-file-already-exists-error?", "i/o-error?", "i/o-error-port", "i/o-error-filename", "vector-sort!", "vector-sort", "list-sort"
,"pretty-print","trace-printer","string-format","string-compare","string-ci-compare","vector-binary-search","vector-copy","vector-index-of",      
"vector-contains?","vector-reverse!","vector-filter","vector-append","get-clr-type","clr-type?","gc-collect","procedure-arity","procedure-name", 
"procedure-environment","procedure-form","format","fprintf","printf","load/args","enum-set?","serialize-port","deserialize-port",
"library-path","optimization-level","interaction-environment-symbols","environment-symbols","environment-bindings", "i/o-error-position"
    };

    #endregion

    static Symbols()
    {
      symbols.Add("define", (int)Tokens.DEFINE);
      symbols.Add("define-syntax", (int)Tokens.DEFINESYNTAX);
      symbols.Add("define-condition-type", (int)Tokens.DEFINECONDITIONTYPE);
      symbols.Add("define-enumeration", (int)Tokens.DEFINEENUMERATION);
      symbols.Add("define-record-type", (int)Tokens.DEFINERECORDTYPE);
      symbols.Add("import", (int)Tokens.IMPORT);
      symbols.Add("library", (int)Tokens.LIBRARY);
      symbols.Add("export", (int)Tokens.EXPORT);
      symbols.Add("module", (int)Tokens.MODULE);

      Array.ForEach(forms, AddForm);
      Array.ForEach(subform, AddSubForm);
      Array.ForEach(records, AddRecord);
      Array.ForEach(procs, AddProcedure);
    }

    static void AddProcedure(string s)
    {
      symbols[s] = (int)Tokens.PROCEDURE;
    }

    static void AddRecord(string s)
    {
      symbols.Add(s, (int)Tokens.RECORD);
    }

    static void AddSubForm(string s)
    {
      symbols.Add(s, (int)Tokens.SUBFORM);
    }

    static void AddForm(string s)
    {
      symbols.Add(s, (int)Tokens.FORM);
    }

    public static IEnumerable<string> GetProcedures()
    {
      return procs.Distinct();
    }
    
    public static int Get(string s)
    {
      int r;
      if (symbols.TryGetValue(s, out r))
      {
        return r;
      }
      else
      {
        return (int)Tokens.SYMBOL;
      }
    }

    public static IEnumerable<string> GetForms()
    {
      yield return "define";
      yield return "define-syntax";
      yield return "define-condition-type";
      yield return "define-enumeration";
      yield return "define-record-type";
      yield return "import";
      yield return "library";
      yield return "export";
      yield return "module";

      foreach (var i in forms.Distinct())
      {
        yield return i;
      }
    }

    internal static IEnumerable<string> GetSubForms()
    {
      return subform.Distinct();
    }

    internal static IEnumerable<string> GetRecords()
    {
      return records.Distinct();
    }


  }
}
