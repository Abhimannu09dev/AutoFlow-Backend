namespace AutoFlow_Backend.Domain.Entities;

public class SaleItems
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }  
    public decimal SubTotal { get; set; }   

    public Sale? Sale { get; set; }
    public Part? Part { get; set; }
}