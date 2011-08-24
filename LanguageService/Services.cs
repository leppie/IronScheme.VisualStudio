using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using IronScheme.Runtime;
using System.Collections;

namespace IronScheme.VisualStudio
{
  public sealed class SymbolBindingService
  {
    public SymbolBindingService()
    {

    }

    public string GetImportSpec(string spec)
    {
      return string.Format("(apply environment '({0}))", spec);
    }

    public string GetInteractionEnv()
    {
      return RuntimeExtensions.INTERACTION_ENVIRONMENT;
    }

    public SymbolBinding[] GetBindings(string importspec)
    {
      return GetBindingsInternal(GetImportSpec(importspec));
    }

    public SymbolBinding[] GetBindings()
    {
      return GetBindingsInternal(GetImportSpec("(ironscheme)"));
    }


    SymbolBinding[] GetBindingsInternal(string importspec)
    {
      if (string.IsNullOrEmpty(importspec))
      {
        throw new ArgumentException("importspec cannot be null or empty");
      }
      try
      {
        Callable c = string.Format(
@"(lambda (maker)
    (map (lambda (b) 
            (maker (car b) (cdr b)))
         (environment-bindings {0})))", importspec).Eval<Callable>();

        CallTarget2 maker = (n, t) =>
        {
          return new SymbolBinding
          {
            Name = SymbolTable.IdToString((SymbolId)n),
            Type = (BindingType)Enum.Parse(typeof(BindingType), SymbolTable.IdToString((SymbolId)t), true),
          };
        };

        var r = c.Call(Closure.CreateStatic(maker)) as IEnumerable;

        List<SymbolBinding> sbs = new List<SymbolBinding>();

        foreach (SymbolBinding sb in r)
        {
          sbs.Add(sb);
        }

        return sbs.ToArray();
      }
      catch (Exception ex)
      {
        throw new EvaluationException(ex.ToString());
      }
    }


    ProcedureInfo GetProcedureInfoInternal(string proc, string importspec)
    {
      if (string.IsNullOrEmpty(proc))
      {
        throw new ArgumentException("proc cannot be null or empty");
      }
      if (string.IsNullOrEmpty(importspec))
      {
        throw new ArgumentException("importspec cannot be null or empty");
      }

      try
      {
        Callable c = string.Format(
@"(lambda (maker)
    (let ((p (eval '{0} {1})))
      (let ((forms (call-with-values (lambda () (procedure-form p)) vector)))
        (maker (symbol->string '{0}) forms))))", proc, importspec).Eval<Callable>();

        CallTarget2 maker = (n, forms) =>
        {
          var f = forms as object[];
          List<string> ff = new List<string>();
          foreach (Cons fc in f)
          {
            ff.Add(fc.PrettyPrint.TrimEnd('\n'));
          }

          var pi = new ProcedureInfo
          {
            Name = n as string,
            Forms = ff.ToArray()
          };
          return pi;
        };

        return c.Call(Closure.CreateStatic(maker)) as ProcedureInfo;
      }
      catch (Exception ex)
      {
        throw new EvaluationException(ex.ToString());
      }
    }

    public ProcedureInfo GetProcedureInfo(string proc)
    {
      return GetProcedureInfoInternal(proc, GetInteractionEnv());
    }

    public ProcedureInfo GetProcedureInfo(string proc, string importspec)
    {
      return GetProcedureInfoInternal(proc, GetImportSpec(importspec));
    }
  }

  [Serializable]
  public sealed class EvaluationException : Exception
  {
    public EvaluationException(string msg)
      : base(msg)
    {

    }
  }


  [Serializable]
  public class ProcedureInfo
  {
    public string Name { get; set; }
    public string[] Forms { get; set; }
  }

  public enum BindingType
  {
    Procedure,
    Syntax,
    Record
  }

  [Serializable]
  public class SymbolBinding
  {
    public string Name { get; set; }
    public BindingType Type { get; set; }
  }
}
