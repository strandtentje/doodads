namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public record ControlBox(Semaphore Slots, HttpListener Listener);