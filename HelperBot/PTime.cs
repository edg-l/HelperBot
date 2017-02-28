using System;

namespace HelperBot
{
    public class PTime
    {
        public int Seconds = 0;
        public int Minutes = 0;
        public int Hours = 0;
        public int RawSeconds = 0;

        public void FillFromSeconds(int secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            Seconds = t.Seconds;
            Minutes = t.Minutes;
            Hours = t.Hours;
            RawSeconds = secs;
        }

        public void Update()
        {
            TimeSpan t = TimeSpan.FromSeconds(RawSeconds);
            Seconds = t.Seconds;
            Minutes = t.Minutes;
            Hours = t.Hours;
        }
    }
}
