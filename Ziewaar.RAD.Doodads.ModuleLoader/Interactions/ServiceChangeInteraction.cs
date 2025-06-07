
using System;
using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;

public class ServiceChangeInteraction(IInteraction parent, ServiceDescription<ServiceBuilder> definition, KnownProgram program) : IInteraction
{
    public ServiceDescription<ServiceBuilder> Definition => definition;
    public KnownProgram Program => program;
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables => throw new NotImplementedException();
}
