// See https://aka.ms/new-console-template for more information
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.Hardcoding;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

Console.WriteLine("Hello, World!");

Service<StartWebServer>.Configure(null, starter => 
    starter.ToStart += Service<WebServer>.Configure(
        new ServiceConstants { { "prefixes", "http://*:41968/" } },
        server => server.HandleRequest += Service<ConstantTextSource>.Configure(
            new ServiceConstants { { "1", "hellorld" } })));