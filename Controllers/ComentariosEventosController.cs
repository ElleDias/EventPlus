using System.Threading.Tasks;
using Azure;
using Azure.AI.ContentSafety;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.event_.Contexts;
using webapi.event_.Domains;
using webapi.event_.Interfaces;
using webapi.event_.Repositories;

namespace webapi.event_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ComentariosEventosController : ControllerBase 
    {
        private readonly IComentariosEventosRepository _comentarioseventosRepository;
        private readonly ContentSafetyClient _contentSafetyClient;
        private readonly Context _context;
        public ComentariosEventosController(ContentSafetyClient contentSafetyClient, IComentariosEventosRepository comentariosEventosRepository, Context contexto)
        {
            _comentarioseventosRepository = comentariosEventosRepository;
            _contentSafetyClient = contentSafetyClient;
            _context = contexto;
        }
        [HttpPost]
        //o task representa uma funcao assincrona 
        public async Task<IActionResult> Post(ComentariosEventos comentario)
        {

            try
            {
                Eventos eventoBuscado = _context.Eventos.FirstOrDefault(e => e.IdEvento == comentario.IdEvento);

                if (eventoBuscado == null)
                {
                    return NotFound("Evento não encontrado");
                }

                if (eventoBuscado.DataEvento >= DateTime.UtcNow)
                {
                    return BadRequest("Não é possível comentar um evento que ainda não aconteceu");
                }



                if (string.IsNullOrEmpty(comentario.Descricao))
                {
                    return BadRequest("O texto a ser moderado nao pode estar vazio");
                }


                //criar um objeto de analise do content safety 
                var request = new AnalyzeTextOptions(comentario.Descricao);

                //chamar a api do content safety 
                Response<AnalyzeTextResult> response = await _contentSafetyClient.AnalyzeTextAsync(request);

                //verificar se o texto analisado tem alguma severidade(termos ofensivos)
                bool temConteudoImproprio = response.Value.CategoriesAnalysis.Any(c => c.Severity > 0);

                //se o conteudo for improprio, nao exibe, caso contrario, exibe
                comentario.Exibe = !temConteudoImproprio;
                _comentarioseventosRepository.Cadastrar(comentario);

                return Ok();
    }
	catch (Exception)
	{

		throw;
	}

        }
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                _comentarioseventosRepository.Deletar(id);

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("ListarSomenteExibe")]
        public IActionResult GetExibe(Guid id)
        {
            try
            {
                return Ok(_comentarioseventosRepository.ListarSomenteExibe(id));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public IActionResult Get(Guid id)
        {
            try
            {
                return Ok(_comentarioseventosRepository.Listar(id));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("BuscarPorIdUsuario")]
        public IActionResult GetByIdUser(Guid idUsuario, Guid idEvento)
        {
            try
            {
                return Ok(_comentarioseventosRepository.BuscarPorIdUsuario(idUsuario, idEvento));
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
