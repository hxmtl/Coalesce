﻿using System.Collections.Generic;
using System.Linq;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class SymbolClassViewModel : ClassViewModel
    {
        protected ITypeSymbol Symbol { get; }

        public SymbolClassViewModel(ITypeSymbol symbol)
        {
            Symbol = symbol;
        }

        public override string Name => Symbol.Name;

        public override string Namespace => FullNamespace(Symbol.ContainingNamespace);

        public override string Comment => SymbolHelper.ExtractXmlComments(Symbol);

        /// <summary>
        /// Recursive function to get the full namespace.
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        private string FullNamespace(INamespaceSymbol ns)
        {
            if (ns.ContainingNamespace != null && !string.IsNullOrWhiteSpace(ns.Name))
            {
                var rest = FullNamespace(ns.ContainingNamespace);
                if (!string.IsNullOrWhiteSpace(rest))
                {
                    return FullNamespace(ns.ContainingNamespace) + "." + ns.Name;
                }
                return ns.Name;
            }
            return "";
        }

        internal override ICollection<PropertyWrapper> RawProperties
        {
            get
            {
                var result = Symbol.GetMembers()
                    .Where(s => s.Kind == SymbolKind.Property)
                    .Cast<IPropertySymbol>()
                    .Select(s => new SymbolPropertyWrapper(s))
                    .Cast<PropertyWrapper>()
                    .ToList();

                // Add properties from the base class
                if (Symbol.BaseType != null && Symbol.BaseType.Name != "Object")
                {
                    var parentSymbol = new SymbolClassViewModel(Symbol.BaseType);
                    result.AddRange(parentSymbol.RawProperties);
                }
                return result;
            }
        }

        internal override ICollection<MethodWrapper> RawMethods
        {
            get
            {
                var result = Symbol.GetMembers()
                    .Where(f => f.Kind == SymbolKind.Method && f.DeclaredAccessibility == Accessibility.Public)
                    .Cast<IMethodSymbol>()
                    .Where(f => f.MethodKind == MethodKind.Ordinary)
                    .Select(s => new SymbolMethodWrapper(s))
                    .ToList();

                // Add methods from the base class
                if (Symbol.BaseType != null && Symbol.BaseType.Name != "Object")
                {
                    var parentSymbol = new SymbolClassViewModel(Symbol.BaseType);
                    result.AddRange(parentSymbol.Methods
                        .Cast<SymbolMethodWrapper>()
                        // Don't add overriden methods
                        .Where(baseMethod => !result.Any(method => method.Symbol.OverriddenMethod == baseMethod.Symbol)
                    ));
                }

                return result
                    .Cast<MethodWrapper>()
                    .ToList();
            }
        }


        public override bool IsDto => Symbol.AllInterfaces.Any(f => f.Name.Contains("IClassDto"));

        public override ClassViewModel DtoBaseViewModel
        {
            get
            {
                var iDto = Symbol.AllInterfaces.FirstOrDefault(f => f.Name.Contains("IClassDto"));
                if (iDto != null)
                {
                    ClassViewModel baseModel = ReflectionRepository.GetClassViewModel(iDto.TypeArguments[0]);
                    return baseModel;
                }
                return null;
            }
        }

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Symbol.GetAttributeValue<TAttribute>(valueName);

        public override bool HasAttribute<TAttribute>() =>
            Symbol.HasAttribute<TAttribute>();

        protected override AttributeWrapper GetSecurityAttribute<TAttribute>() =>
            new AttributeWrapper
            {
                AttributeData = Symbol.GetAttribute<TAttribute>()
            };
    }
}