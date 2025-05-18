using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;

namespace FinaATM
{
    public class CsvWriter
    {
        private readonly List<AtmOutput> _atmOutput;
        private readonly string _outputFileName;


        public CsvWriter(List<AtmOutput> atmOutput, string outputFileName)
        {
            _atmOutput = atmOutput;
            _outputFileName = outputFileName;
        }
        

        public void WriteResults()
        {
            CsvFileDescription outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',', // tab delimited
                FirstLineHasColumnNames = true, // no column names in first record
                FileCultureName = "hr-HR" // use formats used in Croatia
            };
            CsvContext cc = new CsvContext();
            cc.Write(
                _atmOutput,
                "output/"+_outputFileName,
                outputFileDescription);
        }
        
        
    }
}
