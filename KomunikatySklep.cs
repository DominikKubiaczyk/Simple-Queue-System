using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Client
{
    public class ZlozenieZamowienia : IZlozenieZamowienia
    {
        public string login { get; set; }
        public int ilosc { get; set; }
    }

    public class PotwZamKlient : IPotwZamKlient
    {
        public Guid CorrelationId { get; set; }
    }

    public class OdmowaZamKlient : IOdmowaZamKlient
    {
        public Guid CorrelationId { get; set; }
    }

    public class PotwMagazyn : IPotwMagazyn
    {
        public Guid CorrelationId { get; set; }
    }

    public class OdmowaMagazyn : IOdmowaMagazyn
    {
        public Guid CorrelationId { get; set; }
    }

    public class PytanieKlienta : IPytanieKlienta
    {
        public Guid CorrelationId { get; set; }
        public int ilosc { get; set; }
        public string login { get; set; }
    }

    public class PytanieMagazynu : IPytanieMagazynu
    {
        public Guid CorrelationId { get; set; }
        public int ilosc { get; set; }
    }

    public class Zrealizowane : IZrealizowane
    {
        public Guid CorrelationId { get; set; }
        public int ilosc { get; set; }
        public string login { get; set; }
    }

    public class Niezrealizowane : INiezrealizowane
    {
        public Guid CorrelationId { get; set; }
        public int ilosc { get; set; }
        public string login { get; set; }
    }

    public class Timeout : ITimeout
    {
        public Guid CorrelationId { get; set; }
    }
}
