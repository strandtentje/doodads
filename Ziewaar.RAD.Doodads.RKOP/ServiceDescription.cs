using System;
using System.Collections.Generic;
using System.IO;
using Ziewaar.RAD.Doodads.RKOP.Exceptions;
namespace Ziewaar.RAD.Doodads.RKOP;
#nullable enable

public class ServiceDescription<TResult> : IParityParser where TResult : class, IInstanceWrapper, new()
{
    public string? ServiceTypeName;
    public ServiceConstantsDescription ConstantsDescription = new ServiceConstantsDescription();
    public List<ServiceDescription<TResult>> Children = new List<ServiceDescription<TResult>>();
    public ServiceDescription<TResult>? RedirectsTo;
    public ServiceDescription<TResult>? Concatenation;
    public ServiceDescription<TResult>? SingleBranch;
    public DirectoryInfo? WorkingDirectory;

    public TResult? Wrapper { get; private set; }
    public ParityParsingState UpdateFrom(ref CursorText text) => this.UpdateFrom(ref text, null);
    public ParityParsingState UpdateFrom(ref CursorText text, ServiceDescription<TResult>? source = null)
    {
        text = text.
            SkipWhile(char.IsWhiteSpace);

        if (text.Position == text.Text.Length)
        {
            Wrapper?.Cleanup();
            return ParityParsingState.Void;
        }
        var state = ParityParsingState.Unchanged;

        if (source == null)
        {
            text = text.
                TakeToken(TokenDescription.Identifier, out var referenceIdentifier);
            if (!referenceIdentifier.IsValid)
            {
                Wrapper?.Cleanup();
                return ParityParsingState.Void;
            }
            text = text.
                SkipWhile(char.IsWhiteSpace).
                ValidateToken(TokenDescription.BranchAnnouncement, out var assignment);
            if (string.IsNullOrWhiteSpace(this.ConstantsDescription.Key))
                state |= ParityParsingState.New;
            else if (this.ConstantsDescription.Key != referenceIdentifier.Text)
                state |= ParityParsingState.Changed;
            this.ConstantsDescription.Key = referenceIdentifier.Text;
        }
        else
        {
            var preceedingIdentifier = source.ConstantsDescription.Key;
            if (string.IsNullOrWhiteSpace(this.ConstantsDescription.Key))
                state |= ParityParsingState.New;
            else if (this.ConstantsDescription.Key != preceedingIdentifier)
                state |= ParityParsingState.Changed;
            this.ConstantsDescription.Key = preceedingIdentifier;
        }

        this.WorkingDirectory = text.WorkingDirectory;

        text = text.
            SkipWhile(char.IsWhiteSpace);

        var startOfText = text;

        text = text.
            ValidateToken(TokenDescription.Identifier, out var typeIdentifier);

        if (!typeIdentifier.Text.StartsWith("_"))
        {
            this.Wrapper = new();
            this.RedirectsTo = null;
            text = text.
                SkipWhile(char.IsWhiteSpace).
                ValidateToken(TokenDescription.StartOfArguments, out var _);

            if (string.IsNullOrWhiteSpace(this.ServiceTypeName))
                state |= ParityParsingState.New;
            else if (this.ServiceTypeName != typeIdentifier.Text)
                state |= ParityParsingState.Changed;
            this.ServiceTypeName = typeIdentifier.Text;

            // do something with service ref AND OR type name being changed y/n
            state |= ConstantsDescription.UpdateFrom(ref text); // do something with constants being changed y/n

            text = text.
                SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.EndOfArguments, out var _).
                SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.BlockOpen, out var openBlock);

            if (openBlock.IsValid)
            {
                text = text.EnterScope();
                int childCounter = 0;
                ParityParsingState lastState = ParityParsingState.Void;
                do
                {
                    if (Children.Count == childCounter)
                        Children.Add(new ServiceDescription<TResult>());
                    lastState = Children[childCounter].UpdateFrom(ref text);
                    state |= lastState;
                    if (lastState > ParityParsingState.Void)
                        childCounter++;
                } while (lastState != ParityParsingState.Void);

                while (childCounter < Children.Count)
                {
                    Children.RemoveAt(childCounter);
                    state |= ParityParsingState.Changed;
                }

                text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.BlockClose, out var _);
                text = text.ExitScope();
            }
            else
            {
                Children.Clear();
            }

            text[ConstantsDescription.Key] = this;

            text = text.SkipWhile(char.IsWhiteSpace);
            text = text.TakeToken(TokenDescription.Chainer, out var chainer);

            switch (chainer.Text[0])
            {
                case '&':
                    Concatenation = Concatenation ?? new ServiceDescription<TResult>();
                    state |= Concatenation.UpdateFrom(ref text, this);
                    break;
                case ':':
                    SingleBranch = SingleBranch ?? new ServiceDescription<TResult>();
                    state |= SingleBranch.UpdateFrom(ref text, this);
                    break;
                case ';':
                    Concatenation?.Wrapper?.Cleanup();
                    SingleBranch?.Wrapper?.Cleanup();
                    Concatenation = null;
                    SingleBranch = null;
                    state |= ParityParsingState.Changed;
                    break;
                default:
                    throw new ParsingException("Expected &, : or ;");
            }

            if (state > ParityParsingState.Unchanged)
            {
                TResult? concatenationWrapper = null, singleWrapper = null;
                if (Concatenation != null && Concatenation.Wrapper != null)
                    concatenationWrapper = Concatenation.Wrapper;
                if (SingleBranch != null && SingleBranch.Wrapper != null)
                    singleWrapper = SingleBranch.Wrapper;

                Wrapper.SetDefinition(
                    startOfText,
                    this.ServiceTypeName,
                    concatenationWrapper,
                    singleWrapper,
                    this.ConstantsDescription.ToSortedList(),
                    this.Children.ToSortedList());
            }
        }
        else
        {
            this.Wrapper?.Cleanup();
            this.Wrapper = new();
            this.RedirectsTo = text[typeIdentifier.Text] as ServiceDescription<TResult> ??
                throw new ReferenceException($"Referring to unknown existing service {typeIdentifier.Text}");
            Wrapper.SetReference(this.RedirectsTo);
            text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.Terminator, out var _);
        }

        return state;
    }

    public void WriteTo(StreamWriter writer, int indentLevel = 0, string indentString = "    ")
    {
        for (int i = 0; i < indentLevel; i++)
            writer.Write(indentString);
        writer.Write(ConstantsDescription.Key);
        writer.Write("->");
        if (RedirectsTo is ServiceDescription<TResult> redir)
        {
            writer.Write(redir.ConstantsDescription.Key);
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
                    item.WriteTo(writer, indentLevel + 1, indentString);
                }
                for (int i = 0; i < indentLevel; i++)
                    writer.Write(indentString);
                writer.Write("}");
            }
            if (Concatenation != null && SingleBranch != null)
                throw new InvalidOperationException("cannot have a conatenation branch, and a single branch");
            if (Concatenation != null)
            {
                writer.Write("&");
                Concatenation.WriteTo(writer, indentLevel);
            } else if (SingleBranch != null)
            {
                writer.Write(":");
                SingleBranch.WriteTo(writer, indentLevel);
            }
        }
    }
}
