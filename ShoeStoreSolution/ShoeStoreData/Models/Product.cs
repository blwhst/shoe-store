using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStoreData.Models;

public partial class Product
{
    public string Article { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public decimal Price { get; set; }
    public int SupplierId { get; set; }
    public int ManufacturerId { get; set; }
    public int CategoryId { get; set; }
    public int Discount { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
    public string? Photo { get; set; }

    public virtual Category Category { get; set; } = null!;
    public virtual Manufacturer Manufacturer { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual Supplier Supplier { get; set; } = null!;

    // Вычисляемое свойство: цена товара с учетом скидки
    // Не хранится в базе данных (атрибут [NotMapped])
    // Используется в заданиях 4 и 6 для отображения итоговой цены
    // Если скидка больше 0, возвращает цену с применением скидки
    [NotMapped]
    public decimal DiscountPrice
    {
        get
        {
            if (Discount > 0)
            {
                return Price * (100 - Discount) / 100;
            }
            return Price;
        }
    }

    // Флаг выбора товара в интерфейсе
    // Не хранится в базе данных (атрибут [NotMapped])
    // Используется в задании 6 (WPF приложение) для выделения товаров
    // при операциях удаления или редактирования
    [NotMapped]
    public bool IsSelected { get; set; }
}