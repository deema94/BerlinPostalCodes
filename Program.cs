using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BerlinPostalCodes
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // get the file
            await using var fileStream = File.OpenRead("berlin-latest.osm.pbf");

            // create source stream.
            var source = new PBFOsmStreamSource(fileStream);

            // let's use linq to leave only objects with Type = Node and contains the key "postal_code"
            //var filtered = from osmGeo in source
            //               where osmGeo.Tags.ContainsKey("postal_code") && osmGeo.Type == OsmSharp.OsmGeoType.Node 
            //               select osmGeo;

            // let's use linq to leave only objects with "boundary" = "postal_code"
            var filtered = from osmGeo in source
                           where osmGeo.Tags.Contains("boundary", "postal_code") 
                           select osmGeo;

            List<string> postalCodes = new List<string>();

            foreach (var osmGeo in filtered)
            {
                int code = 0;
                if (int.TryParse(osmGeo.Tags.GetValue("postal_code"), out code))
                {
                    if (code >= 10115 && code <= 14199)
                        postalCodes.Add(osmGeo.Tags.GetValue("postal_code"));
                }

                Console.WriteLine(osmGeo.Tags.GetValue("postal_code"));
            }

            postalCodes.Sort();

            postalCodes.Distinct().ToList();

            string combinedString = string.Join("\n", postalCodes);

            File.WriteAllText("output.txt", combinedString);
        }
    }
}
