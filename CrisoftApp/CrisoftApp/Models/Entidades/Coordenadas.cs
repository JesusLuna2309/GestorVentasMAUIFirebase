using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrisoftApp.Models.Entidades
{
    internal class Coordenadas
    {
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public Location Location { get; set; }

        public Coordenadas(double latitud, double longitud)
        {
            Latitud = latitud;
            Longitud = longitud;
            Location = new Location(latitud, longitud);
        }
    }
}
