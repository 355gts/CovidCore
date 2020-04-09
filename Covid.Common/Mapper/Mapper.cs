using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Covid.Common.Mapper
{
    public class Mapper : IMapper
    {
        private readonly Dictionary<Type, Dictionary<Type, object>> typeMappers;

        public Mapper(Assembly assemblyWithMappers)
        {
            if (assemblyWithMappers == null)
                throw new ArgumentNullException(nameof(assemblyWithMappers));

            var allTypes = assemblyWithMappers.GetTypes();

            var typeMapperTypes = allTypes.SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && typeof(ITypeMapper<,>).IsAssignableFrom(i.GetGenericTypeDefinition()))
                .Select(i => new
                {
                    MapperType = t,
                    FromType = i.GetGenericArguments().First(),
                    ToType = i.GetGenericArguments().Last(),
                }));

            typeMappers = typeMapperTypes.GroupBy(x => x.FromType)
                .ToDictionary(x => x.Key,
                              x => x.ToDictionary(z => z.ToType,
                                                  z => (dynamic)Activator.CreateInstance(z.MapperType)));
        }

        public TTo Map<TFrom, TTo>(TFrom fromObject, TTo toObject = null) where TFrom : class where TTo : class
        {

            if (fromObject == null)
                return default(TTo);

            var mapper = GetMapper(fromObject, toObject);

            if (mapper == null)
                return default(TTo);

            return mapper.Map(fromObject, toObject);
        }

        public IEnumerable<TTo> MapEnumerable<TFrom, TTo>(IEnumerable<TFrom> fromObjects) where TFrom : class where TTo : class
        {
            if (fromObjects == null || !fromObjects.Any())
                return Enumerable.Empty<TTo>();

            var mapper = GetMapper<TFrom, TTo>(fromObjects.First());

            if (mapper == null)
                return Enumerable.Empty<TTo>();

            var toObjects = new List<TTo>();

            foreach (var fromObject in fromObjects)
            {
                toObjects.Add(mapper.Map(fromObject));
            }

            return toObjects;
        }

        private ITypeMapper<TFrom, TTo> GetMapper<TFrom, TTo>(TFrom fromObject, TTo toObject = null) where TFrom : class where TTo : class
        {
            if (typeMappers.ContainsKey(typeof(TFrom)) && typeMappers[typeof(TFrom)].ContainsKey(typeof(TTo)))
            {
                return (ITypeMapper<TFrom, TTo>)typeMappers[typeof(TFrom)][typeof(TTo)];
            }
            else
            {
                throw new Exception($"No mapper exists from type '{typeof(TFrom).Name.ToString()}' to type '{typeof(TTo).Name.ToString()}'.");
            }
        }
    }
}
