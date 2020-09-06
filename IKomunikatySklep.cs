using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Client
{
    public interface IZlozenieZamowienia
    {
        string login { get; set; }
        int ilosc { get; set; }
    }

    public interface IPotwZamKlient : CorrelatedBy<Guid>
    { }

    public interface IOdmowaZamKlient : CorrelatedBy<Guid>
    { }

    public interface IPotwMagazyn : CorrelatedBy<Guid>
    { }

    public interface IOdmowaMagazyn : CorrelatedBy<Guid>
    { }

    public interface IPytanieKlienta : CorrelatedBy<Guid>
    {
        int ilosc { get; set; }
        string login { get; set; }
    }

    public interface IPytanieMagazynu : CorrelatedBy<Guid>
    {
        int ilosc { get; set; }
    }

    public interface IZrealizowane : CorrelatedBy<Guid>
    {
        int ilosc { get; set; }
        string login { get; set; }
    }

    public interface INiezrealizowane : CorrelatedBy<Guid>
    {
        int ilosc { get; set; }
        string login { get; set; }
    }

    public interface ITimeout : CorrelatedBy<Guid>
    { }
}
