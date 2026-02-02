namespace server.Services
{
    public interface IDisposalService
    {
        /// <summary>
        /// Обработать утиль: невозвратный брак списывается, возвратный перемещается в склад ЭКО.
        /// </summary>
        Task ProcessDisposalAsync(int disposalWarehouseId, int? materialId, int? productId,
            int returnableQuantity, int nonReturnableQuantity);
    }
}
