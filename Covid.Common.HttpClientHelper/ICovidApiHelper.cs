using Covid.Common.HttpClientHelper.Services.Interfaces;

namespace Covid.Common.HttpClientHelper
{
    public interface ICovidApiHelper
    {
        IUserService Users { get; }
    }
}
