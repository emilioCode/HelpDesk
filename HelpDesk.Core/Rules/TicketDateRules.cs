using System;

namespace HelpDesk.Core.Rules
{
    public static class TicketDateRules
    {
        public const string StartBeforeCreation = "La fecha de inicio no puede ser anterior a la fecha de creación de la orden.";
        public const string DatesRequired = "Favor completar los campos correspondientes a las fechas y horas";
        public const string EndBeforeStart = "Favor verificar la fecha y hora de inicio con la fecha y hora de termino. Este ultimo debe ser posterior a la fecha de inicio.";

        // Returns the first broken rule, or null when the dates are valid.
        public static string Validate(DateTime creationDate, DateTime? startDate, TimeSpan? startTime, DateTime? endDate, TimeSpan? endTime)
        {
            if (startDate != null && startDate.Value.Date < creationDate.Date)
            {
                return StartBeforeCreation;
            }

            if (startDate != null && endDate != null)
            {
                var endsBefore = endDate.Value.Date < startDate.Value.Date;
                var sameDayEndsEarlier = endDate.Value.Date == startDate.Value.Date && startTime > endTime;
                if (endsBefore || sameDayEndsEarlier)
                {
                    return EndBeforeStart;
                }
            }

            return null;
        }
    }
}
