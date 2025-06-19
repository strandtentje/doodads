using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.SQLite;

public class SqliteConnectionSource : IService
{
    public event CallForInteraction OnThen;
    public event CallForInteraction OnElse;
    public event CallForInteraction OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        throw new NotImplementedException();
    }
    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}
