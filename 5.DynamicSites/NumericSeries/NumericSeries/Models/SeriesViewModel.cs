namespace NumericSeries.Models
{
    public class SeriesViewModel
    {
        public string Series { get; set; } = string.Empty;
        public int N { get; set; }
        public int  Result { get; set; }

        public void SelectFun(){

            string serie = Series.ToLower();
            switch (serie)
            {
                case "natural":
                    Result = CalculateNatural(N);
                    break;
                case "fibonacci":
                    Result = CalculateFibonacci(N);
                    break;
                case "squares":
                    Result = CalculateSquares(N);
                    break;
                case "primes":
                    Result = CalculatePrimes(N);
                    break;
                case "evens":
                    Result = CalculateEven(N);
                    break;
                default:
                    Result = 0;
                    break;

            }
            ;

            
        }

        public static bool IsValidSeries(string seriesName)
        {
            var validSeries = new[] { "natural", "fibonacci", "squares", "primes", "evens" };
            return validSeries.Contains(seriesName.ToLower());
        }

        public static bool IsValidNumber(int N)
        {
            if (N >= 0)
            {
                return true;
            }else {
                return false;
            }

                
        }


        public static List<string> GetAvailableSeries()
        {
            return new List<string> { "natural", "fibonacci", "squares", "primes", "evens" };
        }

        private int  CalculateNatural(int position)
        {
            return position;
        }

        private int CalculateFibonacci(int position)
        {
            int a = 0;
            int b = 1;
            if (position <= 1) return position;

            
            for (int i = 2; i <= position; i++)
            {
                int  temp = a + b;
                a = b;
                b = temp;
            }
            return b;
        }

        private int CalculateSquares(int position)
        {
            return position * position;
        }

        private int  CalculatePrimes(int position)
        {
            if (position == 0) return 2;

            var primes = new List<int > { 2 };
            int  candidate = 3;

            while (primes.Count <= position)
            {
                bool isPrime = true;
                foreach (var prime in primes)
                {
                    if (prime * prime > candidate)
                    {
                        break;
                    }
                    if (candidate % prime == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }

                if (isPrime) primes.Add(candidate);
                candidate += 2;
            }

            return primes[position];
        }

        private int  CalculateEven(int position)
        {
            return position * 2;
        }

    }
}
