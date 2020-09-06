using System;
using MassTransit;
using MassTransit.Saga;
using Automatonymous;

namespace Client
{
    public class RealizacjaZamowieniaDane : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string login { get; set; }
        public int ilosc { get; set; }
        public Guid? TimeoutId { get; set; }
    }

    public class RealizacjaZamowieniaSaga : MassTransitStateMachine<RealizacjaZamowieniaDane>
    {
        public State Niepotwierdzony { get; private set; }
        public State PotwierdzilKlient { get; private set; }
        public State PotwierdzilMagazyn { get; private set; }

        public Event<IZlozenieZamowienia> ZloZam { get; private set; }
        public Event<IPotwZamKlient> PotwKlient { get; private set; }
        public Event<IOdmowaZamKlient> OdmowaKlient { get; private set; }
        public Event<IPotwMagazyn> PotwMagazyn { get; private set; }
        public Event<IOdmowaMagazyn> OdmowaMagazyn { get; private set; }
        public Event<ITimeout> ETimeout { get; private set; }
        public Schedule<RealizacjaZamowieniaDane, ITimeout> TimeoutSchedule { get; private set; }

        public RealizacjaZamowieniaSaga()
        {
            InstanceState(x => x.CurrentState);
            Event(() => ZloZam, x => x.CorrelateBy(s => s.login, ctx => ctx.Message.login).SelectId(ContextBoundObject => Guid.NewGuid()));
            Schedule(() => TimeoutSchedule, x => x.TimeoutId, x => { x.Delay = TimeSpan.FromSeconds(10); });
            Initially(
                When(ZloZam)
                .Schedule(TimeoutSchedule, ctx => new Timeout() { CorrelationId = ctx.Instance.CorrelationId })
                .Then(ctx => ctx.Instance.login = ctx.Data.login)
                .Then(ctx => ctx.Instance.ilosc = ctx.Data.ilosc)
                .Respond(ctx => { return new PytanieKlienta() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .Respond(ctx => { return new PytanieMagazynu() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc }; })
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Złozono zamowienie dla klienta {ctx.Instance.login} na ilosc {ctx.Instance.ilosc}."); })
                .TransitionTo(Niepotwierdzony)
                );

            During(Niepotwierdzony,
                When(ETimeout)
                .Respond(ctx => { return new Niezrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Timeout dla zamowienia klienta {ctx.Instance.login} na ilosc {ctx.Instance.ilosc}."); })
                .Finalize(),

                When(PotwKlient)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Klient {ctx.Instance.login} potwierdzil zamowienie."); })
                .TransitionTo(PotwierdzilKlient),

                When(OdmowaKlient)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Klient {ctx.Instance.login} odmowil zamowienie."); })
                .Respond(ctx => { return new Niezrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .Finalize(),

                When(PotwMagazyn)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Magazyn potwierdzil zamowienie klienta {ctx.Instance.login} na ilosc {ctx.Instance.ilosc}"); })
                .TransitionTo(PotwierdzilMagazyn),

                When(OdmowaMagazyn)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Magazyn odmowil zamowienie klienta {ctx.Instance.login} na ilosc {ctx.Instance.ilosc}"); })
                .Respond(ctx => { return new Niezrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .Finalize()
                );

            During(PotwierdzilKlient,
                When(PotwMagazyn)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Magazyn potwierdzil zamowienie klienta {ctx.Instance.login} na ilosc {ctx.Instance.ilosc}"); })
                .Unschedule(TimeoutSchedule)
                .Respond(ctx => { return new Zrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .Finalize(),

                When(OdmowaMagazyn)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Magazyn odmowil zamowienie klienta {ctx.Instance.login} na ilosc {ctx.Instance.ilosc}"); })
                .Respond(ctx => { return new Niezrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .Finalize(),

                When(ETimeout)
                .Respond(ctx => { return new Niezrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Timeout dla zamowienia klienta {ctx.Instance.login} na ilosc {ctx.Instance.ilosc}."); })
                .Finalize()
                );

            During(PotwierdzilMagazyn,
                When(PotwKlient)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Klient {ctx.Instance.login} potwierdzil zamowienie na ilosc {ctx.Instance.ilosc}"); })
                .Unschedule(TimeoutSchedule)
                .Respond(ctx => { return new Zrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .Finalize(),

                When(OdmowaKlient)
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Klient {ctx.Instance.login} odmowil zamowienie na ilosc {ctx.Instance.ilosc}"); })
                .Respond(ctx => { return new Niezrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .Finalize(),

                When(ETimeout)
                .Respond(ctx => { return new Niezrealizowane() { CorrelationId = ctx.Instance.CorrelationId, ilosc = ctx.Instance.ilosc, login = ctx.Instance.login }; })
                .ThenAsync(ctx => { return Console.Out.WriteLineAsync($"Timeout dla zamowienia klienta {ctx.Instance.login} na ilosc {ctx.Instance.ilosc}."); })
                .Finalize()
                );

            SetCompletedWhenFinalized();
        }
    }

    public class Class1
    {
        static void Main(String[] args)
        {
            var repo = new InMemorySagaRepository<RealizacjaZamowieniaDane>();
            var saga = new RealizacjaZamowieniaSaga();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(
                    new Uri("rabbitmq://localhost/VirtHost123"),
                    h => { h.Username("guest"); h.Password("guest"); });
                sbc.ReceiveEndpoint(host, "saga", ep =>
                {
                    ep.StateMachineSaga(saga, repo);
                });
                sbc.UseInMemoryScheduler();
            });
            bus.Start();
            Console.WriteLine("Sklep rusza");
            Console.ReadKey();
            bus.Stop();
        }
    }
}
