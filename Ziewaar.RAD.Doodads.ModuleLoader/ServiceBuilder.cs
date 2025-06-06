using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class ServiceBuilder : IInstanceWrapper, IEntryPoint
{
    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

    private Type currentType;
    private SortedList<string, Delegate> existingEventHandlers = new();
    private ServiceBuilder Concatenation;
    private ServiceConstants CurrentConstants;

    private EventHandler<IInteraction> existingSingleBranchHandler = null;
    private ServiceBuilder Redirection;

    public string CurrentTypeName { get; private set; }
    public IService CurrentInstance { get; private set; }
    public void Cleanup()
    {
        if (CurrentInstance != null)
        {
            var allEvents = currentType.GetEvents().ToArray();

            if (existingEventHandlers != null)
            {
                foreach (var item in allEvents)
                {
                    if (existingEventHandlers.TryGetValue(item.Name, out var dlg))
                        item.RemoveEventHandler(this.CurrentInstance, dlg);
                }
            }

            EventInfo singleEvent = null;
            if (allEvents.Count() == 1)
                singleEvent = allEvents.ElementAt(1);

            if (singleEvent == null)
            {
                var defaultsByAttribute = allEvents.Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(DefaultBranchAttribute)));

                if (defaultsByAttribute.Count() == 1)
                    singleEvent = defaultsByAttribute.ElementAt(0);
            }

            if (existingSingleBranchHandler != null &&
                singleEvent != null)
            {
                singleEvent.RemoveEventHandler(this.CurrentInstance, existingSingleBranchHandler);
            }


            if (CurrentInstance is IDisposable disposable)
                disposable.Dispose();

            this.CurrentInstance = null;
            this.CurrentTypeName = null;
            this.currentType = null;
            this.existingEventHandlers.Clear();
            this.Concatenation = null;
            this.CurrentConstants = null;
            this.existingSingleBranchHandler = null;
            this.Redirection = null;
        }
    }

    public void Run(IInteraction e)
    {
        if (Redirection != null)
        {
            Redirection.Run(e);
        }
        else
        {
            CurrentInstance.Enter(CurrentConstants, e);
            Concatenation?.Run(e);
        }
    }

    public void SetDefinition<TResult>(
        CursorText atPosition,
        string typename,
        TResult concatenation,
        TResult singleBranch,
        SortedList<string, object> constants,
        SortedList<string, TResult> namedBranches)
        where TResult : IInstanceWrapper, new()
    {
        this.Redirection = null;
        if (this.CurrentTypeName != typename)
        {
            if (this.CurrentInstance is IDisposable disposable)
                disposable.Dispose();
            this.CurrentTypeName = typename;
            this.CurrentInstance = TypeRepository.Instance.CreateInstanceFor(this.CurrentTypeName, out this.currentType);
            this.existingEventHandlers.Clear();
            this.existingSingleBranchHandler = null;
        }

        if (concatenation is IInstanceWrapper concatWrapper &&
            concatWrapper is ServiceBuilder concatBuilder)
        {
            this.Concatenation = concatBuilder;
        }

        this.CurrentConstants = new ServiceConstants(constants);
        this.CurrentConstants.LastChange = Stopwatch.Elapsed;

        var allEvents = currentType.GetEvents().ToArray();
        var newEventHandlers = new SortedList<string, Delegate>();

        EventInfo singleEvent = null;
        if (allEvents.Count() == 1)
            singleEvent = allEvents.ElementAt(1);

        if (singleEvent == null)
        {
            var defaultsByAttribute = allEvents.Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(DefaultBranchAttribute)));

            if (defaultsByAttribute.Count() == 1)
                singleEvent = defaultsByAttribute.ElementAt(0);
        }

        if (existingSingleBranchHandler != null &&
            singleEvent != null)
        {
            singleEvent.RemoveEventHandler(this.CurrentInstance, existingSingleBranchHandler);
        }

        foreach (var item in allEvents)
        {
            if (existingEventHandlers.TryGetValue(item.Name, out var dlg))
                item.RemoveEventHandler(this.CurrentInstance, dlg);

            if (namedBranches.TryGetValue(item.Name, out var child))
            {
                var iWrapChild = (IInstanceWrapper)child;
                var sbChild = (ServiceBuilder)iWrapChild;
                var newEvent = newEventHandlers[item.Name] = new EventHandler<IInteraction>((s, e) =>
                {
                    sbChild.Run(e);
                });
                item.AddEventHandler(this.CurrentInstance, newEvent);
            }
        }

        existingEventHandlers = newEventHandlers;

        if (singleEvent != null &&
            singleBranch is IInstanceWrapper singleWrapper &&
            singleWrapper is ServiceBuilder singleBuilder)
        {
            existingSingleBranchHandler = new EventHandler<IInteraction>((s, e) =>
            {
                singleBuilder.Run(e);
            });
        }
    }

    public void SetReference<TResult>(ServiceDescription<TResult> redirectsTo) where TResult : class, IInstanceWrapper, new()
    {
        if (redirectsTo.Wrapper is IInstanceWrapper redirWrapper &&
            redirWrapper is ServiceBuilder redirBuilder)
        {
            this.Redirection = redirBuilder;
        }
    }
}