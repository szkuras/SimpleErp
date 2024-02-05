namespace SimpleErp.Models.Contract;

public class TruckRequest
{
    public string Code { get; set; }
    public string Name { get; set; }
    public TruckStatus Status { get; set; }
    public string Description { get; set; }
}
