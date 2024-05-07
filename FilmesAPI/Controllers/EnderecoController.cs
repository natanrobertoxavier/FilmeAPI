using AutoMapper;
using FilmesAPI.Data;
using FilmesAPI.Data.Dtos;
using FilmesAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FilmesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class EnderecoController : ControllerBase
{
    private FilmeContext _context;
    private IMapper _mapper;

    public EnderecoController(
        FilmeContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Adiciona um endereço ao banco de dados
    /// </summary>
    /// <param name="enderecoDto">Objeto com os campos necessários para criação de um endereço</param>
    /// <returns>IActionResult</returns>
    /// <response code="201"> Caso a inserção seja feita com sucesso</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaEndereco(
        [FromBody] CreateEnderecoDto enderecoDto)
    {
        var endereco = _mapper.Map<Endereco>(enderecoDto);

        _context.AddAsync(endereco);
        _context.SaveChanges();

        return
            CreatedAtAction(nameof(RecuperarEnderecoPorId),
            new { id = endereco.Id }, endereco);
    }

    [HttpGet]
    public IEnumerable<ReadEnderecoDto> RecuperarEnderecos(
        [FromQuery] int skip = 0,
        [FromQuery][Required] int take = 5)
    {
        return _mapper.Map<List<ReadEnderecoDto>>(_context.Enderecos.Skip(skip).Take(take));
    }

    [HttpGet("{id}")]
    public IActionResult RecuperarEnderecoPorId(int id)
    {
        var endereco = _context.Enderecos.Where(o => o.Id == id).FirstOrDefault();

        if (endereco is null)
            return NotFound();

        var enderecoDto = _mapper.Map<ReadEnderecoDto>(endereco);

        return Ok(enderecoDto);
    }

    [HttpPut("{id}")]
    public IActionResult AtualizaFilme(
        int id,
        [FromBody] UpdateEnderecoDto enderecoDto)
    {
        var endereco = _context.Enderecos.Where(o => o.Id == id).FirstOrDefault();

        if (endereco is null)
            return NotFound();

        _mapper.Map(enderecoDto, endereco);
        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public IActionResult AtualizaEnderecoParcial(
        int id,
        JsonPatchDocument<UpdateEnderecoDto> patch)
    {
        var endereco = _context.Filmes.Where(o => o.Id == id).FirstOrDefault();

        if (endereco is null)
            return NotFound();

        var enderecoParaAtualizar = _mapper.Map<UpdateEnderecoDto>(endereco);

        patch.ApplyTo(enderecoParaAtualizar, ModelState);

        if (!TryValidateModel(enderecoParaAtualizar))
            return ValidationProblem(ModelState);

        _mapper.Map(enderecoParaAtualizar, endereco);
        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeletaEndereco(int id)
    {
        var endereco = _context.Enderecos.Where(o => o.Id == id).FirstOrDefault();

        if (endereco is null)
            return NotFound();

        _context.Enderecos.Remove(endereco);
        _context.SaveChanges();

        return NoContent();
    }
}
