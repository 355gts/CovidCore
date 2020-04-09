using System.Collections.Generic;

namespace Covid.Common.Mapper
{
    public interface IMapper
    {
        TTo Map<TFrom, TTo>(TFrom fromObject, TTo toObject = null) where TFrom : class where TTo : class;

        IEnumerable<TTo> MapEnumerable<TFrom, TTo>(IEnumerable<TFrom> fromObjects) where TFrom : class where TTo : class;
    }
}
