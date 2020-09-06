using System;
using System.Threading.Tasks;
using MassTransit;

namespace Client
{
    class Magazyn : IConsumer<IPytanieMagazynu>, IConsumer<IZrealizowane>, IConsumer<INiezrealizowane>
    {
        public int Wolne { get; set; } = 0;
        public int Zarezerwowane { get; set; } = 0;

        public Task Consume(ConsumeContext<IPytanieMagazynu> ctx)
        {
            return Task.Run(() =>
            {
                Wolne -= ctx.Message.ilosc;
                Zarezerwowane += ctx.Message.ilosc;
                if (Wolne + ctx.Message.ilosc >= ctx.Message.ilosc)
                {
                    ctx.RespondAsync(new PotwMagazyn() { CorrelationId = ctx.Message.CorrelationId });
                    Console.Out.WriteLineAsync($"Magazyn zarezerwowal ilosc {ctx.Message.ilosc}\nW magazynie zostalo wolnych: {Wolne} i zarezerwowanych {Zarezerwowane}.");
                }
                else
                {
                    ctx.RespondAsync(new OdmowaMagazyn() { CorrelationId = ctx.Message.CorrelationId });
                    Console.Out.WriteLineAsync($"Magazyn nie ma wystarczajaco duzo zasobow na zamowienie na ilosc {ctx.Message.ilosc}.");
                }
            });
        }

        public Task Consume(ConsumeContext<IZrealizowane> ctx)
        {
            return Task.Run(() =>
            {
                Zarezerwowane -= ctx.Message.ilosc;
                Console.Out.WriteLineAsync($"Zrealizowano zamowienie na ilosc {ctx.Message.ilosc}\nW magazynie zostalo wolnych: {Wolne} i zarezerwowanych {Zarezerwowane}.");
            });
        }

        public Task Consume(ConsumeContext<INiezrealizowane> ctx)
        {
            return Task.Run(() =>
            {
                Zarezerwowane -= ctx.Message.ilosc;
                Wolne += ctx.Message.ilosc;
                Console.Out.WriteLineAsync($"Anulowano zamowienie na ilosc {ctx.Message.ilosc}\nW magazynie zostalo wolnych: {Wolne} i zarezerwowanych {Zarezerwowane}.");
            });
        }
    }

    class Class1
    {
        static void Main(String[] args)
        {
            var Magazyn = new Magazyn();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(
                    new Uri("rabbitmq://localhost/VirtHost123"),
                    h => { h.Username("guest"); h.Password("guest"); });
                sbc.ReceiveEndpoint(host, "magazyn", ep => ep.Instance(Magazyn));
            });
            Console.WriteLine("Magazyn startuje");
            bus.Start();
            int dostawa = 0;
            while(Console.ReadKey().Key == ConsoleKey.Spacebar)
            {
                Console.WriteLine("\nPodaj liczbe produktow do dodania:");
                dostawa = Convert.ToInt32(Console.ReadLine());
                Magazyn.Wolne += dostawa;
                Console.WriteLine($"Po dostawie w magazynie znajduje sie wolnych: {Magazyn.Wolne} i zarezerwowanych: {Magazyn.Zarezerwowane}.");
            }
            bus.Stop();
        }
    }
}
