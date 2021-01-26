using System;

namespace EmployeeListLibrary.Helpers
{
  static class DateTimeHelper
  {
    /// <summary>
    /// Genarates a random long number >= 0.
    /// </summary>
    /// <param name="aRandom">Random generator</param>
    /// <param name="aMin">Lower limit (must be >= 0)</param>
    /// <param name="aMax">Upper limit (must be > aMax)</param>
    /// <returns>Genarated number</returns>
    public static long GenerateRandomLong(Random aRandom, long aMin, long aMax)
    {
      byte[] buf = new byte[8];
      aRandom.NextBytes(buf);
      buf[7] &= 0x7f;
      long res = BitConverter.ToInt64(buf, 0);
      res = (res % (aMax - aMin)) + aMin;
      return res;
    }
  }
}
