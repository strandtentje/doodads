using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

[Category("System & IO")]
[Title("Match text against pattern-matching dir names")]
[Description("""
             Instead of the more common situation of finding a dir using a regex or some other pattern,
             dir names are a pattern, and a regular bit of text is used to match one or multiple dirs.
             The syntax for dir names is as follows:

             Prefix with # to enable pattern matching against dir

             Literals
             #`123` use backticks to match directories numerically
             #'abc' use single quotes to match directories alphabetically (case insensitive)
             #'abc'% suffix with % to match directories case sensitive

             Text Modifiers (works only for text)
             #$'abc' prefix with dollar sign to test if string starts with something
             #;'abc' prefix with semicol to test if string ends with something
             #ß'abc' prefix with altgr+s to test if string contains something 

             Ordinality Modifiers (works for text and numbers)
             #~`123` prefix with wiggly to match anything that isnt this
             #-'cde' prefix with minus to match anything that alphanumerically comes before (check ascii)
             #+'abc' prefix with plus to match anything that matches or comes after

             Range Expressions (extension upon ordinality modifiers)
             #(_`123`,`456`) or #(_`abc`,`456`) combines - and + operators to define a range

             Boolean Series Combiners (to test for multiple things)
             #[.-`100`,+`300`] use blocks and . to test if any of those things is true
             #[=ß'lorem',ß'ipsum'] use blocks and = to test if outcomes of all the tests is the same
                    so in this example, its either lorem ipsum, or something completely different, but 
                    not lorem _or_ ipsum
             #[&$'lorem',;'ipsum'] use blocks and & to test if all outcomes are true. so in this example
                    the text must start with lorem and end with ipsum
             #[@$'lorem',;'ipsum'] use blocks and @ for xor (either-or). so in this example, the text 
                    may either start with lorem, or end with ipsum, but not both or neither.
             """)]
public class MatchDir : IteratingService
{
    [NamedSetting("match", "Variable to match with")]
    public readonly UpdatingKeyValue ToMatchSource = new UpdatingKeyValue("match");
    private readonly ConcurrentDictionary<string, IReverseFileExpression> cachedDirs = new();
    protected override bool RunElse { get; } = false;
    [EventOccasion("When there's an explanation for a match or no match")]
    public event CallForInteraction? OnExplanation;
    private IEnumerable<(IReverseFileExpression ex, DirectoryInfo dir)> MapExpressions(IEnumerable<DirectoryInfo> infos)
    {
        foreach (var item in infos)
        {
            if (cachedDirs.TryGetValue(item.Name, out var ex))
            {
                yield return (ex, item);
            }
            else
            {
                var tup = (ex: ContainingBlock.ParseFrom(item.Name, out var errors), dir: item);
                foreach (var err in errors)
                    GlobalLog.Instance?.Warning("RFEx Error: {item}", err);
                cachedDirs[item.Name] = tup.ex;
                yield return tup;
            }
        }
    }
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!constants.NamedItems.TryGetValue("match", out var matchObj) ||
            matchObj is not string matchVariable ||
            string.IsNullOrWhiteSpace(matchVariable))
            throw new Exception("requires match variable");
        if (repeater.Register.ToString() is not string currentPath)
            throw new Exception("parent dir req'd in register");
        else if (!Directory.Exists(currentPath))
            throw new Exception("parent dir didnt exist");
        else if (!repeater.TryFindVariable<string>(matchVariable, out var matchDir) ||
            string.IsNullOrWhiteSpace(matchDir))
            throw new Exception("matchdir required");
        else
        {
            var parentDir = new DirectoryInfo(currentPath);
            var dirs = parentDir.GetDirectories();
            var exps = MapExpressions(dirs);
            foreach (var item in exps)
            {
                var reasons = new List<string>();
                if (item.ex.Evaluate(matchDir, reasons))
                    yield return repeater.AppendRegister(item.dir);
                foreach (var reason in reasons)
                    OnExplanation?.Invoke(this, repeater.AppendMemory(("reason", reason)));
            }
        }
    }
}