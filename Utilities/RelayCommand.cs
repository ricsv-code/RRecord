using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Utilities
{
    public class RelayCommand : ICommand
    {
        #region Fields

        private Action<object> execute;
        private Predicate<object> canExecute;
        private Func<Action<object>> startCountdown;
        private event EventHandler CanExecuteChangedInternal;

        #endregion

        #region Constructors

        // Konstruktor som tar en execute-delegate och sätter canExecute till en default-metod (alltid true)
        public RelayCommand(Action<object> execute)
          : this(execute, DefaultCanExecute)
        {
        }

        // Konstruktor som tar både execute- och canExecute-delegater
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        // Konstruktor som tar en funktion som returnerar en execute-delegate.
        // Denna konstruktion möjliggör att ett kommando kan bestämma sin execute-logik dynamiskt vid körning.
        public RelayCommand(Func<Action<object>> startCountdown)
        {
            if (startCountdown == null)
            {
                throw new ArgumentNullException("startCountdown");
            }

            this.startCountdown = startCountdown;
            // Vi sätter canExecute till default (alltid true) för denna variant.
            this.canExecute = DefaultCanExecute;
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        #endregion

        #region Public methods

        // Bestämmer om kommandot kan köras.
        // Om startCountdown används returneras alltid true.
        public bool CanExecute(object parameter)
        {
            if (this.startCountdown != null)
            {
                return true;
            }
            return this.canExecute != null && this.canExecute(parameter);
        }

        // Kör kommandot.
        // Om startCountdown används, anropas den för att hämta en execute-delegate som sedan körs.
        // Annars används den sparade execute-delegaten.
        public void Execute(object parameter)
        {
            if (this.startCountdown != null)
            {
                Action<object> action = this.startCountdown();
                if (action != null)
                {
                    action(parameter);
                }
            }
            else
            {
                this.execute(parameter);
            }
        }

        // Anropar händelsen för att meddela att möjligheten att köra kommandot har ändrats.
        public void OnCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChangedInternal;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        // "Dödar" kommandot genom att sätta canExecute till false och execute till en tom metod.
        public void Destroy()
        {
            this.canExecute = _ => false;
            this.execute = _ => { return; };
        }

        #endregion

        #region Other methods

        // Standardmetod som anger att kommandot alltid kan köras.
        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }

        #endregion
    }
}
