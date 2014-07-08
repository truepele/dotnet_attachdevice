using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NvtlApiWrapper;

namespace dotnet_attachdevice
{
    class Program
    {

        private static void Main(string[] args)
        {
            ApiWrapper api = new ApiWrapper();

            if (api.Init())
            {
                
            }
        }

    }
}
