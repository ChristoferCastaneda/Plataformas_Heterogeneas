using OrderNotifications.Models;

namespace OrderNotifications
{
    public class NotificationService : INotificationService
    {
        public void NotifyOrderStatus(Order order)
        {
            
            if (ShouldSendNoti(order, NotificationChannel.Email))
            {
                Console.WriteLine($"Enviando menaje por Email a {order.Customer.ContactInfo.Email}: Tu orden {order.Id} is {order.Status}");
            }

            if (ShouldSendNoti(order, NotificationChannel.SMS))
            {
                Console.WriteLine($"Enviando menaje por SMS a {order.Customer.ContactInfo.PhoneNumber}: Tu orden {order.Id} is {order.Status}");
            }

            if (ShouldSendNoti(order, NotificationChannel.WhatsApp))
            {
                Console.WriteLine($"Enviando menaje por WhatsApp a {order.Customer.ContactInfo.PhoneNumber}: Tu orden {order.Id} is {order.Status}");
            }
        }

        private bool ShouldSendNoti(Order order, NotificationChannel channel)
        {
            if (order.Customer.Preferences.ContainsKey(channel))
            {
                return order.Customer.Preferences[channel] == NotificationChannelPreference.Enabled;
            }

            // Nunca mandadar noti a el maravilloso pais de Arstotzka, porque no cae bien( ya lo implemente y no upe como moverlo)(LO LOGREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE)
            if (order.Customer.CountryCode == "AK")
            {
                return false;
            }

            // Simepre tener Email y SMS jalando
            if (channel == NotificationChannel.Email)
            {
                return true; 
            }


            if (channel == NotificationChannel.SMS)
            {
                return order.Customer.CountryCode != "MX"; 
            }

            if (channel == NotificationChannel.WhatsApp)
            {
                return order.Customer.CountryCode == "MX";
                return order.Customer.CountryCode != "AK";
            }

            return false;
        }


    }
}
