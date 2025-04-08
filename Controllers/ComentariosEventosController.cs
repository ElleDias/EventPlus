using System.Threading.Tasks;
using Azure;
using Azure.AI.ContentSafety;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public ComentariosEventosController(ContentSafetyClient contentSafetyClient, IComentariosEventosRepository comentariosEventosRepository)
        {
            _comentarioseventosRepository = comentariosEventosRepository;
            _contentSafetyClient = contentSafetyClient;
        }
        [HttpPost]
        //o task representa uma funcao assincrona 
        public async Task<IActionResult> Post(ComentariosEventos comentario)
        {

            try 
	         {
                //indica se a string(valor) eh vazia ou nula
                if (string.IsNullOrEmpty(comentario.Descricao))
                {
                    return BadRequest("O texto a ser moderado nao pode estar vazio!!");
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
    }
	catch (Exception)
	{

		throw;
	}

        }
    }
}
