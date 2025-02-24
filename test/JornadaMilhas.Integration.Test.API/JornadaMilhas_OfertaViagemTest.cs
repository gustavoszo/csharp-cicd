using JornadaMilhas.Dados;
using JornadaMilhas.Dominio.Entidades;
using JornadaMilhas.Dominio.ValueObjects;
using JornadaMilhas.Integration.Test.API.DataBuilders;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace JornadaMilhas.Integration.Test.API
{
    public class JornadaMilhas_OfertaViagemTest : IClassFixture<JornadaMilhasWebApplicationFactory>
    {
        private readonly JornadaMilhasWebApplicationFactory app;

        public JornadaMilhas_OfertaViagemTest(JornadaMilhasWebApplicationFactory app)
        {
            this.app = app;
        }

        [Fact]
        public async Task Recupera_OfertaViagem_PorId()
        {
            //Arrange
            var ofertaExistente = app.Context.OfertasViagem.FirstOrDefault();
            if (ofertaExistente is null)
            {
                ofertaExistente = new OfertaViagem()
                {
                    Preco = 100,
                    Rota = new Rota("Origem", "Destino"),
                    Periodo = new Periodo(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-06"))
                };
                app.Context.Add(ofertaExistente);
                app.Context.SaveChanges();
            }

            using var client = await app.GetClientWithAccessTokenAsync();

            //Act
            var response = await client.GetFromJsonAsync<OfertaViagem>("/ofertas-viagem/" + ofertaExistente.Id);

            //Assert
            Assert.NotNull(response);
            Assert.Equal(ofertaExistente.Preco, response.Preco, 0.001);
            Assert.Equal(ofertaExistente.Rota.Origem, response.Rota.Origem);
            Assert.Equal(ofertaExistente.Rota.Destino, response.Rota.Destino);

        }

        [Fact]
        public async Task Recuperar_OfertasViagens_Na_Consulta_Paginada()
        {
            //Arrange
            var ofertaDataBuilder = new OfertaViagemDataBuilder();
            var listaDeOfertas = ofertaDataBuilder.Generate(80);
            app.Context.OfertasViagem.AddRange(listaDeOfertas);
            app.Context.SaveChanges();

            using var client = await app.GetClientWithAccessTokenAsync();

            int pagina = 1;
            int tamanhoPorPagina = 80;

            //Act
            var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

            //Assert
            Assert.True(response != null);
            Assert.Equal(tamanhoPorPagina, response.Count());

        }

        [Fact]
        public async Task Recuperar_OfertasViagens_Na_Consulta_Ultima_Pagina()
        {
            //Arrange
            app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

            var ofertaDataBuilder = new OfertaViagemDataBuilder();
            var listaDeOfertas = ofertaDataBuilder.Generate(80);
            app.Context.OfertasViagem.AddRange(listaDeOfertas);
            app.Context.SaveChanges();

            using var client = await app.GetClientWithAccessTokenAsync();

            int pagina = 4;
            int tamanhoPorPagina = 25;

            //Act
            var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

            //Assert
            Assert.True(response != null);
            Assert.Equal(5, response.Count());
        }

        [Fact]
        public async Task Recuperar_OfertasViagens_Na_Consulta_Com_Pagina_Inexistente()
        {
            //Arrange
            app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

            var ofertaDataBuilder = new OfertaViagemDataBuilder();
            var listaDeOfertas = ofertaDataBuilder.Generate(80);
            app.Context.OfertasViagem.AddRange(listaDeOfertas);
            app.Context.SaveChanges();

            using var client = await app.GetClientWithAccessTokenAsync();

            int pagina = 5;
            int tamanhoPorPagina = 25;

            //Act
            var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

            //Assert
            Assert.True(response != null);
            Assert.Equal(0, response.Count());
        }

        [Fact]
        public async Task Recuperar_OfertasViagens_Na_Consulta_Com_Pagina_Com_Valor_Negativo()
        {
            //Arrange   
            app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

            var ofertaDataBuilder = new OfertaViagemDataBuilder();
            var listaDeOfertas = ofertaDataBuilder.Generate(80);
            app.Context.OfertasViagem.AddRange(listaDeOfertas);
            app.Context.SaveChanges();

            using var client = await app.GetClientWithAccessTokenAsync();

            int pagina = -5;
            int tamanhoPorPagina = 25;

            //Act + Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {

                var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");
            });

        }

        [Fact]
        public async Task Cadastra_OfertaViagem()
        {
            //Arrange
            using var client = await app.GetClientWithAccessTokenAsync();

            var ofertaViagem = new OfertaViagem()
            {
                Preco = 100,
                Rota = new Rota("Origem", "Destino"),
                Periodo = new Periodo(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-06"))
            };
            //Act
            var response = await client.PostAsJsonAsync("/ofertas-viagem", ofertaViagem);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Cadastra_OfertaViagem_SemAutorizacao()
        {
            //Arrange   

            using var client = app.CreateClient();

            var ofertaViagem = new OfertaViagem()
            {
                Preco = 100,
                Rota = new Rota("Origem", "Destino"),
                Periodo = new Periodo(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-06"))
            };
            //Act
            var response = await client.PostAsJsonAsync("/ofertas-viagem", ofertaViagem);

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Atualiza_OfertaViagem_PorId()
        {
            //Arrange
            var ofertaExistente = app.Context.OfertasViagem.FirstOrDefault();
            if (ofertaExistente is null)
            {
                ofertaExistente = new OfertaViagem()
                {
                    Preco = 100,
                    Rota = new Rota("Origem", "Destino"),
                    Periodo = new Periodo(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-06"))
                };
                app.Context.Add(ofertaExistente);
                app.Context.SaveChanges();
            }

            using var client = await app.GetClientWithAccessTokenAsync();

            ofertaExistente.Rota.Origem = "Origem Atualizada";
            ofertaExistente.Rota.Destino = "Destino Atualizada";

            //Act
            var response = await client.PutAsJsonAsync($"/ofertas-viagem/", ofertaExistente);

            //Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Deletar_OfertaViagem_PorId()
        {
            //Arrange
            var ofertaExistente = app.Context.OfertasViagem.FirstOrDefault();
            if (ofertaExistente is null)
            {
                ofertaExistente = new OfertaViagem()
                {
                    Preco = 100,
                    Rota = new Rota("Origem", "Destino"),
                    Periodo = new Periodo(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-06"))
                };
                app.Context.Add(ofertaExistente);
                app.Context.SaveChanges();
            }

            using var client = await app.GetClientWithAccessTokenAsync();

            //Act
            var response = await client.DeleteAsync("/ofertas-viagem/" + ofertaExistente.Id);

            //Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

    }
}
