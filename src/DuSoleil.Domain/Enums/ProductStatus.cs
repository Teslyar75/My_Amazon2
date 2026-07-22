namespace DuSoleil.Domain.Enums;

/// <summary>
/// Статус товара на витрине и в админке.
/// OutOfStock соответствует серой карточке «Out of stock» / «Notify when available».
/// </summary>
public enum ProductStatus
{
    /// <summary>Черновик — не показывается покупателям.</summary>
    Draft = 0,

    /// <summary>Активен — виден в каталоге и доступен к покупке.</summary>
    Active = 1,

    /// <summary>Нет в наличии — карточка видна, но купить нельзя.</summary>
    OutOfStock = 2,

    /// <summary>Снят с продажи / скрыт.</summary>
    Archived = 3
}
