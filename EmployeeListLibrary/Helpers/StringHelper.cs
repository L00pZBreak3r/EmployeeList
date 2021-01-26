using System;

namespace EmployeeListLibrary.Helpers
{
  static class StringHelper
  {
    /// <summary>
    /// Genarates a random string which starts from a capital letter.
    /// </summary>
    /// <param name="aRandom">Random generator</param>
    /// <param name="aLength">Desired string length</param>
    /// <returns></returns>
    public static string GenerateRandomString(Random aRandom, int aLength = 8)
    {
      var stringChars = new char[aLength];

      stringChars[0] = (char)('A' + aRandom.Next(26));
      for (int i = 1; i < stringChars.Length; i++)
      {
        stringChars[i] = (char)('a' + aRandom.Next(26));
      }

      return new string(stringChars);
    }

    public static int CompareNames(string strA, string strB, bool ignoreCase)
    {
      if (string.IsNullOrWhiteSpace(strA) && string.IsNullOrWhiteSpace(strB))
        return 0;

      return string.Compare(strA, strB, ignoreCase);
    }
  }
}
