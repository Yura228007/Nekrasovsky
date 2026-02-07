using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.ViewModels;

public class SDHItem : INotifyPropertyChanged
{
    public int FillingId { get; set; }
    public int WarehouseId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public Material? Material { get; set; }
    public Product? Product { get; set; }
    public double Quantity { get; set; }
    public string MeasuringUnit { get; set; } = string.Empty;

    // Display properties
    public string ItemName => MaterialId.HasValue 
        ? (Material?.Name ?? $"Материал #{MaterialId}") 
        : (Product?.Name ?? $"Продукт #{ProductId}");
    public string ItemCode => MaterialId.HasValue 
        ? (Material?.Code ?? string.Empty) 
        : (Product?.Code ?? string.Empty);

    private string _nonReturnableDefectQty = string.Empty;
    private string _saleQty = string.Empty;
    private string _transferQty = string.Empty;
    private bool _isProcessing;

    public string NonReturnableDefectQty
    {
        get => _nonReturnableDefectQty;
        set { _nonReturnableDefectQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
    }

    public string SaleQty
    {
        get => _saleQty;
        set { _saleQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
    }

    public string TransferQty
    {
        get => _transferQty;
        set { _transferQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set { _isProcessing = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanProcess)); }
    }

    public bool CanProcess => !IsProcessing && (HasNonReturnableDefect || HasSale || HasTransfer);

    public bool HasNonReturnableDefect => double.TryParse(_nonReturnableDefectQty, out var v) && v > 0;
    public bool HasSale => double.TryParse(_saleQty, out var v) && v > 0;
    public bool HasTransfer => double.TryParse(_transferQty, out var v) && v > 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
