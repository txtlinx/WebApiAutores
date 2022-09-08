using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    //[Authorize]
    public class AutoresController : ControllerBase 
    {
        private readonly ApplicationDBConetext context;
        private readonly ServicioTransient servicioTransient;
        private readonly ServicioSingleton servicioSingleton;
        private readonly ServicioScoped servicioScoped;
        private readonly ILogger<AutoresController> logger;

        public IServicio Servicio { get; }

        public AutoresController(ApplicationDBConetext conetext, IServicio servicio, ServicioTransient servicioTransient, ServicioSingleton servicioSingleton,
            ServicioScoped servicioScoped, ILogger<AutoresController> logger)
        {
            this.context = conetext;
            Servicio = servicio;
            this.servicioTransient = servicioTransient;
            this.servicioSingleton = servicioSingleton;
            this.servicioScoped = servicioScoped;
            this.logger = logger;
        }

        [HttpGet("GUID")]
        //[ResponseCache(Duration = 10)]
        [ServiceFilter(typeof(MiFiltroDeAccion))]
        public ActionResult ObtenerGuids()
        {
            return Ok(new
            {
                AutoresControllerTransient = servicioTransient.Guid,
                ServicioA_Transient = Servicio.ObtenerTransient(),
                AutoresControllerScoped = servicioScoped.Guid,
                ServicioA_Scoped = Servicio.ObtenerScoped(),
                AutoresControllerSingleton = servicioSingleton.Guid,
                ServicioA_Singleton = Servicio.ObtenerSingleton(),


            });
                

        }

        [HttpGet] // api/autores
        [HttpGet("listado")] // api/autores/listado
        [HttpGet("/listado")] // listado
        //[ResponseCache(Duration = 10)]
        [ServiceFilter(typeof(MiFiltroDeAccion))]

        public async Task<List<Autor>> Get()
        {
            throw new NotImplementedException();
            logger.LogInformation("Estamos obteniendo los autores");
            logger.LogWarning("este es un msg de prueba");

            Servicio.RealizarTarea();
            return await context.Autores.Include(x => x.Libros).ToListAsync();


        }

        [HttpGet("primero")] //api/autores/primero?nombre=nn
        public async Task<ActionResult<Autor>> PrimerAutor([FromHeader]int miValor, [FromQuery] string nombre)
        {
            return await context.Autores.FirstOrDefaultAsync();
        }
        [HttpGet("{id:int}/{param2=nombre}")]
        public async Task<ActionResult<Autor>> Get(int id, string nombre)
        {
           var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
            {
                return NotFound();  

            }
            return autor;
        }

        //[HttpGet("{id:int}")]

        [HttpGet("{nombre}")]
        public async Task<ActionResult<Autor>> Get([FromRoute]string nombre)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Nombre.Contains(nombre));
            if (autor == null)
            {
                return NotFound();

            }
            return autor;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]Autor autor)
        {
            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre  == autor.Nombre);
            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autor.Nombre}");
            }
            
            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }
        [HttpPut]
        public async Task<ActionResult> Put(Autor autor, int id)
        {
            if (autor.Id != id)
            {
                return BadRequest("id no coincide");
            }
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x =>  x.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }
      
    }
}
