using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;

namespace FinaATM
{
    public class DistanceMatrix
    {
        [CsvColumn(Name = "IdFrom", FieldIndex = 1)]
        public long IdFrom { get; set; }

        [CsvColumn(Name = "IdTo", FieldIndex = 2)]
        public long IdTo { get; set; }

        [CsvColumn(FieldIndex = 3)] public string AddressFrom { get; set; }
        [CsvColumn(FieldIndex = 4)] public string CityFrom { get; set; }
        [CsvColumn(FieldIndex = 5)] public string PostalCodeFrom { get; set; }
        [CsvColumn(FieldIndex = 6)] public string GoogleAddressFrom { get; set; }

        [CsvColumn(FieldIndex = 7)] public string AddressTo { get; set; }
        [CsvColumn(FieldIndex = 8)] public string CityTo { get; set; }
        [CsvColumn(FieldIndex = 9)] public string PostalCodeTo { get; set; }
        [CsvColumn(FieldIndex = 10)] public string GoogleAddressTo { get; set; }

        [CsvColumn(FieldIndex = 11)] public double Distance { get; set; }
    }
}