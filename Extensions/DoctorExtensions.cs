using DoctorAppointment.Models;

namespace DoctorAppointment.Extensions
{
    public static class DoctorExtensions
    {
        public static decimal CalculateTotalCharge(this Doctor doctor)
        {
            return doctor.ConsultationFee + 100;
        }
    }
}