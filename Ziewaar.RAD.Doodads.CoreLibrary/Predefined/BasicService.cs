using System.Globalization;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined
{
    public abstract class BasicService : IService
    {
        [NeverHappens] public virtual event CallForInteraction? OnThen;
        [NeverHappens] public virtual event CallForInteraction? OnElse;

        [EventOccasion(
            "When something goes wrong on the implementing service, a description of the failure is in register.")]
        public virtual event CallForInteraction? OnException;

        protected static readonly CallForInteraction Idle = (sender, interaction) => { };

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            try
            {
                TryEnter(constants, interaction);
            }
            catch (BasicException ex)
            {
                OnException?.Invoke(this, interaction.AppendRegister(ex.Message));
            }
        }

        public static bool IsntJustAnObject(object obj) => !obj.GetType().IsAssignableFrom(typeof(object));

        protected string? Primary(StampedMap constants) =>
            IsntJustAnObject(constants.PrimaryConstant) &&
            constants.PrimaryConstant.ToString() is { } candidateFromConstants &&
            !string.IsNullOrWhiteSpace(candidateFromConstants)
                ? candidateFromConstants
                : null;

        protected string? Named(StampedMap constants, UpdatingKeyValue setting) => Named(constants, setting.Key);
        protected string? Named(StampedMap constants, string name) =>
            constants.NamedItems.TryGetValue(name, out var item) && IsntJustAnObject(item) &&
            Convert.ToString(item, CultureInfo.InvariantCulture) is { } candidateFromConstants && 
            !string.IsNullOrWhiteSpace(candidateFromConstants) ? candidateFromConstants : null;

        private class LinesSinkEnumerable(TextSinkingInteraction tsi) : IEnumerable<string>
        {
            public IEnumerator<string> GetEnumerator() => tsi.ReadAllLines().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected IInteraction LinesSink(IInteraction interaction, out IEnumerable<string> linesEnumerable)
        {
            var tsi = new TextSinkingInteraction(interaction);
            linesEnumerable = new LinesSinkEnumerable(tsi);
            return tsi;
        }
        protected IEnumerable<string> PrimaryPartsFromMemory(StampedMap constants, IInteraction interaction)
        {
            var verbatimParts = PrimaryParts(constants);
            foreach (var item in verbatimParts)
            {
                string replaced = FillPlaceholders(item, interaction);
                yield return replaced;
            }
        }

        protected IEnumerable<string> NamedPartsFromMemory(StampedMap constants, UpdatingKeyValue setting,
            IInteraction interaction) => NamedPartsFromMemory(constants, setting.Key, interaction);
            
        protected IEnumerable<string> NamedPartsFromMemory(StampedMap constants, string name, IInteraction interaction)
        {
            var parts = NamedParts(constants, name);
            foreach (var part in parts)
            {
                string replaced = FillPlaceholders(part, interaction);
                yield return replaced;
            }
        }

        public enum PlaceholderMode { Out, In, }

        private string FillPlaceholders(string item, IInteraction interaction)
        {
            StringBuilder sbl = new StringBuilder();
            PlaceholderMode mode = PlaceholderMode.Out;

            for (
                var puller = item.PullString("", out string part, '{', '}');
                !string.IsNullOrWhiteSpace(part);
                puller = puller.PullString("", out part))
            {
                if (mode != PlaceholderMode.In && puller.Current == '{' && !part.EndsWith(@"\"))
                {
                    mode = PlaceholderMode.In;
                    sbl.Append(part);
                }
                else if (mode == PlaceholderMode.In && puller.Current == '}' && !part.EndsWith(@"\"))
                {
                    mode = PlaceholderMode.Out;
                    BasicException.ForStringInMemory(interaction, part, out var memoryValue);
                    sbl.Append(memoryValue);
                } else if (mode == PlaceholderMode.Out)
                {
                    sbl.Append(part);
                } else
                {
                    throw new BasicException($"Strange occurence in placeholder string around position {puller.CurrentPosition}");
                }
            }
            return sbl.ToString();
        }

        protected Queue<string> PrimaryParts(StampedMap constants)
        {
            var valueFromConstant = constants.PrimaryConstant;
            return new Queue<string>(PartOut(valueFromConstant));
        }

        protected IEnumerable<string> NamedParts(StampedMap constants, UpdatingKeyValue setting) =>
            NamedParts(constants, setting.Key);
        protected IEnumerable<string> NamedParts(StampedMap constants, string name)
        {
            if (!constants.NamedItems.TryGetValue(name, out var valueFromConstant) || valueFromConstant == null)
                return Enumerable.Empty<string>();
            return PartOut(valueFromConstant);
        }
        
        private static IEnumerable<string> PartOut(object valueFromConstant)
        {
            if (valueFromConstant is not IEnumerable enumerable)
            {
                if (IsntJustAnObject(valueFromConstant) &&
                    valueFromConstant.ToString() is string single)
                    return [single];
                else
                    return [];
            }

            if (enumerable is string actuallyAString)
                return [actuallyAString];

            return enumerable.OfType<object>().Where(IsntJustAnObject).Select(x => x.ToString()).OfType<string>();
        }


        protected void RepeatCustom<T>(StampedMap constants, IInteraction source, Func<RepeatInteraction, T, IInteraction> factory, IEnumerable<T> items)
        {
            BasicException.ForNullOrEmpty(Primary(constants), "repeat name required in primary constant",
                out string riName);
            var ri = new RepeatInteraction(riName, source, CancellationToken.None)
            {
                IsRunning = true
            };
            using var en = items.GetEnumerator();
            while (ri.IsRunning && en.MoveNext() && en.Current != null)
            {
                ri.IsRunning = false;
                OnThen?.Invoke(this, factory(ri, en.Current));
            }
        }

        protected void RepeatToRegister(StampedMap constants, IInteraction source, IEnumerable<object> items, bool elseOnEmpty = false)
        {
            BasicException.ForNullOrEmpty(Primary(constants), "repeat name required in primary constant",
                out string riName);
            var ri = new RepeatInteraction(riName, source, CancellationToken.None)
            {
                IsRunning = true
            };
            using var en = items.GetEnumerator();
            bool anyItems = false;
            while (ri.IsRunning && en.MoveNext() && en.Current != null)
            {
                anyItems = true;
                ri.IsRunning = false;
                OnThen?.Invoke(this, ri.AppendRegister(en.Current));
            }

            if (!anyItems && elseOnEmpty)
                OnElse?.Invoke(this, source);
        }
        protected void RepeatToMemory(StampedMap constants, IInteraction source, IEnumerable<IReadOnlyDictionary<string, object>> items)
        {
            BasicException.ForNullOrEmpty(Primary(constants), "repeat name required in primary constant",
                out string riName);
            var ri = new RepeatInteraction(riName, source, CancellationToken.None)
            {
                IsRunning = true
            };
            using var en = items.GetEnumerator();
            while (ri.IsRunning && en.MoveNext() && en.Current != null)
            {
                ri.IsRunning = false;
                OnThen?.Invoke(this, ri.AppendMemory(en.Current));
            }
        }

        protected Queue<string> PrimaryOrRegisterParts(StampedMap constants, IInteraction interaction,
            params char[] regDelimiters)
        {
            var items = PrimaryParts(constants);
            return items.Any() ? items : new Queue<string>((Register(interaction) ?? "").Split(regDelimiters));
        }

        protected Queue<string> PrimaryAndRegisterParts(StampedMap constants, IInteraction interaction,
            params char[] regDelimiters) =>
            new(PrimaryParts(constants).Concat((Register(interaction) ?? "").Split(regDelimiters)));

        protected string[] PrimaryParts(StampedMap constants, params string[] defaults)
        {
            if (defaults.Length == 0) throw new ArgumentException("At least one default req'd");
            if (constants.PrimaryConstant is not IEnumerable enumerable)
                return defaults;
            if (enumerable is string actuallyAString)
            {
                defaults[0] = actuallyAString;
                return defaults;
            }

            using var partPuller = enumerable.OfType<object>().Where(IsntJustAnObject).Select(x => x.ToString())
                .Where(x => x != null).GetEnumerator();
            for (int i = 0; i < defaults.Length && partPuller.MoveNext(); i++)
                defaults[i] = partPuller.Current!;
            return defaults;
        }

        protected string? Register(IInteraction interaction) =>
            IsntJustAnObject(interaction.Register) &&
            Convert.ToString(interaction.Register, CultureInfo.InvariantCulture) is { } candidateFromRegister &&
            !string.IsNullOrWhiteSpace(candidateFromRegister)
                ? candidateFromRegister
                : null;

        protected string DirectoryFromRegister(IInteraction interaction) =>
            Register(interaction) is { } path && Directory.Exists(path)
                ? path
                : throw new BasicException("Directory in register did not exist");

        protected string FileFromRegister(IInteraction interaction) =>
            Register(interaction) is { } path && File.Exists(path)
                ? path
                : throw new BasicException("File in register did not exist");
        protected string FileFromPrimaryOrRegister(StampedMap constants, IInteraction interaction) =>
            PrimaryOrRegister(constants, interaction) is { } path && File.Exists(path)
                ? path
                : throw new BasicException("File in register did not exist");

        protected string? PrimaryOrRegister(StampedMap constants, IInteraction interaction)
        {
            return Primary(constants) ?? Register(interaction);
        }

        public abstract void TryEnter(StampedMap constants, IInteraction interaction);

        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}