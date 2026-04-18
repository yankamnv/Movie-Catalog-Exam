using MovieCatalogTests.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace MovieCatalogTests
{
    public class Tests
    {
        private RestClient client;
        private static string movieId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("yankas@m.com", "123456");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient client = new RestClient("http://144.91.123.158:5000");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            RestResponse response = client.Execute(request);

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").ToString();

                if(string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException("Failed to authenticate");
            }
        }

        [Order(1)]
        [Test]
        public void CreateNewMovie_WithRequiredFields_ShouldReturnSuccess()
        {
            MovieDTO movie = new MovieDTO()
            {
                Id = "",
                Title = "Dumb and Dumber",
                Description = "Comedy movie"
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDTO readyResponse  =  JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse.Movie, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));

                movieId = readyResponse.Movie.Id;

        }

        [Order(2)]
        [Test]
        public void EditTheCreatedMovie_ShouldReturnSuccess()
        {
            var editedMovie = new MovieDTO()
            {
                Id = movieId,
                Title = "Dumb and Dumber 2",
                Description = "Second movie"
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", movieId);
            request.AddJsonBody(editedMovie);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            List<MovieDTO> readyResponse = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);

            Assert.That(readyResponse, Is.Not.Null.And.Not.Empty);
            Assert.That(readyResponse.Count, Is.GreaterThan(0));

        }

        [Order(4)]
        [Test]
        public void DeleteCreatedMovie_ShoudlReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", movieId);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovieWithoutRequiredFields_ShouldReturnBadRequest()
        {
            MovieDTO movie = new MovieDTO()
            {
                Id = "",
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "123";

            var editedMovie = new MovieDTO()
            {
                Id = nonExistingMovieId,
                Title = "Dumb and Dumber 2",
                Description = "Second movie"
            };

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(editedMovie);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "1234";

            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", nonExistingMovieId);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));

        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client.Dispose();
        }
    }
}
