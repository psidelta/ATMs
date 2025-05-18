using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinaATM
{
    public class DataModel
    {
        private readonly double _alpha;
        private readonly double _beta;
        private readonly double _delta;
        private readonly double _epsilon1;
        private readonly double _epsilon2;
        private readonly double _epsilon3;
        private readonly double _epsilon4;
        private readonly double _ageFunctionMean;
        private readonly double _ageFunctionSlope;
        public double Gamma { get; }

        public double[] Costs { get; }
        public double[] TrxCnt { get; }
        public long[] BlockedAtms { get; }
        public int[] AgeArray { get; }
        public int[] TypeArray { get; }
        public double[] CertArray { get; }
        private int minAge;
        private int maxAge;
        private double minProfitCoeff;
        private double maxProfitCoeff;
        private double[] profitCoeff;
        private double profitCoeffDenominator;
        private int ageCoeffDenominator;
        
        public double[] QsiDenominator{ get; private set; }
        
        public double[,] DistanceMatrix { get; }
        public int ATMCnt;

        public double [] ScaledAgeCoeffs{ get; set; }
        public double[] ScaledProfitCoeffs { get; set; }

        public DataModel(double alpha, double beta, double delta, double gamma, double epsilon_1, double epsilon_2, double epsilon_3, double epsilon_4, double ageFunctionMean, double ageFunctionSlope, double[] costs, double[] trxCnt,
            double[,] distanceMatrix, long[] blockedAtms, int[] ageArray, int[] typeArray, double[] certArray)
        {
            _alpha = alpha;
            _beta = beta;
            _delta = delta;
            _epsilon1 = epsilon_1;
            _epsilon2 = epsilon_2;
            _epsilon3 = epsilon_3;
            _epsilon4 = epsilon_4;
            _ageFunctionMean = ageFunctionMean;
            _ageFunctionSlope = ageFunctionSlope;
            Gamma = gamma;
            TrxCnt = trxCnt;
            Costs = costs;
            BlockedAtms = blockedAtms;
            AgeArray = ageArray;
            TypeArray = typeArray;
            CertArray = certArray;
            DistanceMatrix = distanceMatrix;
            ATMCnt = TrxCnt.Length;
            minAge = ageArray.Min();
            maxAge = ageArray.Max();
            profitCoeff = new double [ATMCnt];
            SetCoefficients();
        }

        public void SetCoefficients()
        {
            for (int i = 0; i < ATMCnt; i++)
            {
                profitCoeff[i] = Costs[i] / TrxCnt[i];
            }

            ScaledAgeCoeffs = new double[ATMCnt];
            ScaledProfitCoeffs = new double[ATMCnt];

            minProfitCoeff = profitCoeff.Min();
            maxProfitCoeff = profitCoeff.Max();
            profitCoeffDenominator = maxProfitCoeff - minProfitCoeff;
            ageCoeffDenominator = maxAge - minAge;

            for (int i = 0; i < ATMCnt; i++)
            {
                ScaledAgeCoeffs[i] = GetAgeCoeff(AgeArray[i]);
                ScaledProfitCoeffs[i] = ((Costs[i] / TrxCnt[i]) - minProfitCoeff) / profitCoeffDenominator;
            }


        }

        public double GetDistance(long i, long j)
        {
            var distance =  DistanceMatrix[i,j];

            return distance < 0.1 ? 0.1 : distance;
        }

        public double GetAgeCoeff(int age)
        {
            return 1 - 1/(1+Math.Exp(_ageFunctionSlope * (-(age - _ageFunctionMean))));
        }
        public int IndicatorAlpha(long i, long j)
        {
            return GetDistance(i, j) <= _alpha ? 1 : 0;
        }

        public int IndicatorBeta(long i, long j)
        {
            return GetDistance(i,j) <= _beta ? 1 : 0;
        }

        public double GetShare(long RemainingAtmIdx, long RemovedAtmIdx)
        {
            double denominator = 0;
            for (var i = 0; i < ATMCnt; i++)
            {
                if (i == RemovedAtmIdx) continue;

                denominator += IndicatorBeta(i, RemovedAtmIdx) * Math.Pow(GetDistance(i, RemovedAtmIdx), _delta);
            }

            var nominator = IndicatorBeta(RemainingAtmIdx, RemovedAtmIdx) *
                            Math.Pow(GetDistance(RemainingAtmIdx, RemovedAtmIdx), _delta);

            if (denominator == 0) return 0;

            return nominator / denominator;
        }

        public double GetNewTrxCnt(long atmIdx, Dictionary<long, double> solutionVector)
        {
            var newTrxCnt = TrxCnt[atmIdx];
            if (solutionVector[atmIdx] == 0) return 0;

            foreach (var item in solutionVector)
            {
                if (atmIdx == item.Key)
                {
                    continue;
                }

                var denominator = 0.0;

                foreach (var item2 in solutionVector)
                {
                    if (item2.Key == item.Key)
                    {
                        continue;
                    }

                    denominator += IndicatorBeta(item.Key, item2.Key) *
                                   Math.Pow(GetDistance(item.Key, item2.Key), _delta) * solutionVector[item2.Key];
                }
                if (denominator == 0) denominator = 1;

                newTrxCnt += TrxCnt[item.Key] * IndicatorBeta(atmIdx, item.Key) * Math.Abs(solutionVector[item.Key] - 1) *
                    Math.Pow(GetDistance(item.Key, atmIdx), _delta) / denominator;
            }


            return newTrxCnt;
        }

        public double AlternateGetNewTrxCnt(long atmIdx, double[] solutionVector)
        {
            if (solutionVector[atmIdx] == 0) return 0;
            var newTrxCnt = TrxCnt[atmIdx];

            for (var j = 0; j < solutionVector.Length; j++)
            {
                if (atmIdx == j) continue;

                var denominator = 0.0;

                for (var k = 0; k < solutionVector.Length; k++)
                {
                    if (k == j) continue;
                    denominator += IndicatorBeta(j, k) * Math.Pow(GetDistance(j, k), _delta) * solutionVector[k];
                }

                if (denominator == 0) denominator = 1;

                newTrxCnt += TrxCnt[j] * IndicatorBeta(atmIdx, j) * Math.Abs(solutionVector[j] - 1) *
                    Math.Pow(GetDistance(j, atmIdx), _delta) / denominator;
            }

            return newTrxCnt;

        }

        public double[] GetNewTrxCntList(double[] solutionVector)
        {
            var trxDict = new double[solutionVector.Length];
            InitializeQsiDenominator(solutionVector);

            for(int i = 0; i< solutionVector.Length;i++)
            {
                
                trxDict[i] = AlternateGetNewTrxCntUsingQsiDenominator(i, solutionVector);
            }


            return trxDict;
        }

        public double GoalCoefficient(long AtmIdx)
        {
            return _epsilon1*GetProfitCoeff(AtmIdx) -_epsilon2*GetAgeCoefficient(AtmIdx) + _epsilon3 *GetTypeCoefficient(AtmIdx)-_epsilon4 * GetCertCoefficient(AtmIdx);
        }

        public double GetProfitCoeff(long AtmIdx)
        {
            return ScaledProfitCoeffs[AtmIdx];
        }

        public double GetAgeCoefficient(long AtmIdx)
        {
            return ScaledAgeCoeffs[AtmIdx];
        }

        public double GetTypeCoefficient(long AtmIdx)
        {
            return TypeArray[AtmIdx];
        }

        public double GetCertCoefficient(long AtmIdx)
        {
            return CertArray[AtmIdx];
        }

        private void InitializeQsiDenominator(double[] solutionVector)
        {
            //called in trx list retrieval
            QsiDenominator = new double[solutionVector.Length];

            for (var i = 0; i < solutionVector.Length; i++)
            {
                var denominator = 0.0;
                if (solutionVector[i] > 0 )
                {
                    QsiDenominator[i] = denominator;
                    continue;
                }

                for (int j = 0; j < solutionVector.Length; j++)
                {
                    if (i == j) continue;
                    if (TrxCnt[j] > Gamma ? true : false) continue;
                    denominator +=  IndicatorBeta(i, j) * Math.Pow(GetDistance(i, j), _delta) * solutionVector[j];


                }

                QsiDenominator[i] = denominator;
            }
        }


        public double AlternateGetNewTrxCntUsingQsiDenominator(long atmIdx, double[] solutionVector)
        {
            if (solutionVector[atmIdx] == 0) return 0;
            var newTrxCnt = TrxCnt[atmIdx];

            if (newTrxCnt > Gamma)
            {
               return newTrxCnt;
            }

            for (var j = 0; j < solutionVector.Length; j++)
            {
                if (atmIdx == j) continue;

                var denominator = QsiDenominator[j];

                if (denominator == 0) denominator = 1;

                newTrxCnt += TrxCnt[j] * IndicatorBeta(atmIdx, j) * Math.Abs(solutionVector[j] - 1) *
                    Math.Pow(GetDistance(j, atmIdx), _delta) / denominator;
            }

            return newTrxCnt;

        }

    }
}