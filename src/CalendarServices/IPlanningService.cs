using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CalendarServices
{
    public interface IPlanningService
    {
        // maak xml van object om dan op rabbitmq te zwieren
        // map xml die binnen komt naar de juiste klasse
        // Handle stuff (als xml binnen komt -> map xml naar planningKlasse, en update dan in google calendar)
        // Polling google Calendar (of mss met endpoint en subscription als we een geldig certificaat kunnen maken)
        // Als er iets binnen komt van google calendar -> map naar planningKlasse en zwier op rabbitmq

        //Task ToXml(object obj);
        
    }
}
