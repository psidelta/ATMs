using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.OrTools.LinearSolver;
using LINQtoCSV;
using Microsoft.Extensions.Configuration;


namespace FinaATM
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int alpha;
            int beta;
            int delta;
            int gamma;
            double epsilon_1;
            double epsilon_2;
            double epsilon_3;
            double epsilon_4;
            double ageFunctionMean;
            double ageFunctionSlope;
            int minStayTrxCnt;


            Console.WriteLine("Hi! I'm just booting up the configuration, reading distance data etc...");
            //Configuration stuff
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var inputFileName = Configuration.GetSection("inputFileName").Value;
            var outputFileName = Configuration.GetSection("outputFileName").Value;
            int.TryParse(Configuration.GetSection("alpha").Value, out alpha);
            int.TryParse(Configuration.GetSection("beta").Value, out beta);
            int.TryParse(Configuration.GetSection("delta").Value, out delta);
            int.TryParse(Configuration.GetSection("gamma").Value, out gamma);
            double.TryParse(Configuration.GetSection("epsilon_1").Value, out epsilon_1);
            double.TryParse(Configuration.GetSection("epsilon_2").Value, out epsilon_2);
            double.TryParse(Configuration.GetSection("epsilon_3").Value, out epsilon_3);
            double.TryParse(Configuration.GetSection("epsilon_4").Value, out epsilon_4);
            double.TryParse(Configuration.GetSection("ageFunctionMean").Value, out ageFunctionMean);
            double.TryParse(Configuration.GetSection("ageFunctionSlope").Value, out ageFunctionSlope);
            int.TryParse(Configuration.GetSection("minStayTrxCnt").Value, out minStayTrxCnt);
           
           
            var includedDesignators = Configuration.GetSection("includeDesignators").Get<string[]>();
            var includedPostalCodes = Configuration.GetSection("includePostalCodes").Get<string[]>();
            var includedCities = Configuration.GetSection("includeCities").Get<string[]>();
            var excludedDesignators = Configuration.GetSection("excludeDesignators").Get<string[]>();

            Dictionary<long, double> costDictionary;
            Dictionary<long, double> trxDictionary;
            var blockedATMs = new long[] { };
            Dictionary<long, Address> addressDictionary;
            Dictionary<long, int> ageDictionary;
            Dictionary<long, int> typeDictionary;
            Dictionary<long, double> certifiabilityDictionary;
            Dictionary<long, AdditionalData> additionalData;
            //Get csv data
            var csvReader = new CsvReader("in/" + inputFileName);
            csvReader.ReadCsvFile(out costDictionary, out trxDictionary, out addressDictionary, out blockedATMs,
                out ageDictionary, out typeDictionary, out certifiabilityDictionary, out additionalData,
                includedDesignators, includedPostalCodes, includedCities, excludedDesignators);

            List<DistanceData> distanceMatrix;

            var GoogleDM = new FinaGoogleApi();
            GoogleDM.GetGoogleDistanceMatrix(out distanceMatrix, addressDictionary);

            var geoCodes = new List<GeoCodedData>();
            GoogleDM.GetGeoCoding(out geoCodes, addressDictionary);

            var dataCoding = new Dictionary<long, long>();

            var newAtmIdx = 0;
            var ReverseDataCoding = new Dictionary<long, long>();

            //switch to optimization data model
            foreach (var item in addressDictionary.OrderBy(x => x.Key))
            {
                dataCoding.Add(newAtmIdx, item.Key);
                ReverseDataCoding.Add(item.Key, newAtmIdx);
                newAtmIdx += 1;
            }


            var costArray = (from item in costDictionary
                orderby item.Key ascending
                select item.Value).ToArray();
            var trxArray = (from item in trxDictionary
                orderby item.Key ascending
                select item.Value).ToArray();
            var ageArray = (from item in ageDictionary
                orderby item.Key ascending
                select item.Value).ToArray();
            var typeArray = (from item in typeDictionary
                orderby item.Key ascending
                select item.Value).ToArray();
            var certifiabilityArray = (from item in certifiabilityDictionary
                orderby item.Key ascending
                select item.Value).ToArray();

            var blockedAtmArray = dataCoding.Where(x => blockedATMs.Contains(x.Value)).Select(x => x.Key).ToArray();


            var distanceMatrixArray = new double[newAtmIdx, newAtmIdx];

            foreach (var item in distanceMatrix)
                distanceMatrixArray[ReverseDataCoding[item.IdFrom], ReverseDataCoding[item.IdTo]] = item.Distance;


            var data = new DataModel(alpha, beta, delta, gamma, epsilon_1, epsilon_2, epsilon_3, epsilon_4,
                ageFunctionMean,
                ageFunctionSlope, costArray, trxArray, distanceMatrixArray,
                blockedATMs, ageArray, typeArray, certifiabilityArray);

            // Create the linear solver with the SCIP backend.
            var solver = Solver.CreateSolver("SCIP");

            var x = new Variable[data.ATMCnt];
            for (var j = 0; j < data.ATMCnt; j++) x[j] = solver.MakeIntVar(0.0, 1, $"x_{dataCoding[j]}");
            Console.WriteLine("Number of variables = " + solver.NumVariables());


            // Add blocked ATM constraints 
            for (var i = 0; i < data.BlockedAtms.Length; i++)
            {
                var constraint = solver.MakeConstraint(1, 1, "");
                constraint.SetCoefficient(x[blockedAtmArray[i]],
                    1);
            }

            //Stay devices with trx cnt greater than or equal to minStayTrxCnt constraints

            for (var index = 0; index < data.TrxCnt.Length; index++)
                if (data.TrxCnt[index] >= minStayTrxCnt)
                {
                    var constraint = solver.MakeConstraint(1, 1, $"ys_{index}");
                    constraint.SetCoefficient(x[index], 1);
                }


            // Add Max Distance Constraints for each ATM

            for (var i = 0; i < data.ATMCnt; i++)
            {
                var constraint = solver.MakeConstraint(0.1, double.PositiveInfinity, $"y_{i}");


                for (var j = 0; j < data.ATMCnt; j++)
                {
                    if (i == j) continue;
                    constraint.SetCoefficient(x[j], data.IndicatorAlpha(i, j) * (data.TrxCnt[j] > gamma ? 0:1)); //TODO: verify this is ok trx cnt greater than is not counted and seems to be working
                }

                constraint.SetCoefficient(x[i], 1);
            }


            Console.WriteLine("Number of constraints = " + solver.NumConstraints());


            // define objective 

            var objective = solver.Objective();
            for (var j = 0; j < data.ATMCnt; ++j) objective.SetCoefficient(x[j], data.GoalCoefficient(j));
            objective.SetMinimization();

            var resultStatus = solver.Solve();


            if (resultStatus != Solver.ResultStatus.OPTIMAL)
            {
                Console.WriteLine("The problem does not have an optimal solution!");
                return;
            }


            //iterativni postupak za max trx cnt

            var isNotOK = true;
            var iterationIdx = 1;


            while (isNotOK)
            {
                var solutionVector = new double[newAtmIdx];

                Console.WriteLine($"Iteration: {iterationIdx}");
                iterationIdx += 1;
                for (var i = 0; i < data.ATMCnt; i++) solutionVector[i] = x[i].SolutionValue();
                ;


                var newTrxCntList = data.GetNewTrxCntList(solutionVector);

                Console.WriteLine($"ATMs Over the Trx Limit Cnt: {newTrxCntList.Count(trx => trx > data.Gamma)}");


                var offendingIndexList = newTrxCntList.ToList();
                var existingTrxCnt = trxArray.ToList();


                var li = offendingIndexList.Zip(existingTrxCnt, (x, y) => new Tuple<double, double>(x, y))
                    .ToList();


                var offendingIndex = li.FindIndex(zeta =>
                {
                    var (x, y) = zeta;

                    return x > y && x > data.Gamma;
                });


                if (offendingIndex == -1)
                {
                    Console.WriteLine("Solved.");
                    isNotOK = false;
                    break;
                }

                Console.WriteLine($"Dealing with atm {offendingIndex} having {newTrxCntList[offendingIndex]} Trx");

                var constraint = solver.MakeConstraint(0.1, double.PositiveInfinity, $"y_y{iterationIdx}");

                for (int i = 0; i < trxArray.Length; i++)
                {
                    if (data.IndicatorAlpha(i, offendingIndex) == 1 && solutionVector[i] < 1)
                    {
                        constraint.SetCoefficient(x[i], 1);
                    }
                }

             
                resultStatus = solver.Solve();

                if (resultStatus != Solver.ResultStatus.OPTIMAL)
                {
                    Console.WriteLine("The problem does not have an optimal solution!");
                    return;
                }
            }

            Console.WriteLine("Solution subject to previous notes:");
            Console.WriteLine("Optimal objective value = " + solver.Objective().Value());

            Console.WriteLine("\nAdvanced usage:");
            Console.WriteLine("Problem solved in " + solver.WallTime() + " milliseconds");
            Console.WriteLine("Problem solved in " + iterationIdx + " iterations");


            Console.WriteLine("PREPARING OUTPUT DATA....");
            var atmOutputList = new List<AtmOutput>();

            var solutionVector2 = new double[newAtmIdx]; 


            for (var i = 0; i < data.ATMCnt; i++) solutionVector2[i] = x[i].SolutionValue();

            var testLista2 = data.GetNewTrxCntList(solutionVector2);
            for (var j = 0; j < data.ATMCnt; ++j)
                atmOutputList.Add(new AtmOutput()
                {
                    Id = dataCoding[j],
                    Address = addressDictionary[dataCoding[j]].Street,
                    PostalCode = addressDictionary[dataCoding[j]].PostalCode,
                    City = addressDictionary[dataCoding[j]].City,
                    IsActive = solutionVector2[j],
                    NewTrxCnt = testLista2[j],
                    TrxCnt = data.TrxCnt[j],
                    AtmAge = data.AgeArray[j],
                    AtmType = data.TypeArray[j],
                    IsBlocked = blockedAtmArray.Contains(j) ? 1 : 0,
                    AgeScore = data.ScaledAgeCoeffs[j],
                    ProfitScore = data.ScaledProfitCoeffs[j],
                    TypeScore = data.TypeArray[j],
                    CertScore = data.CertArray[j],
                    Type = additionalData[dataCoding[j]].Type,
                    BankDesignator = additionalData[dataCoding[j]].Designator,
                    Latitude = geoCodes.FirstOrDefault(x=> x.Id == dataCoding[j]).Latitude,
                    Longitude = geoCodes.FirstOrDefault(x => x.Id == dataCoding[j]).Longitude,
                });


            var solutionDate = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() +
                               DateTime.Now.Day.ToString() +
                               DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() +
                               DateTime.Now.Second.ToString();
            var writer = new CsvWriter(atmOutputList,
                solutionDate +
                "_" + outputFileName);

            writer.WriteResults();


            //write optimization results
            var initialAtmCnt = trxDictionary.Count;
            var finalAtmCnt = solutionVector2.Sum();
            var initialCost = costDictionary.Sum(x => x.Value);
            var finalCost = 0.0;

            for (var i = 0; i < costDictionary.Count; i++)
                finalCost += costArray[i] * solutionVector2[i];

            var costImprovement = Math.Round((finalCost - initialCost) / initialCost * 100, 2);
            var countImprovement = Math.Round((double)(finalAtmCnt - initialAtmCnt) / (double)initialAtmCnt * 100, 2);


            string[] lines =
            {
                "Initial Atm Count:\t" + initialAtmCnt, "Final Atm Count:\t" + finalAtmCnt,
                "Initial Total Cost:\t" + initialCost, "Final Total Cost:\t" + finalCost,
                "Cost Improvement:\t" + costImprovement + "%",
                "Count Improvement:\t" + countImprovement + "%",
                "Model run performed w/params:",
                "alpha:\t" + alpha,
                "beta:\t" + beta,
                "gamma:\t" + gamma,
                "delta:\t" + delta,
                "epsilon_1:\t" + epsilon_1,
                "epsilon_2:\t" + epsilon_2,
                "epsilon_3:\t" + epsilon_3,
                "epsilon_4:\t" + epsilon_4,
                "minStayTrxCnt:\t" + minStayTrxCnt,
                "ageFunctionMean:\t" + ageFunctionMean,
                "ageFunctionSlope:\t" + ageFunctionSlope
            };

            File.WriteAllLinesAsync("output/" + solutionDate + "_" + "Diagnostics.txt", lines);
        }
    }
}