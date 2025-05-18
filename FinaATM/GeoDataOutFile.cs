using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;

namespace FinaATM
{
    public class GeoDataOutFile
    {
        [CsvColumn(FieldIndex = 1)] public long Id { get; set; }
        [CsvColumn(FieldIndex = 2)] public double Latitude { get; set; }
        [CsvColumn(FieldIndex = 3)] public double Longitude { get; set; }
    }
}
