using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.ViewModels;

public class FinishedGoodsItem : INotifyPropertyChanged
{
    public int FillingId { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public double Quantity { get; set; }
    public string MeasuringUnit { get; set; } = string.Empty;

    // Display properties
    public string ProductName => Product?.Name ?? $"Продукт #{ProductId}";
    public string ProductCode => Product?.Code ?? string.Empty;

    private string _saleQty = string.Empty;
    private string _disposalQty = string.Empty;
    private bool _isProcessing;

    public string SaleQty
    {
        get => _saleQty;
        set { _saleQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
    }

    public string DisposalQty
    {
        get => _disposalQty;
        set { _disposalQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set { _isProcessing = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
    }

    public bool CanProcess => !IsProcessing && (HasSale || HasDisposal);

    public bool HasSale => double.TryParse(_saleQty, out var v) && v > 0;
    public bool HasDisposal => double.TryParse(_disposalQty, out var v) && v > 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
