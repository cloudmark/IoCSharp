using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IoCSharp.Exceptions;

namespace IoCSharp
{
    public class Container
    {
        readonly Dictionary<Type, Configuration>  _map = new Dictionary<Type, Configuration>();
        readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        public enum ConstructorResolutionStrategy
        {
            ConstructorWithLargestParameterList,
            ConstructorWithSmallestParameterList 
        }
        public class ContainerBuilder
        {
            private readonly Container _container;
            private readonly Type _sourceType;

            public ContainerBuilder(Container container, Type sourceType)
            {
                _container = container;
                _sourceType = sourceType; 
            }

            public Configuration Use<TDest>()
            {
                return Use(typeof(TDest));
            }

            public Configuration Use(Type destinationType)
            {
                Configuration configuration = new Configuration(_sourceType, destinationType);
                
                _container._map.Add(_sourceType, configuration);
                return configuration; 
            }
        }

        public class Configuration
        {
            public bool Singleton { get; private set; }
            public Type SourceType { get; private set; }
            public Type DestinationType { get; private set; }
            public ConstructorResolutionStrategy ConstructionResolutionStrategy { get; private set; }

            public Configuration(Type sourceType, Type destinationType, bool singleton = true, ConstructorResolutionStrategy constructorResolutionStrategy = ConstructorResolutionStrategy.ConstructorWithLargestParameterList)
            {
                Singleton = singleton;
                SourceType = sourceType;
                DestinationType = destinationType;
                ConstructionResolutionStrategy = constructorResolutionStrategy;
            }


            public Configuration IsSingleton()
            {
                Singleton = true;
                return this; 
            }

            public Configuration IsPrototype()
            {
                Singleton = false;
                return this; 
            }


            public Configuration WithConstructorStrategy(ConstructorResolutionStrategy constructorResolutionStrategytrategy)
            {
                ConstructionResolutionStrategy = constructorResolutionStrategytrategy;
                return this; 
            }
        }


        public ContainerBuilder For<TSource>()
        {
            return For(typeof(TSource)); 
        }

        public ContainerBuilder For(Type sourceType)
        {
            return new ContainerBuilder(this, sourceType); 
        }




        public T1 Resolve<T1>()
        {
            return (T1) Resolve(typeof(T1));
        }

        public object Resolve(Type type)
        {
            if (_map.ContainsKey(type))
            {
                Configuration configuration = _map[type];
                Type destinationType = configuration.DestinationType;
                return CreateObject(configuration, destinationType);
            }
            else if (type.IsGenericType && _map.ContainsKey(type.GetGenericTypeDefinition()))
            {
                Configuration configuration = _map[type.GetGenericTypeDefinition()];
                Type closedType = configuration.DestinationType.MakeGenericType(type.GenericTypeArguments);
                return CreateObject(configuration, closedType);
            }
            else if (!type.IsAbstract)
            {
                // We always assume prototype by default and longest parameter list.  
                return CreateObject(type, ConstructorResolutionStrategy.ConstructorWithLargestParameterList);
            }
            throw new IoCSharpException(String.Format("Type {0} is not configured", type.FullName));
        }

        private object CreateObject(Configuration configuration, Type closedType)
        {
            if (!configuration.Singleton) return CreateObject(closedType, configuration.ConstructionResolutionStrategy);

            if (_cache.ContainsKey(closedType))
            {
                return _cache[closedType];
            }
            var obj = CreateObject(closedType, configuration.ConstructionResolutionStrategy);
            _cache.Add(closedType, obj);
            return obj;
        }

        private object CreateObject(Type closedType, ConstructorResolutionStrategy constructorResolutionStrategy)
        {
            IEnumerable<ConstructorInfo> query = closedType.GetConstructors();
            switch (constructorResolutionStrategy)
            {
                case ConstructorResolutionStrategy.ConstructorWithLargestParameterList:
                    query = query.OrderByDescending(c => c.GetParameters().Length);
                    break; 
                case ConstructorResolutionStrategy.ConstructorWithSmallestParameterList:
                    query = query.OrderBy(c => c.GetParameters().Length);
                    break;
            }
                
            var parameters = query.First()
                .GetParameters()
                .Select(p => Resolve(p.ParameterType))
                .ToArray();

            return  Activator.CreateInstance(closedType, parameters);
        }
    }
}
