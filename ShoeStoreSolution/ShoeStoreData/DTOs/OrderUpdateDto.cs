namespace ShoeStoreAPI.Controllers
{
    // DTO для обновления данных заказа
    // Используется в задании 3 (веб-API) для изменения статуса и даты доставки
    // Поля nullable, чтобы можно было обновлять только часть данных
    public class OrderUpdateDto
    {
        public string? Status { get; set; }
        public DateOnly? DeliveryDate { get; set; }
    }
}