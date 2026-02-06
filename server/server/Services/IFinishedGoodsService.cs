namespace server.Services;

public interface IFinishedGoodsService
{
    /// <summary>
    /// Оформить продажу готовой продукции (списать со склада)
    /// </summary>
    Task ProcessSaleAsync(int userId, int warehouseId, int productId, double quantity, string? measuringUnit);

    /// <summary>
    /// Отправить готовую продукцию в утиль (создать запрос на утиль)
    /// </summary>
    Task ProcessDisposalAsync(int userId, int warehouseId, int productId, double quantity, string? measuringUnit);
}
