namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions
{
    public class ConnectionDeadException(Exception? ex = null) : Exception("Connection dead.", ex);
}