using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;
// ReSharper disable MergeIntoPattern

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#nullable enable
#pragma warning disable 67
[Category("Reflection & Documentation")]
[Title("Determine on what kind of part of the program we're working")]
[Description("""
             We recurse through syntax members. The stack is generally but not guaranteed to be
             1. Unconditional Sequence (& ampersand coupled)
             2. Alternative Sequence (| pipe coupled to catch all OnElse)
             3. Conditional Sequence (: colon coupled to catch the closest OnThen)
             4. Regular Service ( `ForExample(asignature="likethis")` )
             5. OnContextValueManipulator ( like `^!"variablename" = "something"` )
             6. OnPrefixShorthand ( like `~"case"` )
             7. OnCapturedShorthand ( like `["Continue"]` )
             """)]
public class Discriminate : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("when we encountered something indiscriminable")]
    public event CallForInteraction? OnException;
    [EventOccasion("In case of an unconditional coupling series")]
    public event CallForInteraction? OnUnconditional;
    [EventOccasion("In case of an alternative coupling series")]
    public event CallForInteraction? OnAlternative;
    [EventOccasion("In case of an conditional coupling series")]
    public event CallForInteraction? OnConditional;
    [EventOccasion("In case of a regular service")]
    public event CallForInteraction? OnRegularService;
    [EventOccasion("In case of context value manipulator service")]
    public event CallForInteraction? OnContextValueManipulator;
    [EventOccasion("In case of a prefix shorthand service")]
    public event CallForInteraction? OnPrefixShorthand;
    [EventOccasion("In case of a captured shorthand service")]
    public event CallForInteraction? OnCapturedShorthand;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryFindVariable(ReflectionKeys.ServiceExpression,
                out object? series) ||
            series is not ServiceExpression<ServiceBuilder> expression)
            OnException?.Invoke(this,
                interaction.AppendRegister("no suitable expression in memory"));
        else if (expression is
                 UnconditionalSerializableServiceSeries<ServiceBuilder>
                 unconditional)
            OnUnconditional?.Invoke(this, interaction.AppendMemory([
                (ReflectionKeys.Series, unconditional),
                (ReflectionKeys.ScopeName,
                    unconditional.CurrentNameInScope ?? "")
            ]));
        else if (expression is
                 AlternativeSerializableServiceSeries<ServiceBuilder>
                 alternative)
            OnAlternative?.Invoke(this, interaction.AppendMemory([
                (ReflectionKeys.Series, alternative),
                (ReflectionKeys.ScopeName, alternative.CurrentNameInScope ?? "")
            ]));
        else if (expression is
                 ConditionalSerializableServiceSeries<ServiceBuilder>
                 conditional)
            OnConditional?.Invoke(this, interaction.AppendMemory([
                (ReflectionKeys.Series, conditional),
                (ReflectionKeys.ScopeName, conditional.CurrentNameInScope ?? "")
            ]));
        else if
            (expression is ServiceDescription<ServiceBuilder> regularService &&
             regularService.CurrentConstructor is RegularNamedConstructor
                 regularConstructor)
            OnRegularService?.Invoke(this, interaction.AppendMemory([
                (ReflectionKeys.Service, regularService),
                (ReflectionKeys.ServiceType,
                    regularConstructor.ServiceTypeName ?? ""),
                (ReflectionKeys.ServiceSetting,
                    regularConstructor.PrimarySettingValue ?? ""),
                (ReflectionKeys.RegularConstructor,
                    regularConstructor),
                (ReflectionKeys.ScopeName,
                    regularService.CurrentNameInScope ?? "")
            ]));
        else if
            (expression is ServiceDescription<ServiceBuilder> contextService &&
             contextService.CurrentConstructor is
                 ContextValueManipulationConstructor contextConstructor)
            OnContextValueManipulator?.Invoke(this, interaction.AppendMemory([
                (ReflectionKeys.Service, contextService),
                (ReflectionKeys.ServiceType,
                    contextConstructor.ServiceTypeName ?? ""),
                (ReflectionKeys.ServiceSetting,
                    contextConstructor.PrimarySettingValue ?? ""),
                (ReflectionKeys.ContextConstructor,
                    contextConstructor),
                (ReflectionKeys.ScopeName,
                    contextService.CurrentNameInScope ?? "")
            ]));
        else if
            (expression is ServiceDescription<ServiceBuilder> prefixService &&
             prefixService.CurrentConstructor is PrefixedShorthandConstructor
                 prefixConstructor)
            OnPrefixShorthand?.Invoke(this, interaction.AppendMemory([
                (ReflectionKeys.Service, prefixService),
                (ReflectionKeys.ServiceType,
                    prefixConstructor.ServiceTypeName ?? ""),
                (ReflectionKeys.ServiceSetting,
                    prefixConstructor.PrimarySettingValue),
                (ReflectionKeys.PrefixConstructor,
                    prefixConstructor),
                (ReflectionKeys.ScopeName,
                    prefixService.CurrentNameInScope ?? "")
            ]));
        else if (expression is ServiceDescription<ServiceBuilder>
                     capturedService &&
                 capturedService.CurrentConstructor is
                     CapturedShorthandConstructor capturedConstructor)
            OnCapturedShorthand?.Invoke(this, interaction.AppendMemory([
                (ReflectionKeys.Service, capturedService),
                (ReflectionKeys.ServiceType,
                    capturedConstructor.ServiceTypeName ?? ""),
                (ReflectionKeys.ServiceSetting,
                    capturedConstructor.PrimarySettingValue),
                (ReflectionKeys.CapturedConstructor,
                    capturedConstructor),
                (ReflectionKeys.ScopeName,
                    capturedService.CurrentNameInScope ?? "")
            ]));
        else
            OnException?.Invoke(this,
                interaction.AppendRegister("unable to discriminate"));
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}