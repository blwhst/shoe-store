using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStoreData.Models;

public partial class Order
{
    public DateOnly OrderDate { get; set; }

    public DateOnly? DeliveryDate { get; set; }

    public int CustomerId { get; set; }

    public int PickupCode { get; set; }

    public string Status { get; set; } = null!;

    public int OrderId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Вычисляемое свойство: общая стоимость заказа
    // Не хранится в базе данных (атрибут [NotMapped])
    // Рассчитывается как сумма стоимостей всех позиций заказа с учетом скидок
    // Используется в задании 5 для отображения в веб-приложении
    [NotMapped]
    public decimal TotalPrice { get; set; }
}
