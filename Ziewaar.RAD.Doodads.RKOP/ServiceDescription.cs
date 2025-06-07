using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Ziewaar.RAD.Doodads.RKOP.Exceptions;
namespace Ziewaar.RAD.Doodads.RKOP;
#nullable enable
public class ServiceDescription<TResultSink> : IParityParser where TResultSink : class, IInstanceWrapper, new()
{
    public string? ServiceTypeName;
    public ServiceConstantsDescription ConstantsDescription = new ServiceConstantsDescription();
    public List<ServiceDescription<TResultSink>> Children = new List<ServiceDescription<TResultSink>>();
    public ServiceDescription<TResultSink>? RedirectsTo;
    public ServiceDescription<TResultSink>? Concatenation;
    public ServiceDescription<TResultSink>? SingleBranch;
    public CursorText TextScope = CursorText.Empty;
    public TResultSink? ResultSink { get; private set; }
    public ParityParsingState UpdateFrom(ref CursorText text) => this.UpdateFrom(ref text, new NonChaining<TResultSink>());
    public ParityParsingState UpdateFrom(ref CursorText text, ChainingPayload<TResultSink> chain)
    {
        var state = HandleEOF(ref text);
        if (state == ParityParsingState.Void)
            return state;

        if (chain is NonChaining<TResultSink>)
            state = ParseBranchKey(ref text);
        else if (chain is ChainingPayload<TResultSink> source)
            state = DeduceBranchKey(source);
        else
            throw new ArgumentException("Provide a chaining origin, or NonChaining if none.", nameof(chain));

        if (state == ParityParsingState.Void)
            return state;

        if (ResultSink == null)
        {
            state = ParityParsingState.New;
            ResultSink = new();
        }

        text = text.
            SkipWhile(char.IsWhiteSpace).
            ValidateToken(TokenDescription.Identifier, out var typeIdentifier);

        if (typeIdentifier.Text.StartsWith("_"))
        {
            state |= ParseReference(ref text, typeIdentifier);            
        }
        else
        {
            state |=
                ParseTypeAndConstants(ref text, typeIdentifier) |
                ParseChildrenBlock(ref text);

            LinkToScope(text);
            state |= ParseClosingToken(ref text, chain);
        }

        if (state > ParityParsingState.Unchanged)
            HandleDataChanges();

        return state;
    }
    private ParityParsingState HandleEOF(ref CursorText text)
    {
        text = text.SkipWhile(char.IsWhiteSpace);
        if (text.Position == text.Text.Length)
        {
            LinkToScope(CursorText.Empty);
            return VoidAll();
        }
        else
        {
            return ParityParsingState.Unchanged;
        }
    }
    private ParityParsingState ParseBranchKey(ref CursorText text)
    {
        text = text.TakeToken(TokenDescription.Identifier, out var referenceIdentifier);
        if (!referenceIdentifier.IsValid)
            return VoidAll();

        text = text.
            SkipWhile(char.IsWhiteSpace).
            ValidateToken(TokenDescription.BranchAnnouncement, out var assignment);

        if (string.IsNullOrWhiteSpace(this.ConstantsDescription.BranchKey))
        {
            this.ConstantsDescription.BranchKey = referenceIdentifier.Text;
            return ParityParsingState.New;
        }
        else if (this.ConstantsDescription.BranchKey != referenceIdentifier.Text)
        {
            this.ConstantsDescription.BranchKey = referenceIdentifier.Text;
            return ParityParsingState.Changed;
        }
        else
        {
            return ParityParsingState.Unchanged;
        }
    }
    private ParityParsingState DeduceBranchKey(ChainingPayload<TResultSink> source)
    {
        var preceedingIdentifier = source.NewKey;
        if (string.IsNullOrWhiteSpace(this.ConstantsDescription.BranchKey))
        {
            this.ConstantsDescription.BranchKey = preceedingIdentifier;
            return ParityParsingState.New;
        }
        else if (this.ConstantsDescription.BranchKey != preceedingIdentifier)
        {
            this.ConstantsDescription.BranchKey = preceedingIdentifier;
            return ParityParsingState.Changed;
        }
        else
        {
            return ParityParsingState.Unchanged;
        }
    }
    private ParityParsingState ParseReference(ref CursorText text, Token typeIdentifier)
    {
        VoidAll();
        ServiceDescription<TResultSink> newRedirection = text[typeIdentifier.Text] as ServiceDescription<TResultSink> ??
            throw new ReferenceException($"Referring to unknown existing service {typeIdentifier.Text}");
        text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.Terminator, out var _);
        if (this.RedirectsTo == null)
        {
            this.RedirectsTo = newRedirection;
            return ParityParsingState.New;
        }
        else if (this.RedirectsTo == newRedirection)
        {
            this.RedirectsTo = newRedirection;
            return ParityParsingState.Changed;
        }
        else
        {
            return ParityParsingState.Unchanged;
        }
    }
    private ParityParsingState ParseTypeAndConstants(ref CursorText text, Token typeIdentifier)
    {
        this.RedirectsTo = null;
        text = text.
            SkipWhile(char.IsWhiteSpace).
            ValidateToken(TokenDescription.StartOfArguments, out var _);

        var state = ConstantsDescription.UpdateFrom(ref text);

        text = text.
            SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.EndOfArguments, out var _);

        if (string.IsNullOrWhiteSpace(this.ServiceTypeName))
        {
            this.ServiceTypeName = typeIdentifier.Text;
            return state | ParityParsingState.New;
        }
        else if (this.ServiceTypeName != typeIdentifier.Text)
        {
            this.ServiceTypeName = typeIdentifier.Text;
            return state | ParityParsingState.Changed;
        }
        else
        {
            return state;
        }
    }
    private ParityParsingState ParseChildrenBlock(ref CursorText text)
    {
        text = text.
            SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.BlockOpen, out var openBlock);

        if (openBlock.IsValid)
        {
            ParityParsingState state = ParityParsingState.Unchanged;
            text = text.EnterScope();
            int childCounter = 0;
            ParityParsingState lastState = ParityParsingState.Void;
            do
            {
                if (Children.Count == childCounter)
                    Children.Add(new ServiceDescription<TResultSink>());
                lastState = Children[childCounter].UpdateFrom(ref text);
                state |= lastState;
                if (lastState > ParityParsingState.Void)
                    childCounter++;
            } while (lastState != ParityParsingState.Void);

            while (childCounter < Children.Count)
            {
                Children[childCounter].VoidAll();
                Children.RemoveAt(childCounter);
                state |= ParityParsingState.Changed;
            }

            text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.BlockClose, out var _);
            text = text.ExitScope();


            return state;
        }
        else if (Children.Count > 0)
        {
            this.VoidChildren();
            return ParityParsingState.Changed;
        }
        else
        {
            return ParityParsingState.Unchanged;
        }
    }
    private ParityParsingState ParseClosingToken(ref CursorText text, ChainingPayload<TResultSink> chain)
    {
        text = text.SkipWhile(char.IsWhiteSpace);
        var candidateNextCursor = text.TakeToken(TokenDescription.Chainer, out var chainingToken);

        switch (chainingToken.Text[0])
        {
            case '&':
                if (chain is ContinueChain<TResultSink>)
                {
                    if (Concatenation != null)
                    {
                        Concatenation?.VoidAll();
                        Concatenation = null;
                        return ParityParsingState.Changed;
                    } else
                    {
                        return ParityParsingState.Unchanged;
                    }
                }
                else
                {
                    text = candidateNextCursor;
                    Concatenation = Concatenation ?? new ServiceDescription<TResultSink>();
                    return Concatenation.UpdateFrom(ref text, new ConcatChain<TResultSink>(this));
                }
            case ':':
                var stateAfterContinue = SingleBranch == null ? ParityParsingState.Changed : ParityParsingState.Unchanged;
                SingleBranch = SingleBranch ?? new ServiceDescription<TResultSink>();
                text = candidateNextCursor;
                stateAfterContinue |= SingleBranch.UpdateFrom(ref text, new ContinueChain<TResultSink>(this));

                text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.Ampersand, out var amperToken);
                if (amperToken.IsValid)
                {
                    stateAfterContinue |= Concatenation == null ? ParityParsingState.Changed : ParityParsingState.Unchanged;
                    Concatenation = new ServiceDescription<TResultSink>();
                    stateAfterContinue |= Concatenation.UpdateFrom(ref text, new ConcatChain<TResultSink>(this));
                } else
                {
                    stateAfterContinue |= Concatenation != null ? ParityParsingState.Changed : ParityParsingState.Unchanged;
                    Concatenation?.VoidAll();
                    Concatenation = null;
                }
                return stateAfterContinue;
            case ';':
                var stateAfterCloser =
                    (Concatenation ?? SingleBranch) != null ?
                    ParityParsingState.Changed : ParityParsingState.Unchanged;
                Concatenation?.VoidAll();
                Concatenation = null;
                SingleBranch?.VoidAll();
                SingleBranch = null;
                text = candidateNextCursor;
                return stateAfterCloser;
            default:
                throw new ParsingException("Expected &, : or ;");
        }
    }
    private void LinkToScope(CursorText text)
    {
        if (this.TextScope?.LocalScope is SortedList<string, object> existingScope && 
            existingScope.IndexOfValue(this) is int currentScopeIndex && 
            currentScopeIndex > -1)
        {
            existingScope.RemoveAt(currentScopeIndex);
        }
        if (ConstantsDescription?.BranchKey is string newBranchName)
            text[newBranchName] = this;
        this.TextScope = text;
    }
    public ParityParsingState VoidAll()
    {
        VoidChildren();
        Children.Clear();
        Concatenation?.UpdateFrom(ref CursorText.Empty);
        SingleBranch?.UpdateFrom(ref CursorText.Empty);
        RedirectsTo = null;
        Concatenation = null;
        SingleBranch = null;
        ServiceTypeName = null;
        HandleDataChanges();

        return ParityParsingState.Void;
    }
    private void VoidChildren()
    {
        foreach (var item in Children)
            item.UpdateFrom(ref CursorText.Empty);
    }
    public void HandleDataChanges()
    {
        if (RedirectsTo != null)
        {
            ResultSink ??= new TResultSink();
            ResultSink.SetReference(this.RedirectsTo);
        } else if (this.ServiceTypeName != null)
        {
            TResultSink? concatenationWrapper = null, singleWrapper = null;
            if (Concatenation != null && Concatenation.ResultSink != null)
                concatenationWrapper = Concatenation.ResultSink;
            if (SingleBranch != null && SingleBranch.ResultSink != null)
                singleWrapper = SingleBranch.ResultSink;
            ResultSink ??= new TResultSink();
            ResultSink.SetDefinition(
                TextScope,
                this.ServiceTypeName,
                concatenationWrapper,
                singleWrapper,
                this.ConstantsDescription.ToSortedList(),
                this.Children.ToSortedList());
        } else
        {
            ResultSink?.Cleanup();
        }
    }
    public void WriteTo(StreamWriter writer, bool skipBranchName = false, int indentLevel = 0, string indentString = "    ")
    {
        if (!skipBranchName)
        {
            for (int i = 0; i < indentLevel; i++)
                writer.Write(indentString);
            writer.Write(ConstantsDescription.BranchKey);
            writer.Write("->");
        }
        if (RedirectsTo is ServiceDescription<TResultSink> redir)
        {
            writer.Write(redir.ConstantsDescription.BranchKey);
        }
        else
        {
            writer.Write(ServiceTypeName);
            writer.Write("(");
            ConstantsDescription.WriteTo(writer);
            writer.Write(")");
            if (Children.Count > 0)
            {
                writer.WriteLine(" {");
                foreach (var item in Children)
                {
                    item.WriteTo(writer, false, indentLevel + 1, indentString);
                }
                for (int i = 0; i < indentLevel; i++)
                    writer.Write(indentString);
                writer.Write("}");
            }
            if (SingleBranch != null)
            {
                writer.Write(":");
                SingleBranch.WriteTo(writer, true, indentLevel);
            }
            if (Concatenation != null)
            {
                writer.Write(" & ");
                Concatenation.WriteTo(writer, true, indentLevel);
            }
        }
        if (!skipBranchName)
        {
            writer.WriteLine(";");
        }
    }
}
