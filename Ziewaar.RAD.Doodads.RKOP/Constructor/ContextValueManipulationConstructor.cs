#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor
{
    public class ContextValueManipulationConstructor : ISerializableConstructor
    {
        public enum ShorthandType
        {
            NoShorthand,
            InvalidShorthand,
            Default,
            Override,
        };

        public ShorthandType CurrentShorthandType { get; private set; } = ShorthandType.InvalidShorthand;

        public string? ServiceTypeName
        {
            get =>
                CurrentShorthandType != ShorthandType.InvalidShorthand
                    ? Enum.GetName(typeof(ShorthandType), CurrentShorthandType)
                    : throw new Exception("Unknown Shorthand");
            set
            {
                CurrentShorthandType = (ShorthandType)Enum.Parse(typeof(ShorthandType), value ?? "InvalidShorthand");
                if (CurrentShorthandType is ShorthandType.InvalidShorthand or ShorthandType.NoShorthand)
                    throw new Exception("May only set this to Definition, Call or Case");
            }
        }

        public IReadOnlyDictionary<string, object> ConstantsList => new SwitchingDictionary(["set"], s => s switch
        {
            "set" => SetExpression.GetValue(),
            _ => throw new KeyNotFoundException(),
        });

        private ServiceConstantExpression PrimaryExpression = new();
        private ServiceConstantExpression SetExpression = new();
        public object? PrimarySettingValue => PrimaryExpression.GetValue();

        public bool UpdateFrom(ref CursorText text)
        {
            var tempCurPos = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.Wiggly, out var wiggly)
                .TakeToken(TokenDescription.HatShorthand, out var hat)
                .TakeToken(TokenDescription.StoreShorthand, out var store);

            this.CurrentShorthandType = (wiggly.IsValid, hat.IsValid, store.IsValid) switch
            {
                (true, false, true) => ShorthandType.Default,
                (false, true, true) => ShorthandType.Override,
                _ => ShorthandType.NoShorthand,
            };

            if (this.CurrentShorthandType == ShorthandType.NoShorthand)
                return false;

            text = tempCurPos.SkipWhile(char.IsWhiteSpace);
            PrimaryExpression.UpdateFrom(ref text);

            text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.AssignmentOperator, out var assignment);
            SetExpression = new();
            if (assignment.IsValid)
            {
                text = text.SkipWhile(char.IsWhiteSpace);
                SetExpression.UpdateFrom(ref text);
            }

            return true;
        }

        public void WriteTo(StreamWriter writer, int indentation)
        {
            switch (CurrentShorthandType)
            {
                case ShorthandType.Default:
                    writer.Write(" ~! ");
                    break;
                case ShorthandType.Override:
                    writer.Write(" ^! ");
                    break;
            }

            PrimaryExpression.WriteTo(writer);
            if (!string.IsNullOrWhiteSpace(SetExpression.GetValue().ToString()))
            {
                writer.Write(" = ");
                SetExpression.WriteTo(writer);
            }
            writer.Write(" ");
        }
    }
}