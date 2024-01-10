namespace LotteryResult.Extensions
{
    public static class DateTimeExtensions
    {
        //public static DateTime VenezuelaTimeZoneNow(this DateTime _) {
        //    // Obtén la fecha y hora actual en UTC
        //    DateTime utcNow = DateTime.UtcNow;

        //    return ToVenezuelaTimeZone(utcNow);
        //}

        public static DateTime ToVenezuelaTimeZone(this DateTime value) {

            TimeZoneInfo venezuelaZone = TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time");

            // Convierte la fecha y hora actual a la zona horaria de Venezuela
            return TimeZoneInfo.ConvertTimeFromUtc(value, venezuelaZone);
        }
    }
}
