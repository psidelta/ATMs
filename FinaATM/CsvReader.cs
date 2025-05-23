using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinaATM
{
    public class CsvReader
    {
        private readonly string _inputFileName;

        public CsvReader(string inputFileName)
        {
            _inputFileName = inputFileName;
        }

        public void ReadCsvFile(out Dictionary<long, double> costs, out Dictionary<long, double> trxCnt,
            out Dictionary<long, Address> addressList, out long[] blockedAtms, out Dictionary<long, int> ageList,
            out Dictionary<long, int> typeList, out Dictionary<long, double> certifiabilityList,
            out Dictionary<long, AdditionalData> additionalData,
            string[] includedDesignators, string[] includedPostalCodes, string[] includedCities, string[] excludedDesignators)
        {
            var inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ';',
                FirstLineHasColumnNames = true,
                FileCultureName = "hr-HR",

            };
            var cc = new CsvContext();
            var atmInputs =
                cc.Read<AtmInput>(_inputFileName, inputFileDescription);
            // Data is now available via variable atmInputs.

            var test = atmInputs.ToList<AtmInput>();
            //costs
            var costInput =
                from atm in atmInputs
                where string.IsNullOrEmpty(atm.DeinstallationDate) &&
                      !string.IsNullOrEmpty(atm.Street) &&
                      (string.IsNullOrEmpty(atm.IsShelved) || atm.IsShelved == "NE") &&
                      (string.IsNullOrEmpty(atm.NotInNetworkFlag) || atm.NotInNetworkFlag == "NE") &&
                      (includedDesignators == null || includedDesignators.Contains(atm.BankDesignator)) &&
                      (includedPostalCodes == null || includedPostalCodes.Contains(atm.PostalCode)) &&
                      (includedCities == null || includedCities.Contains(atm.City)) && 
                      (excludedDesignators == null || !excludedDesignators.Contains(atm.BankDesignator))
                select new CostData()
                {
                    AtmId = atm.Id,
                    Cost = atm.RentPrice * 12 +
                           (double.IsNaN(atm.MonthlyAtmRentalPrice) ? 0.0 : atm.MonthlyAtmRentalPrice * 12)
                };
            costs = costInput.Select((s) => new { s.AtmId, s.Cost }).ToDictionary(x => x.AtmId, x => x.Cost);


            //trx counts
            var trxCntInput =
                from atm in atmInputs
                where string.IsNullOrEmpty(atm.DeinstallationDate) &&
                      !string.IsNullOrEmpty(atm.Street) &&
                      (string.IsNullOrEmpty(atm.IsShelved) || atm.IsShelved == "NE") &&
                      (string.IsNullOrEmpty(atm.NotInNetworkFlag) || atm.NotInNetworkFlag == "NE") &&
                      (includedDesignators == null || includedDesignators.Contains(atm.BankDesignator)) &&
                      (includedPostalCodes == null || includedPostalCodes.Contains(atm.PostalCode)) &&
                      (includedCities == null || includedCities.Contains(atm.City)) &&
                      (excludedDesignators == null || !excludedDesignators.Contains(atm.BankDesignator))
                select new TrxData()
                    { AtmId = atm.Id, TrxCnt = atm.YearInTrxCnt2019 + atm.YearOutTrxCnt2019 + atm.YearOtherTrxCnt2019 };

            trxCnt = trxCntInput.Select((s) => new { s.AtmId, s.TrxCnt }).ToDictionary(x => x.AtmId, x => x.TrxCnt);

            //addressList
            var addressInput =
                from atm in atmInputs
                where string.IsNullOrEmpty(atm.DeinstallationDate) &&
                      !string.IsNullOrEmpty(atm.Street) &&
                      (string.IsNullOrEmpty(atm.IsShelved) || atm.IsShelved == "NE") &&
                      (string.IsNullOrEmpty(atm.NotInNetworkFlag) || atm.NotInNetworkFlag == "NE") &&
                      (includedDesignators == null || includedDesignators.Contains(atm.BankDesignator)) &&
                      (includedPostalCodes == null || includedPostalCodes.Contains(atm.PostalCode)) &&
                      (includedCities == null || includedCities.Contains(atm.City)) &&
                      (excludedDesignators == null || !excludedDesignators.Contains(atm.BankDesignator))
                select new Address()
                {
                    AtmId = atm.Id,
                    Street = atm.Street.Trim().ToUpper() + " " +
                             (string.IsNullOrEmpty(atm.HouseNo) ? " " : atm.HouseNo).Trim(),
                    City = atm.City.Trim().ToUpper(), PostalCode = atm.PostalCode.Trim()
                };


            var blockedAtmInput =
                from atm in atmInputs
                where string.IsNullOrEmpty(atm.DeinstallationDate) &&
                      !string.IsNullOrEmpty(atm.Street) &&
                      (string.IsNullOrEmpty(atm.IsShelved) || atm.IsShelved == "NE") &&
                      (string.IsNullOrEmpty(atm.NotInNetworkFlag) || atm.NotInNetworkFlag == "NE") &&
                      (atm.IntraBank == "DA" || atm.In24HrZone == "DA") &&
                      (includedDesignators == null || includedDesignators.Contains(atm.BankDesignator)) &&
                      (includedPostalCodes == null || includedPostalCodes.Contains(atm.PostalCode)) &&
                      (includedCities == null || includedCities.Contains(atm.City)) &&
                      (excludedDesignators == null || !excludedDesignators.Contains(atm.BankDesignator))
                select atm.Id;

            var ageInput =
                from atm in atmInputs
                where string.IsNullOrEmpty(atm.DeinstallationDate) &&
                      !string.IsNullOrEmpty(atm.Street) &&
                      (string.IsNullOrEmpty(atm.IsShelved) || atm.IsShelved == "NE") &&
                      (string.IsNullOrEmpty(atm.NotInNetworkFlag) || atm.NotInNetworkFlag == "NE") &&
                      (includedDesignators == null || includedDesignators.Contains(atm.BankDesignator)) &&
                      (includedPostalCodes == null || includedPostalCodes.Contains(atm.PostalCode)) &&
                      (includedCities == null || includedCities.Contains(atm.City)) &&
                      (excludedDesignators == null || !excludedDesignators.Contains(atm.BankDesignator))
                select new AgeData() { AtmId = atm.Id, Age = DateTime.Now.Year - atm.ManufacturingYear };

            var typeInput = from atm in atmInputs
                where string.IsNullOrEmpty(atm.DeinstallationDate) &&
                      !string.IsNullOrEmpty(atm.Street) &&
                      (string.IsNullOrEmpty(atm.IsShelved) || atm.IsShelved == "NE") &&
                      (string.IsNullOrEmpty(atm.NotInNetworkFlag) || atm.NotInNetworkFlag == "NE") &&
                      (includedDesignators == null || includedDesignators.Contains(atm.BankDesignator)) &&
                      (includedPostalCodes == null || includedPostalCodes.Contains(atm.PostalCode)) &&
                      (includedCities == null || includedCities.Contains(atm.City)) &&
                      (excludedDesignators == null || !excludedDesignators.Contains(atm.BankDesignator))
                            select new TypeData() { AtmId = atm.Id, Type = atm.Type == "ISPLATNI" ? 1 : 0 };


            var certifiabilityInput =
                from atm in atmInputs
                where string.IsNullOrEmpty(atm.DeinstallationDate) &&
                      !string.IsNullOrEmpty(atm.Street) &&
                      (string.IsNullOrEmpty(atm.IsShelved) || atm.IsShelved == "NE") &&
                      (string.IsNullOrEmpty(atm.NotInNetworkFlag) || atm.NotInNetworkFlag == "NE") &&
                      (includedDesignators == null || includedDesignators.Contains(atm.BankDesignator)) &&
                      (includedPostalCodes == null || includedPostalCodes.Contains(atm.PostalCode)) &&
                      (includedCities == null || includedCities.Contains(atm.City)) &&
                      (excludedDesignators == null || !excludedDesignators.Contains(atm.BankDesignator))
                select new CertData() { AtmId = atm.Id, CertifiabilityIndex = atm.CertifiabilityScore };

            var addData =
                from atm in atmInputs
                where string.IsNullOrEmpty(atm.DeinstallationDate) &&
                      !string.IsNullOrEmpty(atm.Street) &&
                      (string.IsNullOrEmpty(atm.IsShelved) || atm.IsShelved == "NE") &&
                      (string.IsNullOrEmpty(atm.NotInNetworkFlag) || atm.NotInNetworkFlag == "NE") &&
                      (includedDesignators == null || includedDesignators.Contains(atm.BankDesignator)) &&
                      (includedPostalCodes == null || includedPostalCodes.Contains(atm.PostalCode)) &&
                      (includedCities == null || includedCities.Contains(atm.City)) &&
                      (excludedDesignators == null || !excludedDesignators.Contains(atm.BankDesignator))
                select new AdditionalData() { AtmId = atm.Id, Designator = atm.BankDesignator, Type = atm.Type };


            blockedAtms = blockedAtmInput.ToArray();


            addressList = addressInput.Select(s => new { s.AtmId, s }).ToDictionary(x => x.AtmId, x => x.s);
            ageList = ageInput.Select(s => new { s.AtmId, s.Age }).ToDictionary(x => x.AtmId, x => x.Age);
            typeList = typeInput.Select(s => new { s.AtmId, s.Type }).ToDictionary(x => x.AtmId, x => x.Type);
            certifiabilityList = certifiabilityInput.Select(s => new { s.AtmId, s.CertifiabilityIndex })
                .ToDictionary(x => x.AtmId, x => x.CertifiabilityIndex);
            additionalData = addData.Select(s => new { s.AtmId, s }).ToDictionary(x => x.AtmId, x => x.s);
        }
    }
}
