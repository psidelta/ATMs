using LINQtoCSV;

namespace FinaATM
{
    class AtmInput
    {
        [CsvColumn(Name = "RB", FieldIndex = 1)]
        public long Id { get; set; }

        [CsvColumn(Name = "PROIZVODAC BANKOMATA", FieldIndex = 2)] public string Manufacturer { get; set; }
        [CsvColumn(Name = "MODEL BANKOMATA", FieldIndex = 3)] public string Model{ get; set; }
        [CsvColumn(Name = "TIP MODELA", FieldIndex = 4)] public string Type { get; set; }
        [CsvColumn(Name = "GODINA PROIZVODNJE", FieldIndex = 5)] public int ManufacturingYear { get; set; }
        [CsvColumn(Name = "OTKUPNA CIJENA", FieldIndex = 6)] public decimal BuyingPrice { get; set; }
        [CsvColumn(Name = "ULICA", FieldIndex = 7)] public string Street { get; set; }
        [CsvColumn(Name = "KUCNI BROJ", FieldIndex = 8)] public string HouseNo { get; set; }
        [CsvColumn(Name = "POSTANSKI BROJ", FieldIndex = 9)] public string PostalCode { get; set; }
        [CsvColumn(Name = "GRAD", FieldIndex = 10)] public string City { get; set; }
        [CsvColumn(Name = "PROCELJE BANKE (DA/NE)", FieldIndex = 11)] public string IntraBank { get; set; }
        [CsvColumn(Name = "CIJENA NAJMA BEZ PDV-a (u HRK)", FieldIndex = 12)] public double RentPrice { get; set; }
        [CsvColumn(Name = "NAJAM UGOVOREN DO:", FieldIndex = 13)] public string RentContractTo { get; set; }
        [CsvColumn(Name = "UGOVORNE KLAUZLE U SLUCAJU PRIJEVREMENOG RASKIDA UGOVORA", FieldIndex = 14)] public string ContractualFraudClauses { get; set; }
        [CsvColumn(Name = "OSTALE POTPISANE OBVEZE PO BANKOMATU", FieldIndex = 15)] public string OtherContractualObligations { get; set; }
        [CsvColumn(Name = "BROJ ISPLATNIH TRANSAKCIJA - GODISNJE", FieldIndex = 16)] public double YearOutTrxCnt2019 { get; set; }
        [CsvColumn(Name = "BROJ UPLATNIH TRANSAKCIJA - GODISNJE", FieldIndex = 17)] public double YearInTrxCnt2019 { get; set; }
        [CsvColumn(Name = "BROJ OSTALIH TRANSAKCIJA - GODISNJE", FieldIndex = 18)] public double YearOtherTrxCnt2019 { get; set; }
        [CsvColumn(Name = "BROJ PUNJENJA GODISNJE", FieldIndex = 19)] public double YearLoads2019 { get; set; }
        [CsvColumn(Name = "BROJ ISPLATNIH TRANSAKCIJA - GODISNJE 2020", FieldIndex = 20)] public double YearOutTrxCnt2020 { get; set; }
        [CsvColumn(Name = "BROJ UPLATNIH TRANSAKCIJA - GODISNJE 2020", FieldIndex = 21)] public double YearInTrxCnt2020 { get; set; }
        [CsvColumn(Name = "BROJ OSTALIH TRANSAKCIJA - GODISNJE 2020", FieldIndex = 22)] public double YearOtherTrx2020 { get; set; }
        [CsvColumn(Name = "BROJ PUNJENJA GODISNJE 2020", FieldIndex = 23)] public double YearLoads2020 { get; set; }
        [CsvColumn(Name = "DATUM DEINSTALACIJE UREDAJA (ako je uredaj u meduvremenu deinstaliran)", FieldIndex = 24)] public string DeinstallationDate { get; set; }
        [CsvColumn(Name = "NAPOMENA", FieldIndex = 25)] public string Note { get; set; }
        [CsvColumn(Name = "ATM U NAJMU (DA/NE)", FieldIndex = 26)] public string RentedFlag { get; set; }
        [CsvColumn(Name = "24 SATNA ZONA (DA/NE)", FieldIndex = 27)] public string In24HrZone { get; set; }
        [CsvColumn(Name = "CIJENA MJESECNOG NAJMA UREDAJA", FieldIndex = 28)] public double MonthlyAtmRentalPrice { get; set; }
        [CsvColumn(Name = "UREDAJ U SKLADISTU (DA/NE)", FieldIndex = 29)] public string IsShelved { get; set; }
        [CsvColumn(Name = "BANKOMAT KOJI NE ULAZI U MREZU", FieldIndex = 30)] public string NotInNetworkFlag { get; set; }
        [CsvColumn(Name = "BANKA", FieldIndex = 31)] public string Bank { get; set; }
        [CsvColumn(Name = "SKRACENA OZNAKA BANKE", FieldIndex = 32)] public string BankDesignator { get; set; }
        [CsvColumn(Name = "PONOVNA CERTIFIKACIJA", FieldIndex = 33)] public double CertifiabilityScore { get; set; }
    }
}