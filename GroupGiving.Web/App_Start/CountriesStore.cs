using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Com.StellmanGreene.CSVReader;
using GroupGiving.Core.Services;

[assembly: WebActivator.PreApplicationStartMethod(typeof(GroupGiving.Web.App_Start.CountriesStore), "LoadCountries")]
namespace GroupGiving.Web.App_Start
{
    public class CountriesStore
    {
        public static List<Country> Countries { get; set; } 
        public static void LoadCountries()
        {
            Countries = new List<Country>();
            using (var filestream= File.OpenRead(HostingEnvironment.MapPath("~/App_Data/countrylist.csv")))
            using (var reader = new StreamReader(filestream))
            {
                CSVReader csv = new CSVReader(reader);
                var table = csv.CreateDataTable(true);
                foreach(DataRow row in table.Rows)
                {
                    Countries.Add(new Country((string)row["Common Name"]));
                }
            }
        }
    }
}