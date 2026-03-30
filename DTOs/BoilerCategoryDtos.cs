namespace RajFabAPI.DTOs
{
  public class BoilerCategoryDto
    {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

     public Decimal HeatingSurfaceArea { get; set; }

  
  }

  public class CreateBoilerCategoryDto
    {
    public string Name { get; set; } = string.Empty;
    public Decimal HeatingSurfaceArea { get; set; }
   
  }
}
