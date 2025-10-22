using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;
// ReSharper disable MergeIntoPattern

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
#nullable enable
public class Discriminate : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public event CallForInteraction?
        OnUnconditional,
        OnAlternative,
        OnConditional,
        OnRegularService,
        OnContextValueManipulator,
        OnPrefixShorthand,
        OnCapturedShorthand;

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