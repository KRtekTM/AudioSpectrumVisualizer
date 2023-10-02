using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAudioSession.UI.FFT
{
    public static class FFTHelper
    {
        public static List<KeyValuePair<int, int>> Frequencies(int BarsCount)
        {
            int MaxHz = 20480;
            int[] PeakHz = { 0, 20, 30, 50, 70, 100, 200, 300, 500, 700, 1000, 2000, 3000, 5000, 7000, 10000, 20000 };

            List<int> PeakHzDiff = new List<int>();
            int HzInBar = MaxHz / BarsCount;
            List<int> startIds = new List<int>();
            List<int> endIds = new List<int>();
            List<KeyValuePair<int, int>> result = new List<KeyValuePair<int, int>>();

            for(int i = 0; i < PeakHz.Length - 1; i++)
            {
                PeakHzDiff.Add(PeakHz[i + 1] - PeakHz[i]);
            }
            PeakHzDiff.Add(MaxHz);

            for (int i = 0; i < PeakHzDiff.Count; i++)
            {
                if (i == 0)
                {
                    startIds.Add(0);
                }
                else
                {
                    startIds.Add(endIds[i - 1]);
                }

                if (i == PeakHzDiff.Count - 1)
                {
                    endIds.Add((BarsCount));
                }
                else
                {
                    endIds.Add((PeakHzDiff[i] / HzInBar) + (i > 0 ? endIds[i - 1] : 0));
                }

                result.Add(new KeyValuePair<int, int>(startIds[i], endIds[i]));
            }

            return result;
        }
    }
}
