﻿using System.Linq;
using Microsoft.CodeAnalysis;

namespace Typewriter.Metadata.Roslyn
{
    public static class Extensions
    {
        public static string GetFullName(this ISymbol symbol)
        {
            if (symbol is ITypeParameterSymbol)
                return symbol.Name;

            if (symbol is IDynamicTypeSymbol)
                return symbol.Name;

            var type = symbol as INamedTypeSymbol;
            var name = (type != null) ? GetFullTypeName(type) : symbol.Name;

            var ns = symbol.ContainingSymbol as INamespaceSymbol;
            if (ns?.IsGlobalNamespace == true)
            {
                return name;
            }

            return GetFullName(symbol.ContainingSymbol) + "." + name;
        }

        public static string GetNamespace(this ISymbol symbol)
        {
            if (string.IsNullOrEmpty(symbol.ContainingNamespace?.Name))
            {
                return null;
            }

            var restOfResult = GetNamespace(symbol.ContainingNamespace);
            var result = symbol.ContainingNamespace.Name;

            if (restOfResult != null)
                result = restOfResult + '.' + result;

            return result;
        }

        public static string GetFullTypeName(this INamedTypeSymbol type)
        {
            var result = type.Name;

            if (type.Name == "Nullable" && type.ContainingNamespace.Name == "System")
            {
                var typeSymbol = type.TypeArguments.First() as INamedTypeSymbol;

                if (typeSymbol != null)
                    return GetFullTypeName(typeSymbol) + "?";
            }

            if (type.TypeArguments.Any())
            {
                result += "<";
                result += string.Join(", ", type.TypeArguments.Select(t =>
                {
                    var typeSymbol = t as INamedTypeSymbol;
                    return typeSymbol == null ? t.Name : GetFullName(typeSymbol);
                }));
                result += ">";
            }
            else if (type.TypeParameters.Any())
            {
                result += "<";
                result += string.Join(", ", type.TypeParameters.Select(t => t.Name));
                result += ">";
            }

            return result;
        }
    }
}
