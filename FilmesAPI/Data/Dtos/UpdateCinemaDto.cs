using System.ComponentModel.DataAnnotations;

namespace FilmesAPI.Data.Dtos;

public class UpdateCinemaDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Nome { get; set; }
    public int EnderecoId { get; set; }
}
