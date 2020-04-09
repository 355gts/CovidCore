namespace Covid.Common.Mapper
{
    public interface ITypeMapper<in TFrom, TTo> where TFrom : class where TTo : class
    {
        TTo Map(TFrom fromObject, TTo toObject = null);
    }
}
