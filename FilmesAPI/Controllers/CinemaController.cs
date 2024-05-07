using AutoMapper;
using FilmesAPI.Data;
using FilmesAPI.Data.Dtos;
using FilmesAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FilmesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CinemaController : ControllerBase
{
    private FilmeContext _context;
    private IMapper _mapper;

    public CinemaController(
        IMapper mapper, 
        FilmeContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    /// <summary>
    /// Adiciona um cinema ao banco de dados
    /// </summary>
    /// <param name="cinemaDto">Objeto com os campos necessários para criação de um cinema</param>
    /// <returns>IActionResult</returns>
    /// <response code="201"> Caso a inserção seja feita com sucesso</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaCinema(
        [FromBody] CreateCinemaDto cinemaDto)
    {
        var cinema = _mapper.Map<Cinema>(cinemaDto);

        try
        {
            _context.AddAsync(cinema);
            _context.SaveChanges();
        }
        catch (Exception e )
        {
            Console.WriteLine(e.Message);
            throw;
        }

        return
            CreatedAtAction(nameof(RecuperarCinemaPorId),
            new { id = cinema.Id }, cinema);
    }

    [HttpGet]
    public IEnumerable<ReadCinemaDto> RecuperarCinemas(
        [FromQuery] int? enderecoId = null)
    {
        if (enderecoId is null)
            return _mapper.Map<List<ReadCinemaDto>>(_context.Cinemas.ToList());

        return _mapper.Map<List<ReadCinemaDto>>(_context.Cinemas.
               FromSqlRaw($"SELECT Id, Nome, EnderecoId FROM cinemas WHERE cinemas.EnderecoId = {enderecoId}").ToList());
    }

    [HttpGet("{id}")]
    public IActionResult RecuperarCinemaPorId(int id)
    {
        var filme = _context.Cinemas.Where(o => o.Id == id).FirstOrDefault();

        if (filme is null)
            return NotFound();

        var filmeDto = _mapper.Map<ReadCinemaDto>(filme);

        return Ok(filmeDto);
    }

    [HttpPut("{id}")]
    public IActionResult AtualizaCinema(
        int id,
        [FromBody] UpdateCinemaDto cinemaDto)
    {
        var cinema = _context.Cinemas.Where(o => o.Id == id).FirstOrDefault();

        if (cinema is null)
            return NotFound();

        _mapper.Map(cinemaDto, cinema);
        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public IActionResult AtualizaFilmeParcial(
        int id,
        JsonPatchDocument<UpdateCinemaDto> patch)
    {
        var cinema = _context.Cinemas.Where(o => o.Id == id).FirstOrDefault();

        if (cinema is null)
            return NotFound();

        var cinemaParaAtualizar = _mapper.Map<UpdateCinemaDto>(cinema);

        patch.ApplyTo(cinemaParaAtualizar, ModelState);

        if (!TryValidateModel(cinemaParaAtualizar))
            return ValidationProblem(ModelState);

        _mapper.Map(cinemaParaAtualizar, cinema);
        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeletaCinema(int id)
    {
        var cinema = _context.Cinemas.Where(o => o.Id == id).FirstOrDefault();

        if (cinema is null)
            return NotFound();

        _context.Cinemas.Remove(cinema);
        _context.SaveChanges();

        return NoContent();
    }
}
