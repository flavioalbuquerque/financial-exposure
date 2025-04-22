using System.ComponentModel.DataAnnotations;

namespace OrderGenerator.Models;

public class OrderModel
{
    private const int MinQuantity = 1;
    private const int MaxQuantity = 99999;
    private const decimal MinPrice = 0.01M;
    private const decimal MaxPrice = 999.99M;
    
    
    [Required(ErrorMessage = "Símbolo é obrigatório.")]
    public string Symbol { get; set; }

    [Required(ErrorMessage = "Lado é obrigatório.")]
    public string Side { get; set; }

    [Required(ErrorMessage = "Quantidade é obrigatória.")]
    [Range(MinQuantity, MaxQuantity, ErrorMessage = "Quantidade deve ser maior que 0 e menor que 100.000.")]
    public int? Quantity { get; set; }
    
    [Required(ErrorMessage = "Preço é obrigatório.")]
    [Range((double)MinPrice, (double)MaxPrice, ErrorMessage = "Preço deve ser maior que 0,00 e menor que 1.000,00.")]
    public decimal? Price { get; set; }
    
    public string Account { get; set; }
}