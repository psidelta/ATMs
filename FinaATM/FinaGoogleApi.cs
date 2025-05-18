using System;
using System.Collections.Generic;
using System.Linq;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using GoogleApi.Entities.Maps.Geocoding;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using LINQtoCSV;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;


namespace FinaATM
{
    public class FinaGoogleApi
    {
        private bool skipDistanceMatrixFetch;
        private string distanceMatrixFileName;
        private string GoogleApiKey;
        private string geocodedLocationsFileName;
        private bool skipGeoCodeFetch;

        public FinaGoogleApi()
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            bool.TryParse(Configuration.GetSection("skipDistanceMatrixFetch").Value, out skipDistanceMatrixFetch);
            distanceMatrixFileName = "in/" + Configuration.GetSection("distanceMatrixFileName").Value;
            geocodedLocationsFileName = "in/" + Configuration.GetSection("geoCodingFileName").Value;
            GoogleApiKey = Configuration.GetSection("googleApiKey").Value;
            bool.TryParse(Configuration.GetSection("skipGeoCodeFetch").Value, out skipGeoCodeFetch);
        }

        public void GetGoogleDistanceMatrix(out List<DistanceData> distanceMatrix,
            Dictionary<long, Address> addressData)
        {
            var request = new DistanceMatrixRequest
            {
                Key = GoogleApiKey
            };
            request.TravelMode = GoogleApi.Entities.Maps.Common.Enums.TravelMode.Walking;

            //get available data
            var inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ';',
                FirstLineHasColumnNames = true
            };
            var cc = new CsvContext();

            var localDistanceMatrix =
                cc.Read<DistanceMatrix>(distanceMatrixFileName, inputFileDescription);
            // Data is now available via variable atmInputs


            //list of data requested by the address list that is available in the stored distance matrix csv file
            var availableData =
                from data in localDistanceMatrix
                where addressData.Any(x => x.Value.AtmId == data.IdFrom) &&
                      addressData.Any(x => x.Value.AtmId == data.IdTo)
                select new DistanceData() { IdFrom = data.IdFrom, IdTo = data.IdTo, Distance = data.Distance };


            distanceMatrix = availableData.ToList();


            //Add system assumed distance data
            foreach (var item in addressData)
            foreach (var item2 in addressData)
                if (item.Value.PostalCode != item2.Value.PostalCode && item.Value.City != item2.Value.City)
                    distanceMatrix.Add(new DistanceData()
                    {
                        IdFrom = item.Value.AtmId,
                        IdTo = item2.Value.AtmId,
                        Distance = 300000,
                        AddressTo = "SYSTEM",
                        AddressFrom = "SYSTEM"
                    });

                else if (item.Value.AtmId == item2.Value.AtmId)
                    distanceMatrix.Add(new DistanceData()
                    {
                        IdFrom = item.Value.AtmId,
                        IdTo = item.Value.AtmId,
                        Distance = 0.0,
                        AddressTo = "SYSTEM",
                        AddressFrom = "SYSTEM"
                    });


            var newDistanceMatrixData = new List<DistanceData>();


