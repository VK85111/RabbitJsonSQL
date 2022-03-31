using System;

namespace SQL_Solution.Model
{
    public class RootObject
    {
        public DateTime timestamp { get; set; }
        public string action { get; set; }
        public string model { get; set; }
        public Data data { get; set; }
        public string comment { get; set; }
    }
    public class Data
    {
        public string GUID { get; set; }
        public string number { get; set; }
        public string leasingcalculation { get; set; }
        public DateTime date { get; set; }
        public string bldblp { get; set; }
        public decimal summary { get; set; }

    }
}
