using Covid.Common.HttpClientHelper.Services;
using Covid.Common.HttpClientHelper.Services.Interfaces;
using System;

namespace Covid.Common.HttpClientHelper
{
    public class CovidApiHelper : ICovidApiHelper
    {
        public IUserService Users { get; }

        public CovidApiHelper(IHttpClientHelper httpClientHelper)
        {
            if (httpClientHelper == null)
                throw new ArgumentNullException(nameof(httpClientHelper));

            Users = new UserService(httpClientHelper);
        }
    }
}
