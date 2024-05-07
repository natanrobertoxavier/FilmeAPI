using AutoMapper;
using FilmesAPI.Data;
using FilmesAPI.Data.Dtos;
using FilmesAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace FilmesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmeController : ControllerBase
{
    private FilmeContext _context;
    private IMapper _mapper;

    public FilmeController(
        FilmeContext context, 
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Adiciona um filme ao banco de dados
    /// </summary>
    /// <param name="filmeDto">Objeto com os campos necessários para criação de um filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="201"> Caso a inserção seja feita com sucesso</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaFilme(
        [FromBody] CreateFilmeDto filmeDto)
    {
        var filme = _mapper.Map<Filme>(filmeDto);

        _context.AddAsync(filme);
        _context.SaveChanges();

        return 
            CreatedAtAction(nameof(RecuperarFilmePorId), 
            new { id = filme.Id }, filme);
    }

    [HttpGet]
    public IEnumerable<ReadFilmeDto> RecuperarFilmes(
        [FromQuery] int skip = 0,
        [FromQuery] [Required] int take = 5,
        [FromQuery] string? nomeCinema = null)
    {
        if (nomeCinema is null)
            return _mapper.Map<List<ReadFilmeDto>>(_context.Filmes.Skip(skip).Take(take).ToList());

        return _mapper.Map<List<ReadFilmeDto>>(_context.Filmes.Skip(skip).Take(take).
            Where(filme => filme.Sessoes.Any(sessao => sessao.Cinema.Nome == nomeCinema)).ToList());
    }

    [HttpGet("{id}")]
    public IActionResult RecuperarFilmePorId(int id)
    {
        var filme = _context.Filmes.Where(o => o.Id == id).FirstOrDefault();

        if (filme is  null)
            return NotFound();

        var filmeDto = _mapper.Map<ReadFilmeDto>(filme);

        return Ok(filmeDto);
    }

    [HttpPut("{id}")]
    public IActionResult AtualizaFilme(
        int id,
        [FromBody] UpdateFilmeDto filmeDto)
    {
        var filme = _context.Filmes.Where(o => o.Id == id).FirstOrDefault();

        if (filme is null)
            return NotFound();

        _mapper.Map(filmeDto, filme);
        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public IActionResult AtualizaFilmeParcial(
        int id,
        JsonPatchDocument<UpdateFilmeDto> patch)
    {
        var filme = _context.Filmes.Where(o => o.Id == id).FirstOrDefault();

        if (filme is null)
            return NotFound();
        
        var filmeParaAtualizar = _mapper.Map<UpdateFilmeDto>(filme);

        patch.ApplyTo(filmeParaAtualizar, ModelState);

        if (!TryValidateModel(filmeParaAtualizar))
            return ValidationProblem(ModelState);
        
        _mapper.Map(filmeParaAtualizar, filme);
        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeletaFilme(int id)
    {
        var filme = _context.Filmes.Where(o => o.Id == id).FirstOrDefault();

        if (filme is null)
            return NotFound();

        _context.Filmes.Remove(filme);
        _context.SaveChanges();

        return NoContent();
    }
}
