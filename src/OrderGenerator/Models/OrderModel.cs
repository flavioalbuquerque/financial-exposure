using System.ComponentModel.DataAnnotations;

namespace OrderGenerator.Models;

public class OrderModel
{
    [Required(ErrorMessage = "Símbolo é obrigatório.")]
    public string Symbol { get; set; }

    [Required(ErrorMessage = "Lado é obrigatório.")]
    public string Side { get; set; }

    [Required(ErrorMessage = "Quantidade é obrigatória.")]
    [Range(1, 99999, ErrorMessage = "Quantidade deve ser maior que 0 e menor que 100.000.")]
    public int? Quantity { get; set; }
    
    [Required(ErrorMessage = "Preço é obrigatório.")]
    [Range(0.01, 999.99, ErrorMessage = "Preço deve ser maior que 0,00 e menor que 1.000,00.")]
    public decimal? Price { get; set; }
    
    public string Account { get; set; }
}