            if (!skipDistanceMatrixFetch)
            {
                var requestedData = new List<DistanceData>();

                foreach (var item in addressData)
                foreach (var item2 in addressData)
                    if ((item.Value.PostalCode == item2.Value.PostalCode || item.Value.City == item2.Value.City) && item.Value.AtmId != item2.Value.AtmId)
                        requestedData.Add(new DistanceData() { IdFrom = item.Key, IdTo = item2.Key });


                //remove already available data from the data to be requested from the api
                foreach (var item in availableData)
                    requestedData.RemoveAll(x => x.IdFrom == item.IdFrom && x.IdTo == item.IdTo);

                if (requestedData.Count == 0)
                {
                    Console.WriteLine("All distance data is available.\n");
                    return;
                }


                Console.WriteLine("");
                Console.WriteLine("Starting Google API calls:");
                var noOfRequests = requestedData.Count;
                var countidx = 1;

                foreach (var item in requestedData)
                {
                    Console.Write("\r{0}%                  ", Math.Round((double)countidx * 100 / noOfRequests, 2));
                    countidx += 1;


                    //assuming symmetric distance
                    if (item.IdFrom > item.IdTo) continue;

                    string originString;
                    string destinationString;


                    if (addressData[item.IdFrom].ToString().Any(x => char.IsLetter(x)))
                        originString = addressData[item.IdFrom].Street + " " + addressData[item.IdFrom].PostalCode +
                                       " " +
                                       addressData[item.IdFrom].City + " " + "CROATIA";
                    else
                        originString = addressData[item.IdFrom].Street;

                    if (addressData[item.IdTo].ToString().Any(x => char.IsLetter(x)))
                        destinationString = addressData[item.IdTo].Street + " " + addressData[item.IdTo].PostalCode +
                                            " " +
                                            addressData[item.IdTo].City + " " + "CROATIA";
                    else
                        destinationString = addressData[item.IdTo].Street;


                    var origin = new Location(originString);
                    var destination = new Location(destinationString);


                    var origins = new List<Location>();
                    var destinations = new List<Location>();
                    origins.Add(origin);


                    destinations.Add(destination);


                    request.Origins = origins;
                    request.Destinations = destinations;


                    try
                    {
                        var res = GoogleApi.GoogleMaps.DistanceMatrix.Query(request);
                        var test = res.RawJson;

                        var testomg = JObject.Parse(test);

                        //element/duration/value

                        var rows = testomg.SelectToken("rows");
                        var originAddress = testomg.SelectToken("origin_addresses")[0].ToString();
                        var destinationAddress = testomg.SelectToken("destination_addresses")[0].ToString();
                        for (var i = 0; i < rows.Count(); i++)
                        {
                            var token = rows[i].SelectToken("elements");

                            for (var j = 0; j < token.Count(); j++)
                            {
                                var token2 = token[j].SelectToken("duration.value");
                                double tempDistance;

                                double.TryParse(token2.ToString(), out tempDistance);
                                newDistanceMatrixData.Add(new DistanceData()
                                {
                                    IdFrom = item.IdFrom,
                                    IdTo = item.IdTo,
                                    Distance = tempDistance,
                                    AddressFrom = originAddress,
                                    AddressTo = destinationAddress
                                });

                                //saving some time 
                                //if (item.IdFrom != item.IdTo) implies difference
                                newDistanceMatrixData.Add(new DistanceData()
                                {
                                    IdFrom = item.IdTo,
                                    IdTo = item.IdFrom,
                                    Distance = tempDistance,
                                    AddressFrom = destinationAddress,
                                    AddressTo = originAddress
                                });
                            }
                        }

                        //Console.WriteLine($"got data from: {item.IdFrom} to: {item.IdTo}");
                    }
                    catch (Exception e)
                    {
                        newDistanceMatrixData.Add(new DistanceData()
                        {
                            IdFrom = item.IdFrom,
                            IdTo = item.IdTo,
                            Distance = 1000000,
                            AddressFrom = "ERROR",
                            AddressTo = "ERROR"
                        });
                        //if (item.IdFrom != item.IdTo) then they are different
                        newDistanceMatrixData.Add(new DistanceData()
                        {
                            IdFrom = item.IdTo,
                            IdTo = item.IdFrom,
                            Distance = 1000000,
                            AddressFrom = "ERROR",
                            AddressTo = "ERROR"
                        });
                    }
                }

                Console.WriteLine("Google calls: DONE\n");
                foreach (var item in newDistanceMatrixData) distanceMatrix.Add(item);


                var distanceMatrixOutput = new List<DistanceMatrix>();
                foreach (var item in localDistanceMatrix) distanceMatrixOutput.Add(item);


                foreach (var item in newDistanceMatrixData)
                    distanceMatrixOutput.Add(new DistanceMatrix()
                    {
                        IdFrom = item.IdFrom,
                        IdTo = item.IdTo,
                        AddressFrom = addressData[item.IdFrom].Street,
                        AddressTo = addressData[item.IdTo].Street,
                        CityFrom = addressData[item.IdFrom].City,
                        CityTo = addressData[item.IdTo].City,
                        Distance = item.Distance,
                        PostalCodeFrom = addressData[item.IdFrom].PostalCode,
                        PostalCodeTo = addressData[item.IdTo].PostalCode,
                        GoogleAddressFrom = item.AddressFrom,
                        GoogleAddressTo = item.AddressTo
                    });


                var outputFileDescription = new CsvFileDescription
                {
                    SeparatorChar = ';', 
                    FirstLineHasColumnNames = true, 
                    FileCultureName = "hr-HR" 
                };
                var cc2 = new CsvContext();
                cc2.Write(
                    distanceMatrixOutput,
                    distanceMatrixFileName,
                    outputFileDescription);
            }
        }


        public void GetGeoCoding(out List<GeoCodedData> geoCodedDataList, Dictionary<long, Address> addressData)
        {
            var newGeoCodedDataList = new List<GeoCodedData>();
            geoCodedDataList = new List<GeoCodedData>();

            var requestedData = new List<Address>();

            var inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ';',
                FirstLineHasColumnNames = true
            };
            var cc = new CsvContext();

            var localGeoCodedData =
                cc.Read<GeoDataOutFile>(geocodedLocationsFileName, inputFileDescription).ToList();

            var availableData = localGeoCodedData.Where(x => addressData.ContainsKey(x.Id));

            foreach (var item in availableData)
                geoCodedDataList.Add(new GeoCodedData()
                {
                    Id = item.Id,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude
                });

            foreach (var item in addressData)
                if (!localGeoCodedData.Any(x => x.Id == item.Key))
                    requestedData.Add(item.Value);

            if (requestedData.Count == 0)
            {
                Console.WriteLine("Geocoding Api Fetch Not Needed\n");
                return;
            }

            if (skipGeoCodeFetch)
            {
                Console.WriteLine("Skipping Geocoding Api Fetch\n");
                return;
            }

            Console.WriteLine("Initiating Geocoding Api Fetch\n");
            var request = new AddressGeocodeRequest()
            {
                Key = GoogleApiKey
            };


            foreach (var item in requestedData)
            {

                if (item.Street.Contains('+'))
                {
                    request.Address = item.Street;
                }
                else
                {
                    request.Address = item.Street + " " + item.PostalCode + " " + item.City + " CROATIA";
                }
                

                try
                {
                    var response = GoogleApi.GoogleMaps.AddressGeocode.Query(request);

                    var latitude = response.Results.ToArray()[0].Geometry.Location.Latitude;
                    var longitude = response.Results.ToArray()[0].Geometry.Location.Longitude;

                    newGeoCodedDataList.Add(new GeoCodedData()
                        { Id = item.AtmId, Latitude = latitude, Longitude = longitude });
                }
                catch (Exception e)
                {
                    newGeoCodedDataList.Add(new GeoCodedData() { Id = item.AtmId, Latitude = 0, Longitude = 0 });
                }
            }

            foreach (var item in newGeoCodedDataList)
            {
                localGeoCodedData.Add(new GeoDataOutFile()
                {
                    Id = item.Id,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude
                });

                geoCodedDataList.Add(new GeoCodedData()
                {
                    Id = item.Id,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude
                });
            }


            Console.WriteLine("Finished Geocoding Api Fetch\n");
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ';',
                FirstLineHasColumnNames = true,
                FileCultureName = "hr-HR"
            };

            var cc2 = new CsvContext();
            cc2.Write(
                localGeoCodedData,
                geocodedLocationsFileName,
                outputFileDescription);
        }
    }
}