using System.Globalization;

namespace ec.edu.monster.utils
{
    public static class Money
    {
        public static string Fmt(decimal v) =>
            v.ToString("C2", new CultureInfo("es-EC")); // ajusta cultura si deseas
    }
}
