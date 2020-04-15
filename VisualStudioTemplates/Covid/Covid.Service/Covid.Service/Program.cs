using System;
using Topshelf;

namespace Covid.$ext_safeprojectname$
{

    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<$ext_safeprojectname$Service>();
                x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(10)));
                x.SetServiceName("$ext_safeprojectname$Service");
                x.StartAutomatically();
            });
        }
    }
}
