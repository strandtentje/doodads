#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Reflection;
[Category("Reflection")]
[Title("Determine service expression type")]
[Description("""
             Provided the special service information, this determines whether this is a nested
             series of expressions (the stuff that handles colons, pipes and ampersands), or a discrete 
             service definition such as Template("hi") {  }

             OnElse emits when a series was found, and puts the following data in memory:
              - expression : propagates the incoming memory service information
              - seriestype : string saying `unconditional` (&) , `alternative` (|) or `conditional` (:)
              - scopename  : name the parser came up with to uniquely identify this expression in the scope
              - children   : list of underlying service information which may be iterated.
              
             OnThen emits when a discrete service definition was found, and puts the following data in memory:
              - expression : propagation of service information in memory
              - servicetype : name of type as its defined in the .net libs
              - scopename : name of this definition in scope as figured out by the parser.
              - primarysetting : string-cast value of the primary setting.
              - workingdir : working directory of this service's file; useful for relative path strings.
              - constnames : list of settings that have values assigned
              - childnames : list of children hooked up to named event branches. does not include series. 
             """)]
public class ServiceDiscriminator : IService
{
    [EventOccasion("When a discrete service definition was found")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When a service series was found")]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   may happen when:
                    - there was no service expression information in the register
                    - series or service had no scope name 
                    - series or service had no children collection
                    - the series or service type could not be determined
                   """)]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.Register is not ServiceExpression<ServiceBuilder> serviceExpression)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "service expression expected in register"));
            return;
        }

        if (serviceExpression is SerializableServiceSeries<ServiceBuilder> series)
        {
            if (series.CurrentNameInScope == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "series has no name in scope"));
                return;
            }
            if (series.Children == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "series has no children collection"));
                return;
            }
            string? seriesType = series switch
            {
                UnconditionalSerializableServiceSeries<ServiceBuilder> _ => "unconditional",
                AlternativeSerializableServiceSeries<ServiceBuilder> _ => "alternative",
                ConditionalSerializableServiceSeries<ServiceBuilder> _ => "conditional",
                _ => null,
            };

            if (seriesType == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "unexpected series type"));
                return;
            }

            OnElse?.Invoke(this, new CommonInteraction(interaction, seriesType, new SortedList<string, object>()
            {
                { "expression", series },
                { "seriestype", seriesType },
                { "scopename", series.CurrentNameInScope },
                { "children", series.Children },
            }));
        }
        else if (serviceExpression is ServiceDescription<ServiceBuilder> description)
        {
            if (description.Constructor.ServiceTypeName == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "service had no type name"));
                return;
            }
            if (description.CurrentNameInScope == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "service had no scope name"));
                return;
            }

            OnThen?.Invoke(this, new CommonInteraction(interaction, description.Constructor.ServiceTypeName,
                new SortedList<string, object>()
                {
                    { "expression", description },
                    { "servicetype", description.Constructor.ServiceTypeName },
                    { "scopename", description.CurrentNameInScope },
                    { "primarysetting", description.Constructor.PrimaryExpression.GetValue()?.ToString() ?? "" },
                    { "workingdir", description.TextScope.WorkingDirectory },
                    { "constnames", description.Constructor.Constants.Members.Select(x => x.Key).ToArray() },
                    { "childnames", description.Children.Branches?.Select(x => x.key).ToArray() ?? [] },
                }));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}