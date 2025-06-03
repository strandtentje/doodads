using System.Collections.Generic;
using Ziewaar.RAD.Doodads.RKOP.Exceptions;


namespace Ziewaar.RAD.Doodads.RKOP;

public class ServiceDescription : IParityParser
{
    public string ServiceTypeName;
    public ServiceConstantsDescription ConstantsDescription = new ServiceConstantsDescription();
    public List<ServiceDescription> Children = new List<ServiceDescription>();
    public ServiceDescription RedirectsTo;

    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        text = text.
            SkipWhile(char.IsWhiteSpace);

        if (text.Position == text.Text.Length)
            // clean up here!
            return ParityParsingState.Void;

        text = text.
            TakeToken(TokenDescription.Identifier, out var serviceReferenceToken);

        if (!serviceReferenceToken.IsValid)
            // or clean up here!
            return ParityParsingState.Void;

        text = text.
            SkipWhile(char.IsWhiteSpace).
            ValidateToken(TokenDescription.BranchAnnouncement, out var assignment).
            SkipWhile(char.IsWhiteSpace).
            ValidateToken(TokenDescription.Identifier, out var serviceNameToken);

        var state = ParityParsingState.Unchanged;
        if (!serviceNameToken.Text.StartsWith("_"))
        {
            this.RedirectsTo = null;
            text = text.
                SkipWhile(char.IsWhiteSpace).
                ValidateToken(TokenDescription.StartOfArguments, out var _);


            if (string.IsNullOrWhiteSpace(this.ConstantsDescription.Key))
                state |= ParityParsingState.New;
            else if (this.ConstantsDescription.Key != serviceReferenceToken.Text)
                state |= ParityParsingState.Changed;
            this.ConstantsDescription.Key = serviceReferenceToken.Text;

            if (string.IsNullOrWhiteSpace(this.ServiceTypeName))
                state |= ParityParsingState.New;
            else if (this.ConstantsDescription.Key != serviceNameToken.Text)
                state |= ParityParsingState.Changed;
            this.ServiceTypeName = serviceNameToken.Text;


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
                        Children.Add(new ServiceDescription());
                    lastState = Children[childCounter].UpdateFrom(ref text);
                    state |= lastState;
                    if (lastState > ParityParsingState.Void)
                        childCounter++;
                } while (lastState != ParityParsingState.Void);

                while(childCounter < Children.Count)
                {
                    Children.RemoveAt(childCounter);
                    state |= ParityParsingState.Changed;
                }
                        
                text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.BlockClose, out var _);
                text = text.ExitScope();
            }
            text[ConstantsDescription.Key] = this;
        } else
        {
            this.RedirectsTo = text[serviceNameToken.Text] as ServiceDescription ?? 
                throw new ReferenceException($"Referring to unknown existing service {serviceNameToken.Text}");
        }


        text = text.TakeToken(TokenDescription.Terminator, out var terminator);

        return state;
    }

}
