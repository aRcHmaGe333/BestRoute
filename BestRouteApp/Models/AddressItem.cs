using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BestRouteApp.Models;

public sealed class AddressItem : INotifyPropertyChanged
{
    private string _address;
    private bool _isCompleted;
    private int? _completionOrder;

    public AddressItem(string address)
    {
        _address = address;
    }

    public string Address
    {
        get => _address;
        set
        {
            if (_address != value)
            {
                _address = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (_isCompleted != value)
            {
                _isCompleted = value;
                OnPropertyChanged();
            }
        }
    }

    public int? CompletionOrder
    {
        get => _completionOrder;
        set
        {
            if (_completionOrder != value)
            {
                _completionOrder = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
