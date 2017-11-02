﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Helpers.Search;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class ClassViewModel : IAttributeProvider
    {
        
        protected ICollection<PropertyViewModel> _Properties;
        protected ICollection<MethodViewModel> _Methods;

        public static ClassViewModel From(TypeViewModel type)
        {
            // TODO: implement some sort of factory pattern for this.
            // Having ClassViewModel know about its derived types is quite undesirable.
            if (type.Wrapper is ReflectionTypeWrapper rw) return new ReflectionClassViewModel(rw.Info);
            if (type.Wrapper is SymbolTypeWrapper sw) return new SymbolClassViewModel(sw.Symbol);
            throw new ArgumentException("Unknown TypeViewModel wrapper type.");
        }

        public abstract string Name { get; }
        public abstract string Namespace { get; }
        public abstract string Comment { get; }


        public string FullName => Namespace + "." + Name;

        public string ControllerName => Name;

        public string ApiControllerClassName
        {
            get
            {
                var overrideName = this.GetAttributeValue<ControllerAttribute>(a => a.ApiControllerName);
                if (!string.IsNullOrWhiteSpace(overrideName)) return overrideName;

                var suffix = this.GetAttributeValue<ControllerAttribute>(a => a.ApiControllerSuffix);
                if (!string.IsNullOrWhiteSpace(suffix)) return $"{ControllerName}Controller{suffix}";

                return $"{ControllerName}Controller";
            }
        }

        public string ApiActionAccessModifier =>
            this.GetAttributeValue<ControllerAttribute, bool>(a => a.ApiActionsProtected) ?? false
            ? "protected"
            : "public";

        public string ApiName => Name;

        public string DtoName => IsDto ? Name : $"{Name}DtoGen";

        public ClassViewModel BaseViewModel => IsDto ? DtoBaseViewModel : this;

        /// <summary>
        /// If this class implements IClassDto, return true.
        /// </summary>
        public abstract bool IsDto { get; }

        /// <summary>
        /// If this class implements IClassDto, return the ClassViewModel this DTO is based upon.
        /// </summary>
        public abstract ClassViewModel DtoBaseViewModel { get; }

        /// <summary>
        /// Name of the ViewModelClass
        /// </summary>
        public string ViewModelClassName => Name;

        public string ViewModelGeneratedClassName
        {
            get
            {
                if (!HasTypeScriptPartial)
                    return ViewModelClassName;

                var name = this.GetAttributeValue<TypeScriptPartialAttribute>(a => a.BaseClassName);

                if (string.IsNullOrEmpty(name)) return $"{ViewModelClassName}Partial";

                return name;
            }
        }

        public bool ApiRouted => this.GetAttributeValue<ControllerAttribute, bool>(a => a.ApiRouted) ?? true;

        /// <summary>
        /// Name of the List ViewModelClass
        /// </summary>
        public string ListViewModelClassName => Name + "List";

        /// <summary>
        /// Name of an instance of the List ViewModelClass
        /// </summary>
        public string ListViewModelObjectName => ListViewModelClassName.ToCamelCase();




        #region Member Info - Properties & Methods

        internal abstract ICollection<PropertyWrapper> RawProperties { get; }
        internal abstract ICollection<MethodWrapper> RawMethods { get; }

        /// <summary>
        /// All properties for the object
        /// </summary>
        public ICollection<PropertyViewModel> Properties
        {
            get
            {
                if (_Properties == null)
                {
                    _Properties = new List<PropertyViewModel>();
                    int count = 1;
                    foreach (var pw in RawProperties)
                    {
                        if (_Properties.Any(f => f.Name == pw.Name))
                        {
                            // This is a duplicate. Keep the one that isn't virtual
                            if (!pw.IsVirtual)
                            {
                                _Properties.Remove(_Properties.First(f => f.Name == pw.Name));
                                var prop = new PropertyViewModel(pw, this, count);
                                _Properties.Add(prop);
                            }
                        }
                        else
                        {
                            var prop = new PropertyViewModel(pw, this, count);
                            _Properties.Add(prop);
                        }
                        count++;
                    }

                }
                return _Properties;
            }
        }

        /// <summary>
        /// All the methods for the Class
        /// </summary>
        public ICollection<MethodViewModel> Methods
        {
            get
            {
                if (_Methods != null) return _Methods;

                var exclude = new[] {
                    nameof(Data.IIncludable<object>.Include),
                    nameof(Data.IIncludeExternal<object>.IncludeExternal),
                    nameof(object.ToString),
                    nameof(object.Equals),
                    nameof(object.GetHashCode),
                    nameof(object.GetType),
                };

                return _Methods = RawMethods
                    .Where(m => !exclude.Contains(m.Name))
                    .Where(m => !IsDto || (m.Name != nameof(Interfaces.IClassDto<object, object>.Update) && m.Name != nameof(Interfaces.IClassDto<object, object>.CreateInstance)))
                    .Select(m => new MethodViewModel(m, this))
                    .ToList();
            }
        }


        /// <summary>
        /// Returns a property matching the name if it exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PropertyViewModel PropertyByName(string key)
        {
            return Properties.FirstOrDefault(f => string.Equals(f.Name, key, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns a method matching the name if it exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public MethodViewModel MethodByName(string key)
        {
            return Methods.FirstOrDefault(f => string.Equals(f.Name, key, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns a property matching the name if it exists.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public PropertyViewModel PropertyBySelector<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            PropertyInfo propInfo = propertySelector.GetExpressedProperty();
            if (propInfo == null) throw new ArgumentException("Could not find property");
            return PropertyByName(propInfo.Name);
        }

        #endregion


        #region Searching/Sorting

        public string DefaultOrderByClause(string prependText = "")
        {
            var defaultOrderBy = DefaultOrderBy.ToList();
            if (defaultOrderBy.Any())
            {
                var orderByClauseList = new List<string>();
                foreach (var orderInfo in defaultOrderBy)
                {
                    if (orderInfo.OrderByDirection == DefaultOrderByAttribute.OrderByDirections.Ascending)
                    {
                        orderByClauseList.Add($"{prependText}{orderInfo.FieldName} ASC");
                    }
                    else
                    {
                        orderByClauseList.Add($"{prependText}{orderInfo.FieldName} DESC");
                    }
                }
                return string.Join(",", orderByClauseList);
            }
            return null;

        }

        /// <summary>
        /// Gets a sorted list of the default order by attributes for the class.
        /// </summary>
        public IEnumerable<OrderByInformation> DefaultOrderBy
        {
            get
            {
                var result = new List<OrderByInformation>();
                foreach (var prop in Properties)
                {
                    var orderInfo = prop.DefaultOrderBy;
                    if (orderInfo != null)
                    {
                        result.Add(orderInfo);
                    }
                }
                // Nothing found, order by ListText and then ID.
                if (!result.Any())
                {
                    var nameProp = PropertyByName("Name");
                    if (nameProp != null && !nameProp.HasNotMapped)
                    {
                        result.Add(new OrderByInformation()
                        {
                            FieldName = "Name",
                            OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Ascending,
                            FieldOrder = 1
                        });
                    }
                    else if (PrimaryKey != null)
                    {
                        result.Add(new OrderByInformation()
                        {
                            FieldName = PrimaryKey.Name,
                            OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Ascending,
                            FieldOrder = 1
                        });
                    }
                }
                return result.OrderBy(f => f.FieldOrder);
            }
        }

        /// <summary>
        /// Gets a list of properties that should be searched for text.
        /// It will first look for any properties that are decorated with the Search attribute.
        /// If it doesn't find those, it will look for a property called Name or {Class}Name.
        /// If none of those are found, it will result in returning the property that is the primary key for the object
        /// </summary>
        public IEnumerable<SearchableProperty> SearchProperties(string rootModelName = null, int depth = 0, int maxDepth = 2)
        {
            // Only go down three levels.
            if (depth == 3) yield break;

            var searchProperties = Properties.Where(f => f.IsSearchable(rootModelName)).ToList();
            if (searchProperties.Any())
            {
                // Process these items to make sure we have things we can search on.
                foreach (var property in searchProperties)
                {
                    // Get all the child items
                    foreach (var searchProperty in property.SearchProperties(rootModelName, depth, maxDepth))
                    {
                        yield return searchProperty;
                    }
                }
                yield break;
            }

            var prop = PropertyByName("Name");
            if (prop != null && !prop.HasNotMapped)
            {
                yield return new SearchableValueProperty(prop);
                yield break;
            }

            prop = Properties.FirstOrDefault(p => string.Equals(p.Name, $"{p.Parent.Name}Name", StringComparison.InvariantCultureIgnoreCase) && !p.HasNotMapped);
            if (prop != null && !prop.HasNotMapped)
            {
                yield return new SearchableValueProperty(prop);
                yield break;
            }

            prop = Properties.FirstOrDefault(f => f.IsPrimaryKey);
            if (prop != null)
            {
                yield return new SearchableValueProperty(prop);
                yield break;
            }
        }

        #endregion



        /// <summary>
        /// Returns the property ID field.
        /// </summary>
        public PropertyViewModel PrimaryKey => Properties.FirstOrDefault(f => f.IsPrimaryKey);

        /// <summary>
        /// Use the ListText Attribute first, then Name and then ID.
        /// </summary>
        public PropertyViewModel ListTextProperty =>
            Properties.FirstOrDefault(f => f.IsListText) ??
            Properties.FirstOrDefault(f => f.Name == "Name") ??
            PrimaryKey;

        public string ApiUrl => ApiName;


        public bool IsOneToOne => PrimaryKey?.IsForeignKey ?? false;

        /// <summary>
        /// Returns true if this class has a partial typescript file.
        /// </summary>
        public bool HasTypeScriptPartial => HasAttribute<TypeScriptPartialAttribute>();

        public bool WillCreateViewController =>
            this.GetAttributeValue<CreateControllerAttribute, bool>(a => a.WillCreateView) ?? true;

        public bool WillCreateApiController =>
            this.GetAttributeValue<CreateControllerAttribute, bool>(a => a.WillCreateApi) ?? true;

        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName => Regex.Replace(Name, "[A-Z]", " $0").Trim();

        public string TableName =>
            "dbo." + (this.GetAttributeValue<TableAttribute>(a => a.Name) ?? ContextPropertyName ?? Name);

        public string ContextPropertyName { get; set; }

        public bool OnContext { get; set; }

        /// <summary>
        /// Has a DbSet property in the Context.
        /// </summary>
        public bool HasDbSet { get; internal set; }

        private ClassSecurityInfo _securityInfo;
        public ClassSecurityInfo SecurityInfo => _securityInfo ?? (_securityInfo = new ClassSecurityInfo(
            new SecurityPermission(GetSecurityAttribute<ReadAttribute>()),
            new SecurityPermission(GetSecurityAttribute<EditAttribute>()),
            new SecurityPermission(GetSecurityAttribute<DeleteAttribute>()),
            new SecurityPermission(GetSecurityAttribute<CreateAttribute>())
        ));

        public string DtoIncludesAsCS()
        {
            var includeList = Properties
                                .Where(p => p.HasDtoIncludes)
                                .SelectMany(p => p.DtoIncludes)
                                .Distinct()
                                .Select(include => $"bool include{include} = includes == \"{include}\";")
                                .ToList();

            return string.Join($"{Environment.NewLine}\t\t\t", includeList);
        }

        public string DtoExcludesAsCS()
        {
            var excludeList = Properties
                                .Where(p => p.HasDtoExcludes)
                                .SelectMany(p => p.DtoExcludes)
                                .Distinct()
                                .Select(exclude => $"bool exclude{exclude} = includes == \"{exclude}\";")
                                .ToList();

            return string.Join($"{Environment.NewLine}\t\t\t", excludeList);
        }

        public string PropertyRolesAsCS()
        {
            var allPropertyRoles = Properties
                .SelectMany(p => p.SecurityInfo.EditRolesList.Union(p.SecurityInfo.ReadRolesList))
                .Distinct()
                .Select(role => $"bool is{role} = context.IsInRoleCached(\"{role}\");")
                .ToList();
            
            return string.Join($"{Environment.NewLine}\t\t\t", allPropertyRoles);
        }


        public abstract object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;
        protected virtual AttributeWrapper GetSecurityAttribute<TAttribute>() where TAttribute : SecurityAttribute
        {
            throw new NotImplementedException();
        }

        public override string ToString() => $"{FullName}";
    }
}