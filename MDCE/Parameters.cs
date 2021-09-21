using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftDynamicsConnectorExporter.Params
{
    public class Parameters
    {
        public static string Username
        {
            get
            {
                return ConfigurationManager.AppSettings["Username"];
            }
        }

        public static string Password
        {
            get
            {
                return ConfigurationManager.AppSettings["Password"];
            }
        }

        public static string Domain
        {
            get
            {
                return ConfigurationManager.AppSettings["Domain"];
            }
        }

        public static string Customer
        {
            get
            {
                return ConfigurationManager.AppSettings["Customer"];
            }
        }
    }
}
