using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Client
{
    class Klient : IConsumer<IPytanieKlienta>, IConsumer<IZrealizowane>, IConsumer<INiezrealizowane>
    {
        public string Login = "Klient_A";

        public Task Consume(ConsumeContext<IPytanieKlienta> ctx)
        {
            if(ctx.Message.login == Login)
            {
                Console.WriteLine($"\nCzy potwierdzasz zamowienie na ilosc {ctx.Message.ilosc}? [T/N]");

                return Task.Run(() =>
                {
                    ConsoleKey a = Console.ReadKey().Key;
                    if (a == ConsoleKey.T)
                    {
                        ctx.RespondAsync(new PotwZamKlient() { CorrelationId = ctx.Message.CorrelationId });
                        Console.Out.WriteAsync($"\nPotwierdzono zamowienie na ilosc {ctx.Message.ilosc}");
                    }
                    else if (a == ConsoleKey.N)
                    {
                        ctx.RespondAsync(new OdmowaZamKlient() { CorrelationId = ctx.Message.CorrelationId });
                        Console.Out.WriteAsync($"\nOdmowiono zamowienie na ilosc {ctx.Message.ilosc}");
                    }
                });
            }
            else
            {
                return Task.Run(() => { });
            }
        }

        public Task Consume(ConsumeContext<IZrealizowane> ctx)
        {
            if(ctx.Message.login == Login)
            {
                return Task.Run(() => { Console.Out.WriteLineAsync($"\nZrealizowano zamowienie na ilosc {ctx.Message.ilosc}"); });
            }
            else
            {
                return Task.Run(() => { });
            }
        }

        public Task Consume(ConsumeContext<INiezrealizowane> ctx)
        {
            if (ctx.Message.login == Login)
            {
                return Task.Run(() => { Console.Out.WriteLineAsync($"\nAnulowano zamowienie na ilosc {ctx.Message.ilosc}"); });
            }
            else
            {
                return Task.Run(() => { });
            }
        }
    }

    class Class1
    {
        static void Main(String[] args)
        {
            var Klient = new Klient();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(
                    new Uri("rabbitmq://localhost/VirtHost123"),
                    h => { h.Username("guest"); h.Password("guest"); });
                sbc.ReceiveEndpoint(host, "klienta", ep => ep.Instance(Klient));
            });
            bus.Start();
            Console.WriteLine("Klient A startuje");
            while(true)
            {
                int zamow = 0;
                if(Console.ReadKey().Key == ConsoleKey.Spacebar)
                {
                    Console.WriteLine("\nPodaj ile chcesz zamowic:");
                    zamow = Convert.ToInt32(Console.ReadLine());
                    bus.Publish(new ZlozenieZamowienia() { login = "Klient_A", ilosc = zamow });
                }
            }
            bus.Stop();
        }
    }
}
