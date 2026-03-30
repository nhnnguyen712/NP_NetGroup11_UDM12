using System;
using System.Windows.Input;

namespace MultiFileDownloader.Client
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Constructor cho hàm CÓ tham số
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Constructor cho hàm KHÔNG tham số (Để tương thích với TogglePause)
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(p => execute(), p => canExecute == null || canExecute())
        {
        }

        // Tự động kết nối với hệ thống quản lý lệnh của WPF
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // Vẫn giữ hàm này nếu bạn muốn ép buộc refresh thủ công
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}