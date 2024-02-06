using System;
using System.Reflection;

namespace Oxide.DependencyInjection;

/// <summary>
/// Decorate a constructor with this attribute to tell the <see cref="ActivationUtility"/> what constructors
/// it should be using for object creation
/// </summary>
/// <remarks>
/// If no constructors are tagged then every <see cref="BindingFlags.Public"/> | <see cref="BindingFlags.Instance"/> constructor
/// is a valid candidate.
/// </remarks>
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public class ActivatorConstructorAttribute : Attribute
{
}
