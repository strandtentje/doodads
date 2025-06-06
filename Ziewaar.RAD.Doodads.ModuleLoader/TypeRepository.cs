﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class TypeRepository
{
    public static readonly TypeRepository Instance = new();
    private readonly SortedList<string, Type> namedServiceTypes = new();
    public TypeRepository PopulateWith(params string[] assemblyFiles)
    {
        foreach (var assemblyPath in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            PopulateWith(assembly);
        }
        return this;
    }
    public TypeRepository PopulateWith(Assembly assembly)
    {
        var serviceTypes = assembly.GetTypes().Where(x => typeof(IService).IsAssignableFrom(x));
        foreach (var serviceType in serviceTypes)
        {
            namedServiceTypes.Add(serviceType.Name, serviceType);
        }
        return this;
    }
    public IService CreateInstanceFor(string name, out Type foundType)
    {
        foundType = namedServiceTypes[name];
        return (IService)Activator.CreateInstance(foundType ??
            throw new MissingServiceTypeException(name));
    }
    public string[] GetAvailableNames() => namedServiceTypes.Keys.ToArray();
    public bool HasName(string newTypeName) => namedServiceTypes.ContainsKey(newTypeName);
}
