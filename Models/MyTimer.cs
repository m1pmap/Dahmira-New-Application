using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Dahmira.Models
{
    public class MyTimer
    {
        private Timer _timer;
        private Action _action;

        public MyTimer(double intervalInSeconds, Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action)); // Проверка на null
            _timer = new Timer(intervalInSeconds * 1000); // Устанавливаем интервал в миллисекундах
            _timer.Elapsed += OnTimedEvent; // Подписываемся на событие Elapsed
            _timer.AutoReset = false; // Останавливаем таймер после первого срабатывания
        }

        public void Start()
        {
            _timer.Start(); // Запускаем таймер
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            _action.Invoke(); // Выполняем заданное действие
            _timer.Stop(); // Останавливаем таймер
        }

    }
}
