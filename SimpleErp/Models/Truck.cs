namespace SimpleErp.Models;

public class Truck : ModelBase
{
    public string Code { get; set; }
    public string Name { get; set; }
    public TruckStatus Status { get; set; }
    public string Description { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
}