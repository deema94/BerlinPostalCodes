using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BerlinPostalCodes
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /// <summary>
            /// get the file, download the file if it doesn't exist yet.
            /// <summary>
            await ToFile("http://download.geofabrik.de/europe/germany/berlin-latest.osm.pbf", "berlin-latest.osm.pbf");
            await using var fileStream = File.OpenRead("berlin-latest.osm.pbf");

            /// <summary>
            /// create source stream.
            /// <summary>
            var source = new PBFOsmStreamSource(fileStream);

            /// <summary>
            /// first tag "boundary=postal_code".
            /// based on taginfo's statistics this tag is useful only in Germany because
            /// the geographical distribution of this key here is very high.
            /// "boundary=postal_code" will give us 192 postal codes in Berlin.
            /// use linq to leave only objects with Type = Way and contains the tag "postal_code".
            /// <summary>
            //var filtered = from osmGeo in source
            //               where osmGeo.Tags.ContainsKey("postal_code") && osmGeo.Type == OsmSharp.OsmGeoType.Way
            //               select osmGeo;

            /// <summary>
            /// second key "addr:postcode"
            /// in Karlsruhe Schema they use this key
            /// "addr:postcode=*" will give us 193 postal codes in Berlin
            /// use linq to leave only objects with Type = Node and contains the key "addr:postcode"
            /// <summary>
            var filtered = from osmGeo in source
                           where osmGeo.Tags.ContainsKey("addr:postcode") && osmGeo.Type == OsmSharp.OsmGeoType.Node
                           select osmGeo;

            /// <summary>
            /// add postal codes to a list then sort it and finally remove duplicates
            /// <summary>
            List<string> postalCodes = new List<string>();
            foreach (var osmGeo in filtered)
            {
                // show details on console
                Console.WriteLine(osmGeo.Tags.GetValue("addr:street")
                    + ", " + osmGeo.Tags.GetValue("addr:city")
                    + ", " + osmGeo.Tags.GetValue("addr:postcode"));

                string postalCode = osmGeo.Tags.GetValue("addr:postcode");
                int intPostalCode = 0;
                if (int.TryParse(postalCode, out intPostalCode))
                {
                    // berlin postal codes are from 10115 to 14199
                    if (intPostalCode >= 10115 && intPostalCode <= 14199)
                        postalCodes.Add(postalCode);
                }
            }

            postalCodes.Sort();
            List<string> distinctPostalCodes = postalCodes.Distinct().ToList();
            string combinedString = string.Join("\n", distinctPostalCodes);
            File.WriteAllText("output.txt", combinedString);
        }

        /// <summary>
        /// Downloads a file if it doesn't exist yet.
        /// <summary>
        public static async Task ToFile(string url, string filename)
        {
            if (!File.Exists(filename))
            {
                var client = new HttpClient();
                await using var stream = await client.GetStreamAsync(url);
                await using var outputStream = File.OpenWrite(filename);
                await stream.CopyToAsync(outputStream);
            }
        }
    }
}
