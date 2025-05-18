using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;

namespace FinaATM
{
    public class AtmOutput
    {
        [CsvColumn(Name = "id", FieldIndex = 1)]
        public long Id { get; set; }

        [CsvColumn(FieldIndex = 2)] public string Address { get; set; }
        [CsvColumn(FieldIndex = 3)] public string City { get; set; }
        [CsvColumn(FieldIndex = 4)] public string PostalCode { get; set; }
        [CsvColumn(FieldIndex = 5)] public double IsActive { get; set; }

        [CsvColumn(FieldIndex = 6)] public double TrxCnt { get; set; }
        [CsvColumn(FieldIndex = 7)] public double NewTrxCnt { get; set; }
        [CsvColumn(FieldIndex = 8)] public int IsBlocked { get; set; }
        [CsvColumn(FieldIndex = 9)] public int AtmType { get; set; }
        [CsvColumn(FieldIndex = 10)] public int AtmAge { get; set; }
        [CsvColumn(FieldIndex = 11)] public double ProfitScore { get; set; }
        [CsvColumn(FieldIndex = 12)] public double AgeScore { get; set; }
        [CsvColumn(FieldIndex = 13)] public double TypeScore { get; set; }
        [CsvColumn(FieldIndex = 14)] public double CertScore { get; set; }
        [CsvColumn(FieldIndex = 15)] public string Type { get; set; }
        [CsvColumn(FieldIndex = 16)] public string BankDesignator { get; set; }
        [CsvColumn(FieldIndex = 17)] public double Latitude { get; set; }
        [CsvColumn(FieldIndex = 18)] public double Longitude { get; set; }

    }
}