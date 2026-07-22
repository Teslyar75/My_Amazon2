namespace DuSoleil.Domain.Entities;

/// <summary>
/// Характеристика товара (пара «имя — значение»).
/// Используется в таблице specs на Product Page и в фильтрах каталога
/// (Brand, Fabric type, Size, Color и т.д.).
/// </summary>
public class ProductAttribute
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    /// <summary>Название характеристики, например «Color» или «Item weight».</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Значение, например «Black» или «1.6 ounces».</summary>
    public string Value { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    /// <summary>Можно ли использовать в боковых фильтрах Product List.</summary>
    public bool IsFilterable { get; set; }
}
